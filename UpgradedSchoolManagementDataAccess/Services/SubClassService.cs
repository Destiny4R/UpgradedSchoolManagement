using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class SubClassService : ISubClassService
    {
        private readonly ApplicationDbContext _context;

        public SubClassService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataTablesResponse<SubClassTable>> GetSubClasses(DataTablesRequest request)
        {
            try
            {
                var query = _context.SubClassTables.AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var searchValue = request.Search.Value.ToLower();
                    query = query.Where(sc => sc.Name.ToLower().Contains(searchValue));
                }

                var recordsFiltered = await query.CountAsync();

                if (request.Order != null && request.Order.Any())
                {
                    var order = request.Order.First();
                    var rawColumn = request.Columns != null && order.Column < request.Columns.Count
                        ? request.Columns[order.Column].Data ?? string.Empty
                        : string.Empty;

                    // Normalise camelCase → PascalCase to match EF Core property names
                    var columnName = rawColumn.Length > 0
                        ? char.ToUpper(rawColumn[0]) + rawColumn.Substring(1)
                        : string.Empty;

                    query = columnName switch
                    {
                        nameof(SubClassTable.Name) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(sc => sc.Name)
                            : query.OrderByDescending(sc => sc.Name),
                        nameof(SubClassTable.IsActive) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(sc => sc.IsActive)
                            : query.OrderByDescending(sc => sc.IsActive),
                        nameof(SubClassTable.CreatedDate) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(sc => sc.CreatedDate)
                            : query.OrderByDescending(sc => sc.CreatedDate),
                        _ => query.OrderBy(sc => sc.Name)
                    };
                }
                else
                {
                    query = query.OrderBy(sc => sc.Name);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .ToListAsync();

                return new DataTablesResponse<SubClassTable>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<SubClassTable>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<SubClassTable>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<SubClassTable?> GetSubClassById(int id)
        {
            try
            {
                return await _context.SubClassTables.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<SubClassTable>> CreateSubClass(string name)
        {
            try
            {
                var existingSubClass = await _context.SubClassTables
                    .FirstOrDefaultAsync(sc => sc.Name.ToLower() == name.ToLower());

                if (existingSubClass != null)
                {
                    return new ApiResponse<SubClassTable>
                    {
                        Success = false,
                        Message = "Sub-class with this name already exists"
                    };
                }

                var subClass = new SubClassTable
                {
                    Name = name.Trim(),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.SubClassTables.Add(subClass);
                await _context.SaveChangesAsync();

                return new ApiResponse<SubClassTable>
                {
                    Success = true,
                    Message = "Sub-class created successfully",
                    Data = subClass
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SubClassTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<SubClassTable>> UpdateSubClass(int id, string name)
        {
            try
            {
                var subClass = await _context.SubClassTables.FindAsync(id);
                if (subClass == null)
                {
                    return new ApiResponse<SubClassTable>
                    {
                        Success = false,
                        Message = "Sub-class not found"
                    };
                }

                var existingSubClass = await _context.SubClassTables
                    .FirstOrDefaultAsync(sc => sc.Name.ToLower() == name.ToLower() && sc.Id != id);

                if (existingSubClass != null)
                {
                    return new ApiResponse<SubClassTable>
                    {
                        Success = false,
                        Message = "Sub-class with this name already exists"
                    };
                }

                subClass.Name = name.Trim();
                subClass.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<SubClassTable>
                {
                    Success = true,
                    Message = "Sub-class updated successfully",
                    Data = subClass
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SubClassTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> DeleteSubClass(int id)
        {
            try
            {
                var subClass = await _context.SubClassTables.FindAsync(id);
                if (subClass == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Sub-class not found"
                    };
                }

                var hasRegistrations = await _context.TermRegistrations
                    .AnyAsync(tr => tr.SubClassId == id);

                if (hasRegistrations)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete sub-class with existing term registrations"
                    };
                }

                _context.SubClassTables.Remove(subClass);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Sub-class deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> ToggleSubClassStatus(int id)
        {
            try
            {
                var subClass = await _context.SubClassTables.FindAsync(id);
                if (subClass == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Sub-class not found"
                    };
                }

                subClass.IsActive = !subClass.IsActive;
                subClass.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = subClass.IsActive ? "Sub-class activated" : "Sub-class deactivated",
                    Data = subClass.IsActive
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<SubClassTable>> GetSubClasses()
        {
            try
            {
                return await _context.SubClassTables
                    .Where(sc => sc.IsActive)
                    .OrderBy(sc => sc.Name)
                    .ToListAsync();
            }
            catch
            {
                return new List<SubClassTable>();
            }
        }
    }
}
