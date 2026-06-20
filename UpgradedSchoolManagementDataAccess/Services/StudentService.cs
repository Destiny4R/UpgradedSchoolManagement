using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ── Admission number ──────────────────────────────────────────────────────
        public async Task<string> GenerateAdmissionNumber()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"EDU/STD/{year}/";

            // Find the highest sequence already used this year
            var lastSeq = await _context.StudentsTables
                .Where(s => s.AdmissionNumber != null && s.AdmissionNumber.StartsWith(prefix))
                .Select(s => s.AdmissionNumber!)
                .ToListAsync();                        // fetch strings, then parse in memory

            int nextNum = 1;
            if (lastSeq.Any())
            {
                nextNum = lastSeq
                    .Select(n =>
                    {
                        var part = n.Substring(prefix.Length);
                        return int.TryParse(part, out var v) ? v : 0;
                    })
                    .Max() + 1;
            }

            return $"{prefix}{nextNum:D3}";
        }

        // ── DataTables listing ────────────────────────────────────────────────────
        public async Task<DataTablesResponse<StudentDto>> GetStudents(DataTablesRequest request)
        {
            try
            {
                var query = _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var sv = request.Search.Value.ToLower();
                    query = query.Where(s =>
                        s.FirstName.ToLower().Contains(sv) ||
                        s.Surname.ToLower().Contains(sv) ||
                        (s.AdmissionNumber != null && s.AdmissionNumber.ToLower().Contains(sv)) ||
                        (s.OtherName != null && s.OtherName.ToLower().Contains(sv)) ||
                        (s.State != null && s.State.ToLower().Contains(sv)) ||
                        (s.ApplicationUser != null && s.ApplicationUser.Email != null && s.ApplicationUser.Email.ToLower().Contains(sv)));
                }

                var recordsFiltered = await query.CountAsync();

                if (request.Order != null && request.Order.Any())
                {
                    var order = request.Order.First();
                    var raw = request.Columns != null && order.Column < request.Columns.Count
                        ? request.Columns[order.Column].Data ?? string.Empty
                        : string.Empty;
                    var col = raw.Length > 0 ? char.ToUpper(raw[0]) + raw.Substring(1) : string.Empty;

                    query = col switch
                    {
                        "Surname" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.Surname)
                            : query.OrderByDescending(s => s.Surname),
                        "Gender" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.Gender)
                            : query.OrderByDescending(s => s.Gender),
                        "State" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.State)
                            : query.OrderByDescending(s => s.State),
                        "DateOfBirth" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.DateOfBirth)
                            : query.OrderByDescending(s => s.DateOfBirth),
                        _ => query.OrderBy(s => s.Surname)
                    };
                }
                else
                {
                    query = query.OrderBy(s => s.Surname);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        AdmissionNumber = s.AdmissionNumber,
                        FullName = $"{s.Surname} {s.FirstName} {s.OtherName ?? string.Empty}",
                        Gender = s.Gender.ToString(),
                        DateOfBirth = s.DateOfBirth.ToString("dd-MMM-yyyy"),
                        Nationality = s.Nationality,
                        State = s.State,
                        LocalGov = s.LocalGov,
                        PicturePath = s.PicturePath,
                        Email = s.ApplicationUser != null ? s.ApplicationUser.Email : null,
                        IsActive = s.ApplicationUser != null && s.ApplicationUser.IsActive,
                        CreatedDate = s.ApplicationUser != null ? s.ApplicationUser.CreatedDate : DateTime.UtcNow
                    })
                    .ToListAsync();

                return new DataTablesResponse<StudentDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<StudentDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<StudentDto>(),
                    Error = ex.Message
                };
            }
        }

        // ── Get by id ─────────────────────────────────────────────────────────────
        public async Task<StudentsTable?> GetStudentById(int id)
        {
            try
            {
                return await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch
            {
                return null;
            }
        }

        // ── Create ────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<StudentsTable>> CreateStudent(CreateStudentInput input)
        {
            try
            {
                var admissionNumber = await GenerateAdmissionNumber();

                // Use the admission number (EDU/STD/YYYY/NNN) as both UserName and Email.
                // '/' is permitted via AllowedUserNameCharacters; email format validation
                // is handled by AdmissionNumberEmailValidator (uniqueness only).
                var user = new ApplicationUser
                {
                    UserName  = admissionNumber,
                    Email     = admissionNumber,
                    FullName  = $"{input.FirstName} {input.Surname}",
                    IsActive  = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, input.Password);
                if (!result.Succeeded)
                {
                    return new ApiResponse<StudentsTable>
                    {
                        Success = false,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description))
                    };
                }

                await _userManager.AddToRoleAsync(user, "Student");

                var student = new StudentsTable
                {
                    AdmissionNumber = admissionNumber,
                    FirstName  = input.FirstName,
                    Surname    = input.Surname,
                    OtherName  = input.OtherName,
                    Gender     = (ConstantEnums.Gender)input.Gender,
                    DateOfBirth = input.DateOfBirth,
                    Nationality = input.Nationality,
                    State      = input.State,
                    LocalGov   = input.LocalGov,
                    Address    = input.Address,
                    PicturePath = input.PicturePath,
                    ApplicationUserId = user.Id
                };

                _context.StudentsTables.Add(student);
                await _context.SaveChangesAsync();

                return new ApiResponse<StudentsTable>
                {
                    Success = true,
                    Message = $"Student enrolled successfully. Admission No: {admissionNumber}",
                    Data = student
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentsTable> { Success = false, Message = ex.Message };
            }
        }

        // ── Update ────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<StudentsTable>> UpdateStudent(UpdateStudentInput input)
        {
            try
            {
                var student = await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.Id == input.Id);

                if (student == null)
                    return new ApiResponse<StudentsTable> { Success = false, Message = "Student not found" };

                student.FirstName   = input.FirstName;
                student.Surname     = input.Surname;
                student.OtherName   = input.OtherName;
                student.Gender      = (ConstantEnums.Gender)input.Gender;
                student.DateOfBirth = input.DateOfBirth;
                student.Nationality = input.Nationality;
                student.State       = input.State;
                student.LocalGov    = input.LocalGov;
                student.Address     = input.Address;

                if (!string.IsNullOrEmpty(input.PicturePath))
                    student.PicturePath = input.PicturePath;

                if (student.ApplicationUser != null)
                {
                    student.ApplicationUser.FullName    = $"{input.FirstName} {input.Surname}";
                    student.ApplicationUser.UpdatedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return new ApiResponse<StudentsTable>
                {
                    Success = true,
                    Message = "Student updated successfully",
                    Data = student
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentsTable> { Success = false, Message = ex.Message };
            }
        }

        // ── Get by UserId ──────────────────────────────────────────────────────────
        public async Task<StudentsTable?> GetStudentByUserId(string userId)
        {
            return await _context.StudentsTables
                .Include(s => s.ApplicationUser)
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
        }

        // ── Find by Admission Number ─────────────────────────────────────────────
        public async Task<StudentDto?> FindStudentByAdmissionNumberAsync(string admissionNumber)
        {
            try
            {
                var sv = admissionNumber.ToLower().Trim();
                var student = await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s =>
                        (s.AdmissionNumber != null && s.AdmissionNumber.ToLower() == sv) ||
                        (s.ApplicationUser != null && s.ApplicationUser.Email != null && s.ApplicationUser.Email.ToLower() == sv) ||
                        (s.ApplicationUser != null && s.ApplicationUser.UserName != null && s.ApplicationUser.UserName.ToLower() == sv));

                if (student == null) return null;

                return new StudentDto
                {
                    Id = student.Id,
                    AdmissionNumber = student.AdmissionNumber,
                    FullName = student.Surname + ", " + student.FirstName + (student.OtherName != null ? " " + student.OtherName : ""),
                    Gender = student.Gender.ToString(),
                    DateOfBirth = student.DateOfBirth.ToString("dd-MMM-yyyy"),
                    Nationality = student.Nationality,
                    State = student.State,
                    LocalGov = student.LocalGov,
                    PicturePath = student.PicturePath,
                    Email = student.ApplicationUser?.Email
                };
            }
            catch
            {
                return null;
            }
        }

        // ── Delete ────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<bool>> DeleteStudent(int id, string webRootPath)
        {
            try
            {
                var isRegistered = await _context.TermRegistrations.AnyAsync(tr => tr.StudentId == id);
                if (isRegistered)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete: student has term registration records."
                    };

                var student = await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                    return new ApiResponse<bool> { Success = false, Message = "Student not found" };

                // Remove parent links (keep parent record for other siblings)
                var parentLinks = await _context.StudentParentLinks
                    .Where(spl => spl.StudentId == id)
                    .ToListAsync();
                _context.StudentParentLinks.RemoveRange(parentLinks);

                // Delete picture file if present
                if (!string.IsNullOrEmpty(student.PicturePath))
                {
                    var fullPath = Path.Combine(webRootPath, student.PicturePath.TrimStart('/'));
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                // Terminate identity account
                if (student.ApplicationUser != null)
                    await _userManager.DeleteAsync(student.ApplicationUser);

                _context.StudentsTables.Remove(student);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Student deleted successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        // ── Reset Password ─────────────────────────────────────────────────────────
        public async Task<ApiResponse<object>> ResetStudentPassword(int studentId, string newPassword)
        {
            try
            {
                var student = await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                    return new ApiResponse<object> { Success = false, Message = "Student not found" };

                var user = student.ApplicationUser;
                if (user == null)
                    return new ApiResponse<object> { Success = false, Message = "Student has no linked user account" };

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (!result.Succeeded)
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description))
                    };

                return new ApiResponse<object> { Success = true, Message = "Password reset successfully" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, Message = ex.Message };
            }
        }

        // ── Toggle Active Status ────────────────────────────────────────────────────
        public async Task<ApiResponse<object>> ToggleStudentStatus(int studentId)
        {
            try
            {
                var student = await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                    return new ApiResponse<object> { Success = false, Message = "Student not found" };

                if (student.ApplicationUser == null)
                    return new ApiResponse<object> { Success = false, Message = "Student has no linked user account" };

                student.ApplicationUser.IsActive = !student.ApplicationUser.IsActive;
                student.ApplicationUser.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = student.ApplicationUser.IsActive ? "activated" : "deactivated";
                return new ApiResponse<object> { Success = true, Message = $"Student account {status} successfully" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, Message = ex.Message };
            }
        }
    }
}
