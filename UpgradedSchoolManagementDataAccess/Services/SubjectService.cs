using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ApplicationDbContext _context;

        public SubjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataTablesResponse<SubjectTable>> GetSubjects(DataTablesRequest request)
        {
            try
            {
                var query = _context.SubjectTables.AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var searchValue = request.Search.Value.ToLower();
                    query = query.Where(s => s.Name.ToLower().Contains(searchValue) ||
                                             (s.Code != null && s.Code.ToLower().Contains(searchValue)));
                }

                var recordsFiltered = await query.CountAsync();

                if (request.Order != null && request.Order.Any())
                {
                    var order = request.Order.First();
                    var rawColumn = request.Columns != null && order.Column < request.Columns.Count
                        ? request.Columns[order.Column].Data ?? string.Empty
                        : string.Empty;

                    var columnName = rawColumn.Length > 0
                        ? char.ToUpper(rawColumn[0]) + rawColumn.Substring(1)
                        : string.Empty;

                    query = columnName switch
                    {
                        nameof(SubjectTable.Name) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.Name)
                            : query.OrderByDescending(s => s.Name),
                        nameof(SubjectTable.Code) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.Code)
                            : query.OrderByDescending(s => s.Code),
                        nameof(SubjectTable.DisplayOrder) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.DisplayOrder)
                            : query.OrderByDescending(s => s.DisplayOrder),
                        nameof(SubjectTable.IsActive) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.IsActive)
                            : query.OrderByDescending(s => s.IsActive),
                        nameof(SubjectTable.CreatedDate) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.CreatedDate)
                            : query.OrderByDescending(s => s.CreatedDate),
                        _ => query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
                    };
                }
                else
                {
                    query = query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .ToListAsync();

                return new DataTablesResponse<SubjectTable>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<SubjectTable>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<SubjectTable>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<SubjectTable?> GetSubjectById(int id)
        {
            try
            {
                return await _context.SubjectTables.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<SubjectTable>> CreateSubject(string name, string? code)
        {
            try
            {
                var existingSubject = await _context.SubjectTables
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());

                if (existingSubject != null)
                {
                    return new ApiResponse<SubjectTable>
                    {
                        Success = false,
                        Message = "Subject with this name already exists"
                    };
                }

                if (!string.IsNullOrEmpty(code))
                {
                    var existingCode = await _context.SubjectTables
                        .FirstOrDefaultAsync(s => s.Code == code);

                    if (existingCode != null)
                    {
                        return new ApiResponse<SubjectTable>
                        {
                            Success = false,
                            Message = "Subject with this code already exists"
                        };
                    }
                }

                var subject = new SubjectTable
                {
                    Name = name.Trim(),
                    Code = code?.Trim(),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.SubjectTables.Add(subject);
                await _context.SaveChangesAsync();

                return new ApiResponse<SubjectTable>
                {
                    Success = true,
                    Message = "Subject created successfully",
                    Data = subject
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SubjectTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<SubjectTable>> UpdateSubject(int id, string name, string? code)
        {
            try
            {
                var subject = await _context.SubjectTables.FindAsync(id);
                if (subject == null)
                {
                    return new ApiResponse<SubjectTable>
                    {
                        Success = false,
                        Message = "Subject not found"
                    };
                }

                var existingSubject = await _context.SubjectTables
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower() && s.Id != id);

                if (existingSubject != null)
                {
                    return new ApiResponse<SubjectTable>
                    {
                        Success = false,
                        Message = "Subject with this name already exists"
                    };
                }

                if (!string.IsNullOrEmpty(code))
                {
                    var existingCode = await _context.SubjectTables
                        .FirstOrDefaultAsync(s => s.Code == code && s.Id != id);

                    if (existingCode != null)
                    {
                        return new ApiResponse<SubjectTable>
                        {
                            Success = false,
                            Message = "Subject with this code already exists"
                        };
                    }
                }

                subject.Name = name.Trim();
                subject.Code = code?.Trim();
                subject.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<SubjectTable>
                {
                    Success = true,
                    Message = "Subject updated successfully",
                    Data = subject
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SubjectTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> DeleteSubject(int id)
        {
            try
            {
                var subject = await _context.SubjectTables.FindAsync(id);
                if (subject == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Subject not found"
                    };
                }

                var hasResults = await _context.ResultTables.AnyAsync(r => r.SubjectId == id);
                if (hasResults)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete subject with existing results"
                    };
                }

                _context.SubjectTables.Remove(subject);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Subject deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> ToggleSubjectStatus(int id)
        {
            try
            {
                var subject = await _context.SubjectTables.FindAsync(id);
                if (subject == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Subject not found"
                    };
                }

                subject.IsActive = !subject.IsActive;
                subject.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = subject.IsActive ? "Subject activated" : "Subject deactivated",
                    Data = subject.IsActive
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }



        public IEnumerable<SubjectTable> GetAllActiveSubjects()
        {
            try
            {
                return _context.SubjectTables.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            }
            catch
            {
                return new List<SubjectTable>();
            }
        }
    }
}