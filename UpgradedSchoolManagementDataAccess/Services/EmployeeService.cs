using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<string> GenerateEmployeeCode()
        {
            var lastSeq = await _context.EmployeeTables
                .Where(e => e.EmployeeCode != null && e.EmployeeCode.StartsWith("EMP-"))
                .Select(e => e.EmployeeCode!)
                .ToListAsync();

            int nextNum = 1;
            if (lastSeq.Any())
            {
                nextNum = lastSeq
                    .Select(c =>
                    {
                        var part = c.Substring(4);
                        return int.TryParse(part, out var v) ? v : 0;
                    })
                    .Max() + 1;
            }

            return $"EMP-{nextNum:D4}";
        }

        public async Task<DataTablesResponse<EmployeeDto>> GetEmployees(DataTablesRequest request)
        {
            try
            {
                var query = _context.EmployeeTables
                    .Include(e => e.ApplicationUser)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var sv = request.Search.Value.ToLower();
                    query = query.Where(e =>
                        e.FullName.ToLower().Contains(sv) ||
                        (e.EmployeeCode != null && e.EmployeeCode.ToLower().Contains(sv)) ||
                        (e.EmployeeType != null && e.EmployeeType.ToLower().Contains(sv)) ||
                        (e.ApplicationUser != null && e.ApplicationUser.Email != null && e.ApplicationUser.Email.ToLower().Contains(sv)));
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
                        "FullName" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(e => e.FullName)
                            : query.OrderByDescending(e => e.FullName),
                        "EmployeeType" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(e => e.EmployeeType)
                            : query.OrderByDescending(e => e.EmployeeType),
                        "EmployeeCode" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(e => e.EmployeeCode)
                            : query.OrderByDescending(e => e.EmployeeCode),
                        _ => query.OrderByDescending(e => e.CreatedDate)
                    };
                }
                else
                {
                    query = query.OrderByDescending(e => e.CreatedDate);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        Gender = e.Gender.ToString(),
                        EmployeeType = e.EmployeeType,
                        EmployeeCode = e.EmployeeCode,
                        Email = e.ApplicationUser != null ? e.ApplicationUser.Email : null,
                        IsActive = e.ApplicationUser != null && e.ApplicationUser.IsActive,
                        CreatedDate = e.ApplicationUser != null ? e.ApplicationUser.CreatedDate : DateTime.UtcNow
                    })
                    .ToListAsync();

                return new DataTablesResponse<EmployeeDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<EmployeeDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<EmployeeDto>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<EmployeeTable?> GetEmployeeById(int id)
        {
            return await _context.EmployeeTables
                .Include(e => e.ApplicationUser)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<ApiResponse<EmployeeTable>> CreateEmployee(CreateEmployeeInput input)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employeeCode = await GenerateEmployeeCode();
                var user = new ApplicationUser
                {
                    UserName = employeeCode,
                    Email = employeeCode,
                    FullName = input.FullName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(user, input.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<EmployeeTable>
                    {
                        Success = false,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description))
                    };
                }
                var employee = new EmployeeTable
                {
                    FullName = input.FullName,
                    Gender = (ConstantEnums.Gender)input.Gender,
                    EmployeeType = input.EmployeeType,
                    Address = input.Address,
                    EmployeeCode = employeeCode,
                    ApplicationUserId = user.Id
                };
                _context.EmployeeTables.Add(employee);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<EmployeeTable>
                {
                    Success = true,
                    Message = $"Employee created successfully. Code: {employeeCode}",
                    Data = employee
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<EmployeeTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<EmployeeTable>> UpdateEmployee(UpdateEmployeeInput input)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = await _context.EmployeeTables
                    .Include(e => e.ApplicationUser)
                    .FirstOrDefaultAsync(e => e.Id == input.Id);

                if (employee == null)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<EmployeeTable> { Success = false, Message = "Employee not found" };
                }

                employee.FullName = input.FullName;
                employee.Gender = (ConstantEnums.Gender)input.Gender;
                employee.EmployeeType = input.EmployeeType;
                employee.Address = input.Address;

                if (employee.ApplicationUser != null)
                {
                    employee.ApplicationUser.FullName = input.FullName;
                    employee.ApplicationUser.UpdatedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<EmployeeTable>
                {
                    Success = true,
                    Message = "Employee updated successfully",
                    Data = employee
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<EmployeeTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _context.EmployeeTables
                    .Include(e => e.ApplicationUser)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee == null)
                    return new ApiResponse<bool> { Success = false, Message = "Employee not found" };

                var userId = employee.ApplicationUserId;
                _context.EmployeeTables.Remove(employee);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        foreach (var r in roles)
                            await _userManager.RemoveFromRoleAsync(user, r);
                        await _userManager.DeleteAsync(user);
                    }
                }

                return new ApiResponse<bool> { Success = true, Message = "Employee deleted successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
    }
}
