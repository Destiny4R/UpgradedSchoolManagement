using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.PermissionConstants;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class TermRegistrationServices : ITermRegistrationServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TermRegistrationServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public TermRegistrationServices(
            ApplicationDbContext context,
            ILogger<TermRegistrationServices> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        //public async Task<StudentViewModel> GetStudentByUsernameAsync(string username)
        //{
        //    try
        //    {
        //        var student = await _context.Students
        //            .Include(s => s.ApplicationUser)
        //            .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == username);

        //        if (student == null)
        //            return null;

        //        return new StudentViewModel
        //        {
        //            Id = student.Id,
        //            Surname = student.Surname,
        //            Firstname = student.Firstname,
        //            Othername = student.Othername,
        //            GenderId = (int)student.Gender,
        //            ApplicationUserId = student.ApplicationUserId
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error getting student by username: {ex.Message}");
        //        return null;
        //    }
        //}

        public async Task<(List<TermRegDto> data, int recordsTotal, int recordsFiltered)> GetStudentTermRegistrationAsync(
            int    skip           = 0,
            int    pageSize       = 10,
            string searchTerm     = "",
            int    sortColumn     = 0,
            string sortDirection  = "asc",
            int?   termFilter     = null,
            int?   sessionFilter  = null,
            int?   classFilter    = null,
            int?   subclassFilter = null)
        {
            try
            {
                var query = _context.TermRegistrations
                    .Include(tr => tr.StudentsTable.ApplicationUser)
                    .Include(tr => tr.SchoolClasses)
                    .Include(tr => tr.SubClassTable)
                    .Include(tr => tr.SesseionTable)
                    .Include(tr => tr.ResultTable)
                    .AsNoTracking()
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                // Search across First, Surname, OtherName, RegNumber/Username, Term, Session, Class, SubClass
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(x =>
                        x.StudentsTable.FirstName.Contains(searchTerm) ||
                        x.StudentsTable.Surname.Contains(searchTerm) ||
                        (x.StudentsTable.OtherName != null && x.StudentsTable.OtherName.Contains(searchTerm)) ||
                        (x.StudentsTable.ApplicationUser != null && x.StudentsTable.ApplicationUser.UserName != null && x.StudentsTable.ApplicationUser.UserName.Contains(searchTerm)) ||
                        x.SchoolClasses.Name.Contains(searchTerm) ||
                        x.SubClassTable.Name.Contains(searchTerm) ||
                        x.SesseionTable.Name.Contains(searchTerm));
                }

                // Apply dropdown filters
                if (termFilter.HasValue && termFilter > 0)
                    query = query.Where(x => (int)x.Term == termFilter.Value);

                if (sessionFilter.HasValue && sessionFilter > 0)
                    query = query.Where(x => x.SessionId == sessionFilter.Value);

                if (classFilter.HasValue && classFilter > 0)
                    query = query.Where(x => x.SchoolClassId == classFilter.Value);

                if (subclassFilter.HasValue && subclassFilter > 0)
                    query = query.Where(x => x.SubClassId == subclassFilter.Value);

                var recordsFiltered = await query.CountAsync();

                // Sorting
                query = sortColumn switch
                {
                    2 => sortDirection == "desc"
                        ? query.OrderByDescending(x => x.StudentsTable.Surname)
                        : query.OrderBy(x => x.StudentsTable.Surname),
                    3 => sortDirection == "desc"
                        ? query.OrderByDescending(x => x.StudentsTable.ApplicationUser.UserName)
                        : query.OrderBy(x => x.StudentsTable.ApplicationUser.UserName),
                    4 => sortDirection == "desc"
                        ? query.OrderByDescending(x => x.Term)
                        : query.OrderBy(x => x.Term),
                    5 => sortDirection == "desc"
                        ? query.OrderByDescending(x => x.SesseionTable.Name)
                        : query.OrderBy(x => x.SesseionTable.Name),
                    6 => sortDirection == "desc"
                        ? query.OrderByDescending(x => x.SchoolClasses.Name)
                        : query.OrderBy(x => x.SchoolClasses.Name),
                    _ => sortDirection == "desc"
                        ? query.OrderByDescending(x => x.CreatedDate)
                        : query.OrderBy(x => x.CreatedDate)
                };

                var data = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new TermRegDto
                    {
                        Id                = x.Id,
                        StudentId         = x.StudentId,
                        FirstName         = x.StudentsTable.FirstName,
                        Surname           = x.StudentsTable.Surname,
                        OtherName         = x.StudentsTable.OtherName,
                        FullName          = x.StudentsTable.FirstName + " " + x.StudentsTable.Surname +
                                            (x.StudentsTable.OtherName != null ? " " + x.StudentsTable.OtherName : ""),
                        RegNumber         = x.StudentsTable.ApplicationUser != null ? x.StudentsTable.ApplicationUser.UserName : x.StudentsTable.AdmissionNumber,
                        Term              = x.Term.ToString(),
                        Session           = x.SesseionTable.Name,
                        SchoolClass       = x.SchoolClasses.Name,
                        SubClass          = x.SubClassTable.Name,
                        CreatedDate       = x.CreatedDate,
                        NoOfSubjects      = x.ResultTable.Count,
                        HasRecordedResults = _context.ResultTables.Any(rt => rt.TermRegId == x.Id && rt.Status == true)
                    })
                    .ToListAsync();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting term registrations");
                return (new List<TermRegDto>(), 0, 0);
            }
        }

        public async Task<TermRegistrationViewModel> GetStudentTermRegistrationByIdAsync(int id)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .Include(tr => tr.StudentsTable.ApplicationUser)
                    .Include(tr => tr.ResultTable).ThenInclude(rt => rt.Subject)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (termReg == null)
                    return null;

                return new TermRegistrationViewModel
                {
                    Id = (int)termReg.Id,
                    SchoolClassId = termReg.SchoolClassId,
                    SessionId = termReg.SessionId,
                    Term = termReg.Term,
                    SchoolSubclassId = termReg.SubClassId,
                    StudentId = termReg.StudentId,
                    StudentName = $"{termReg.StudentsTable.FirstName} {termReg.StudentsTable.Surname}",
                    StudentRegNumber = termReg.StudentsTable.ApplicationUser.UserName.ToString(),
                    RegisteredSubjects = termReg.ResultTable.Select(rt => new RegisteredSubjectDto
                    {
                        ResultTableId = rt.Id,
                        SubjectName = rt.Subject?.Name ?? "Unknown",
                        HasRecordedResults = rt.Status
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting term registration by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<AttendanceViewModel>> GetAllStudentAttendanceTermRegistrationsAsync(TermObjects model)
        {
            try
            {
                var results = await _context.TermRegistrations
                    .AsNoTracking()
                    .Where(tr =>
                        tr.SchoolClassId == model.schoolClassId &&
                        tr.SubClassId == model.SubclassId &&
                        tr.SessionId == model.SessionId &&
                        tr.Term == model.Term)
                    .OrderBy(tr => tr.StudentsTable.Surname)
                    .ThenBy(tr => tr.StudentsTable.FirstName)
                    .Select(tr => new AttendanceViewModel
                    {
                        Id = tr.Id,
                        FullName = tr.StudentsTable != null ? tr.StudentsTable.FullName : string.Empty,
                        RegNumber = tr.StudentsTable != null ? tr.StudentsTable.AdmissionNumber : string.Empty,
                        Term = tr.Term.ToString(),
                        Session = tr.SesseionTable != null ? tr.SesseionTable.Name : string.Empty,
                        SchoolClass = tr.SchoolClasses != null && tr.SubClassTable != null
                            ? $"{tr.SchoolClasses.Name} - {tr.SubClassTable.Name}"
                            : string.Empty,
                        StudentAttendance = (int)(tr.Attendance ?? 0),
                        SchoolAttendance = 0
                    }).ToListAsync();


                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance registrations for Class {ClassId}, Term {Term}",
                    model.schoolClassId, model.Term);
                return null;
            }
        }

        public async Task<ApiResponse<int>> CreateStudentTermRegistrationAsync(TermRegistrationViewModel model)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Ensure assessment configuration exists
                var hasAssessmentConfig = await _context.AssessmentConfigurations
                    .AnyAsync(x => x.SchoolClassId == model.SchoolClassId);

                if (!hasAssessmentConfig)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "No assessment configuration found for the selected class. Please set up assessment configurations before registering students.",
                        Data = 0
                    };
                }

                // Validate student
                var studentExists = await _context.StudentsTables
                    .AnyAsync(x => x.Id == model.StudentId);

                if (!studentExists)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Student not found",
                        Data = 0
                    };
                }

                // Validate class
                var classExists = await _context.SchoolClasses
                    .AnyAsync(x => x.Id == model.SchoolClassId);

                if (!classExists)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "School class not found",
                        Data = 0
                    };
                }

                // Validate session
                var sessionExists = await _context.SesseionTables
                    .AnyAsync(x => x.Id == model.SessionId);

                if (!sessionExists)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Academic session not found",
                        Data = 0
                    };
                }

                // Validate subclass
                var subClassExists = await _context.SubClassTables
                    .AnyAsync(x => x.Id == model.SchoolSubclassId);

                if (!subClassExists)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "School subclass not found",
                        Data = 0
                    };
                }

                // Prevent duplicate registration
                var alreadyRegistered = await _context.TermRegistrations
                    .AnyAsync(x =>
                        x.StudentId == model.StudentId &&
                        x.SessionId == model.SessionId &&
                        x.SchoolClassId == model.SchoolClassId &&
                        x.SubClassId == model.SchoolSubclassId &&
                        x.Term == model.Term);

                if (alreadyRegistered)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Student is already registered for this class in this academic session and term.",
                        Data = 0
                    };
                }

                var termRegistration = new TermRegistration
                {
                    StudentId = model.StudentId,
                    SchoolClassId = model.SchoolClassId,
                    SubClassId = model.SchoolSubclassId,
                    SessionId = model.SessionId,
                    Term = model.Term,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                termRegistration.StudentRatings = new StudentRating
                {
                    TermRegId = (int)termRegistration.Id
                };
                termRegistration.ResultTable = new List<ResultTable>();
                if (model.SubjectsId?.Any() == true)
                {
                    _logger.LogInformation("Selected Subjects: {Subjects}", string.Join(",", model.SubjectsId));

                    foreach (var subjectId in model.SubjectsId.Distinct())
                    {
                        termRegistration.ResultTable.Add(new ResultTable
                        {
                            SubjectId = subjectId,
                            Status = false,
                            TermRegId = termRegistration.Id
                        });
                    }
                }


                await _context.TermRegistrations.AddAsync(termRegistration);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Term registration created successfully. RegistrationId: {RegistrationId}, StudentId: {StudentId}",
                    termRegistration.Id,
                    model.StudentId);

                return new ApiResponse<int>
                {
                    Success = true,
                    Message = "Term registration created successfully",
                    Data = (int)termRegistration.Id
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error creating term registration for StudentId: {StudentId}",
                    model.StudentId);

                return new ApiResponse<int>
                {
                    Success = false,
                    Message = "An error occurred",
                    Data = 0
                };
            }
        }

        public async Task<ApiResponse<int>> UpdateStudentTermRegistrationAsync(TermRegistrationViewModel model)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var termReg = await _context.TermRegistrations.FindAsync(model.Id);

                if (termReg == null)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Term registration not found",
                        Data = 0
                    };
                }

                // Validate school class
                var schoolClassExists = await _context.SchoolClasses.AnyAsync(x => x.Id == model.SchoolClassId);

                if (!schoolClassExists)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "School class not found",
                        Data = 0
                    };
                }

                // Validate subclass
                var subClassExists = await _context.SubClassTables.AnyAsync(x => x.Id == model.SchoolSubclassId);

                if (!subClassExists)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "School subclass not found",
                        Data = 0
                    };
                }

                // Optional: Validate session
                var sessionExists = await _context.SesseionTables.AnyAsync(x => x.Id == model.SessionId);

                if (!sessionExists)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Session not found",
                        Data = 0
                    };
                }

                // Check if changing class
                if (termReg.SchoolClassId != model.SchoolClassId)
                {
                    var conflictingClassReg = await _context.TermRegistrations
                        .AnyAsync(x =>
                            x.StudentId == termReg.StudentId &&
                            x.SessionId == model.SessionId &&
                            x.Id != termReg.Id);

                    if (conflictingClassReg)
                    {
                        return new ApiResponse<int>
                        {
                            Success = false,
                            Message = "Cannot change class. Student is already registered for another class in this session.",
                            Data = 0
                        };
                    }
                }

                // Update registration
                termReg.SchoolClassId = model.SchoolClassId;
                termReg.SessionId = model.SessionId;
                termReg.SubClassId = model.SchoolSubclassId;
                termReg.Term = model.Term;
                termReg.UpdatedDate = DateTime.UtcNow;

                // Add newly selected subjects only
                if (model.SubjectsId?.Any() == true)
                {
                    var existingSubjectIds = await _context.ResultTables.Where(x => x.TermRegId == termReg.Id).Select(x => x.SubjectId).ToListAsync();

                    var newSubjects = model.SubjectsId
                        .Distinct()
                        .Where(subjectId => !existingSubjectIds.Contains(subjectId))
                        .Select(subjectId => new ResultTable
                        {
                            SubjectId = subjectId,
                            TermRegId = termReg.Id,
                            Status = false
                        }).ToList();

                    if (newSubjects.Any())
                    {
                        await _context.ResultTables.AddRangeAsync(newSubjects);
                    }
                }

                _context.TermRegistrations.Update(termReg);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Term registration updated successfully for ID: {RegistrationId}",
                    model.Id);

                return new ApiResponse<int>
                {
                    Success = true,
                    Message = "Term registration updated successfully",
                    Data = 0
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error updating term registration for ID: {RegistrationId}",
                    model.Id);

                return new ApiResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred - {ex.Message}",
                    Data = 0
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteStudentTermRegistrationAsync(int id)
        {
            try
            {
                var termReg = await _context.TermRegistrations.FindAsync((long)id);
                if (termReg == null)
                    return new ApiResponse<bool> { Success = false, Message = "Term registration not found", Data = false };

                // Block delete if any linked ResultTable row has Status == true (results recorded)
                var hasRecordedResults = await _context.ResultTables
                    .AnyAsync(rt => rt.TermRegId == termReg.Id && rt.Status == true);

                if (hasRecordedResults)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete this registration because results have already been recorded for it.",
                        Data = false
                    };

                // Remove linked ResultTable rows first
                var resultRows = _context.ResultTables.Where(rt => rt.TermRegId == termReg.Id);
                _context.ResultTables.RemoveRange(resultRows);

                _context.TermRegistrations.Remove(termReg);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Term registration deleted successfully for ID: {Id}", id);
                return new ApiResponse<bool> { Success = true, Message = "Term registration deleted successfully", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting term registration ID: {Id}", id);
                return new ApiResponse<bool> { Success = false, Message = "An error occurred while deleting.", Data = false };
            }
        }

        public async Task<ApiResponse<bool>> RemoveSubjectFromResultTableAsync(long resultTableId)
        {
            try
            {
                var resultRow = await _context.ResultTables.FindAsync(resultTableId);
                if (resultRow == null)
                {
                    return new ApiResponse<bool> { Success = false, Message = "Subject record not found", Data = false };
                }

                if (resultRow.Status)
                {
                    return new ApiResponse<bool> 
                    { 
                        Success = false, 
                        Message = "Cannot remove this subject because results have already been recorded for it.", 
                        Data = false 
                    };
                }

                _context.ResultTables.Remove(resultRow);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subject result row deleted successfully for ResultTableId: {Id}", resultTableId);
                return new ApiResponse<bool> { Success = true, Message = "Subject removed successfully", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing subject ResultTableId: {Id}", resultTableId);
                return new ApiResponse<bool> { Success = false, Message = "An error occurred while removing the subject.", Data = false };
            }
        }

        public async Task<ApiResponse<bool>> DeleteStudentsTermRegistrationAsync(List<int> ids)
        {
            try
            {
                var longIds = ids.Select(i => (long)i).ToList();

                var termRegs = await _context.TermRegistrations
                    .Where(x => longIds.Contains(x.Id))
                    .ToListAsync();

                if (!termRegs.Any())
                    return new ApiResponse<bool> { Success = false, Message = "No term registrations found to delete", Data = false };

                // Block bulk delete if any registration has recorded results (Status == true)
                var idsWithResults = await _context.ResultTables
                    .Where(rt => longIds.Contains(rt.TermRegId) && rt.Status == true)
                    .Select(rt => rt.TermRegId)
                    .Distinct()
                    .ToListAsync();

                if (idsWithResults.Any())
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Cannot delete {idsWithResults.Count} registration(s) because results have already been recorded for them. Please deselect those registrations and try again.",
                        Data = false
                    };

                // Remove linked ResultTable rows first
                var resultRows = _context.ResultTables.Where(rt => longIds.Contains(rt.TermRegId));
                _context.ResultTables.RemoveRange(resultRows);

                _context.TermRegistrations.RemoveRange(termRegs);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk deleted {Count} term registrations", termRegs.Count);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Successfully deleted {termRegs.Count} term registration(s)",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting term registrations");
                return new ApiResponse<bool> { Success = false, Message = "An error occurred while deleting.", Data = false };
            }
        }

        public async Task<BatchRegistrationResult> BatchRegisterStudentsAsync(
            List<string> registrationNumbers, 
            int classId, 
            int sessionId, 
            int subClassId, 
            Term term, 
            List<int>? subjectIds)
        {
            var result = new BatchRegistrationResult { TotalProcessed = registrationNumbers.Count };

            if (!registrationNumbers.Any())
            {
                result.Message = "No registration numbers provided.";
                return result;
            }

            // Clean up the list
            registrationNumbers = registrationNumbers
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            // Find matching students
            var students = await _context.StudentsTables
                .Include(s => s.ApplicationUser)
                .Where(s => registrationNumbers.Contains(s.AdmissionNumber) || 
                            (s.ApplicationUser != null && registrationNumbers.Contains(s.ApplicationUser.UserName)))
                .ToListAsync();

            foreach (var regNum in registrationNumbers)
            {
                var student = students.FirstOrDefault(s => s.AdmissionNumber == regNum || (s.ApplicationUser != null && s.ApplicationUser.UserName == regNum));
                
                if (student == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Registration Number '{regNum}': Student not found.");
                    continue;
                }

                // Prepare model for existing Create logic
                var model = new TermRegistrationViewModel
                {
                    StudentId = student.Id,
                    SchoolClassId = classId,
                    SessionId = sessionId,
                    SchoolSubclassId = subClassId,
                    Term = term,
                    SubjectsId = subjectIds ?? new List<int>()
                };

                // Create registration using the existing validation logic
                var createResult = await CreateStudentTermRegistrationAsync(model);

                if (createResult.Success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    result.FailureCount++;
                    result.Errors.Add($"Registration Number '{regNum}': {createResult.Message}");
                }
            }

            result.Success = result.FailureCount == 0;
            result.Message = $"Successfully registered {result.SuccessCount} students. Failed: {result.FailureCount}.";
            
            return result;
        }

        //public async Task<bool> HasPaymentForTermRegistrationAsync(int termRegId)
        //{
        //    return await _context.StudentPayments.AnyAsync(sp => sp.TermRegId == termRegId);
        //}

        public async Task<List<TermRegDto>> GetStudentsForBatchPromotionAsync(int term, int sessionId, int classId, int subclassId)
        {
            try
            {
                var data = await _context.TermRegistrations
                    .Include(tr => tr.StudentsTable.ApplicationUser)
                    .Include(tr => tr.SesseionTable)
                    .Include(tr => tr.SchoolClasses)
                    .Include(tr => tr.SubClassTable)
                    .Include(tr => tr.ResultTable) // Include this so we know how many subjects they have
                    .Where(x => (int)x.Term == term && x.SessionId == sessionId && x.SchoolClassId == classId && x.SubClassId == subclassId)
                    .Select(x => new TermRegDto
                    {
                        Id = x.Id,
                        StudentId = x.StudentId,
                        FirstName = x.StudentsTable.FirstName,
                        Surname = x.StudentsTable.Surname,
                        OtherName = x.StudentsTable.OtherName,
                        FullName = x.StudentsTable.FirstName + " " + x.StudentsTable.Surname +
                                  (x.StudentsTable.OtherName != null ? " " + x.StudentsTable.OtherName : ""),
                        RegNumber = x.StudentsTable.ApplicationUser != null ? x.StudentsTable.ApplicationUser.UserName : x.StudentsTable.AdmissionNumber,
                        Term = x.Term.ToString(),
                        Session = x.SesseionTable.Name,
                        SchoolClass = x.SchoolClasses.Name,
                        SubClass = x.SubClassTable.Name,
                        CreatedDate = x.CreatedDate,
                        NoOfSubjects = x.ResultTable.Count
                    })
                    .ToListAsync();
                
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching students for batch promotion");
                return new List<TermRegDto>();
            }
        }

        public async Task<List<TermRegDto>> GetStudentTermRegistrationsAsync(int studentId)
        {
            try
            {
                return await _context.TermRegistrations
                    .Include(tr => tr.SesseionTable)
                    .Include(tr => tr.SchoolClasses)
                    .Include(tr => tr.SubClassTable)
                    .Include(tr => tr.ResultTable)
                    .Where(tr => tr.StudentId == studentId)
                    .OrderByDescending(tr => tr.SessionId)
                    .ThenBy(tr => tr.Term)
                    .Select(x => new TermRegDto
                    {
                        Id = x.Id,
                        StudentId = x.StudentId,
                        SessionId = x.SessionId,
                        SchoolClassId = x.SchoolClassId,
                        SubClassId = x.SubClassId,
                        FirstName = x.StudentsTable.FirstName,
                        Surname = x.StudentsTable.Surname,
                        OtherName = x.StudentsTable.OtherName,
                        FullName = x.StudentsTable.FirstName + " " + x.StudentsTable.Surname +
                                  (x.StudentsTable.OtherName != null ? " " + x.StudentsTable.OtherName : ""),
                        RegNumber = x.StudentsTable.AdmissionNumber ?? "",
                        Term = x.Term.ToString(),
                        Session = x.SesseionTable.Name,
                        SchoolClass = x.SchoolClasses.Name,
                        SubClass = x.SubClassTable.Name,
                        CreatedDate = x.CreatedDate,
                        NoOfSubjects = x.ResultTable.Count,
                        ResultType = (int)x.SchoolClasses.Resulttype
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching term registrations for student {StudentId}", studentId);
                return new List<TermRegDto>();
            }
        }

        public async Task<BatchRegistrationResult> BatchPromoteStudentsAsync(
            List<long> previousTermRegIds, 
            int classId, 
            int sessionId, 
            int subClassId, 
            Term term)
        {
            var result = new BatchRegistrationResult { TotalProcessed = previousTermRegIds.Count };

            if (!previousTermRegIds.Any())
            {
                result.Message = "No students selected for promotion.";
                return result;
            }

            // Fetch the old registrations along with their subjects and student data
            var previousRegistrations = await _context.TermRegistrations
                .Include(tr => tr.ResultTable)
                .Include(tr => tr.StudentsTable.ApplicationUser)
                .Where(tr => previousTermRegIds.Contains(tr.Id))
                .ToListAsync();

            foreach (var oldReg in previousRegistrations)
            {
                var regNum = oldReg.StudentsTable.ApplicationUser?.UserName ?? oldReg.StudentsTable.AdmissionNumber;

                // Prepare model for existing Create logic
                var model = new TermRegistrationViewModel
                {
                    StudentId = oldReg.StudentId,
                    SchoolClassId = classId,
                    SessionId = sessionId,
                    SchoolSubclassId = subClassId,
                    Term = term,
                    // Inherit subjects from the previous registration
                    SubjectsId = oldReg.ResultTable.Select(rt => rt.SubjectId).ToList()
                };

                // Create registration using the existing robust validation logic
                var createResult = await CreateStudentTermRegistrationAsync(model);

                if (createResult.Success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    result.FailureCount++;
                    result.Errors.Add($"Student '{oldReg.StudentsTable.FirstName} {oldReg.StudentsTable.Surname}' ({regNum}): {createResult.Message}");
                }
            }

            result.Success = result.FailureCount == 0;
            result.Message = $"Successfully promoted {result.SuccessCount} students. Failed: {result.FailureCount}.";
            
            return result;
        }
        public async Task<ApiResponse<bool>> UpdateStudentAttendanceAsync(List<AttendanceViewModel> attendanceUpdates,TermObjects termObjects)
        {
            if (attendanceUpdates == null || attendanceUpdates.Count == 0)
                return new ApiResponse<bool> { Success = false, Message = "No attendance updates provided." };

            try
            {
                // Build a lookup for O(1) access instead of repeated FirstOrDefault
                var updateMap = attendanceUpdates.ToDictionary(a => a.Id, a => a.StudentAttendance);

                if (updateMap.Count == 0)
                    return new ApiResponse<bool> { Success = false, Message = "No valid attendance values in the update list." };

                // Only fetch the columns we need — no includes, no full entity load
                var matchingRegs = await _context.TermRegistrations
                    .Where(tr =>
                        tr.SchoolClassId == termObjects.schoolClassId &&
                        tr.SubClassId == termObjects.SubclassId &&
                        tr.SessionId == termObjects.SessionId &&
                        tr.Term == termObjects.Term &&
                        updateMap.Keys.Contains(tr.Id))
                    .ToListAsync();

                if (matchingRegs.Count == 0)
                    return new ApiResponse<bool> { Success = false, Message = "No matching term registrations found." };

                foreach (var reg in matchingRegs)
                {
                    if (updateMap.TryGetValue(reg.Id, out var newAttendance))
                        reg.Attendance = newAttendance;
                }

                await _context.SaveChangesAsync();
                return new ApiResponse<bool> { Success = true, Message = "Students attendance successfully updated!" };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating attendance for Class {ClassId}, Term {Term}",
                    termObjects.schoolClassId, termObjects.Term);
                return new ApiResponse<bool> { Success = false, Message = "A database error occurred while saving attendance." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating attendance for Class {ClassId}, Term {Term}",
                    termObjects.schoolClassId, termObjects.Term);
                return new ApiResponse<bool> { Success = false, Message = "An unexpected error occurred while updating attendance." };
            }
        }
    }
}
