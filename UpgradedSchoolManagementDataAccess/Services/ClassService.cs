using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class ClassService : IClassService
    {
        private readonly ApplicationDbContext _context;

        public ClassService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════════════════════════════════
        // SCHOOL CLASS CRUD
        // ════════════════════════════════════════════════════════════════════════

        public async Task<DataTablesResponse<SchoolClasses>> GetClasses(DataTablesRequest request)
        {
            try
            {
                var query = _context.SchoolClasses.AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var searchValue = request.Search.Value.ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(searchValue));
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
                        nameof(SchoolClasses.Name) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name),
                        nameof(SchoolClasses.DisplayOrder) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(c => c.DisplayOrder)
                            : query.OrderByDescending(c => c.DisplayOrder),
                        nameof(SchoolClasses.IsActive) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(c => c.IsActive)
                            : query.OrderByDescending(c => c.IsActive),
                        nameof(SchoolClasses.Resulttype) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(c => c.Resulttype)
                            : query.OrderByDescending(c => c.Resulttype),
                        nameof(SchoolClasses.CreatedDate) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(c => c.CreatedDate)
                            : query.OrderByDescending(c => c.CreatedDate),
                        _ => query.OrderBy(c => c.Name)
                    };
                }
                else
                {
                    query = query.OrderBy(c => c.Name);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .ToListAsync();

                return new DataTablesResponse<SchoolClasses>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<SchoolClasses>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<SchoolClasses>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<SchoolClasses?> GetClassById(int id)
        {
            try
            {
                return await _context.SchoolClasses.FirstOrDefaultAsync(c => c.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<SchoolClasses>> CreateClass(string name, int displayOrder = 0, int resultType = 0)
        {
            try
            {
                var existingClass = await _context.SchoolClasses
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

                if (existingClass != null)
                    return new ApiResponse<SchoolClasses> { Success = false, Message = "Class with this name already exists" };

                var schoolClass = new SchoolClasses
                {
                    Name = name.Trim(),
                    DisplayOrder = displayOrder,
                    Resulttype = (ResultType)resultType,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.SchoolClasses.Add(schoolClass);
                await _context.SaveChangesAsync();

                return new ApiResponse<SchoolClasses> { Success = true, Message = "Class created successfully", Data = schoolClass };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SchoolClasses> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<SchoolClasses>> UpdateClass(int id, string name, int displayOrder, int resultType = 0)
        {
            try
            {
                var schoolClass = await _context.SchoolClasses.FindAsync(id);
                if (schoolClass == null)
                    return new ApiResponse<SchoolClasses> { Success = false, Message = "Class not found" };

                var existingClass = await _context.SchoolClasses
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != id);

                if (existingClass != null)
                    return new ApiResponse<SchoolClasses> { Success = false, Message = "Class with this name already exists" };

                schoolClass.Name = name.Trim();
                schoolClass.DisplayOrder = displayOrder;
                schoolClass.Resulttype = (ResultType)resultType;
                schoolClass.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<SchoolClasses> { Success = true, Message = "Class updated successfully", Data = schoolClass };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SchoolClasses> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> DeleteClass(int id)
        {
            try
            {
                var schoolClass = await _context.SchoolClasses.FindAsync(id);
                if (schoolClass == null)
                    return new ApiResponse<bool> { Success = false, Message = "Class not found" };

                var hasTermRegs = await _context.TermRegistrations.AnyAsync(sc => sc.SchoolClassId == id);
                if (hasTermRegs)
                    return new ApiResponse<bool> { Success = false, Message = "Cannot delete class with student term registration" };

                _context.SchoolClasses.Remove(schoolClass);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Class deleted successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> ToggleClassStatus(int id)
        {
            try
            {
                var schoolClass = await _context.SchoolClasses.FindAsync(id);
                if (schoolClass == null)
                    return new ApiResponse<bool> { Success = false, Message = "Class not found" };

                schoolClass.IsActive = !schoolClass.IsActive;
                schoolClass.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = schoolClass.IsActive ? "Class activated" : "Class deactivated",
                    Data = schoolClass.IsActive
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // ASSESSMENT CONFIGURATION
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns a DataTables-paged list of AssessmentConfiguration rows joined with SchoolClasses.
        /// If classId == 0, returns configs for ALL classes; otherwise filters by classId.
        /// </summary>
        public async Task<DataTablesResponse<AssessmentConfigDto>> GetClassAssessmentConfigs(
            DataTablesRequest request, int classId)
        {
            try
            {
                var query = _context.AssessmentConfigurations
                    .Include(a => a.SchoolClasses)
                    .AsQueryable();

                if (classId > 0)
                    query = query.Where(a => a.SchoolClassId == classId);

                var recordsTotal = await query.CountAsync();

                // Search
                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var sv = request.Search.Value.ToLower();
                    query = query.Where(a =>
                        a.AssessmentName.ToLower().Contains(sv) ||
                        a.SchoolClasses.Name.ToLower().Contains(sv));
                }

                var recordsFiltered = await query.CountAsync();

                // Ordering
                query = query.OrderBy(a => a.SchoolClasses.Name)
                             .ThenBy(a => a.DisplayOrder);

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .Select(a => new AssessmentConfigDto
                    {
                        Id = a.Id,
                        AssessmentName = a.AssessmentName,
                        AssessmentScore = a.AssessmentScore,
                        DisplayOrder = a.DisplayOrder,
                        ClassName = a.SchoolClasses.Name,
                        SchoolClassId = a.SchoolClassId,
                        CreatedDate = a.CreatedDate,
                        UpdatedDate = a.UpdatedDate
                    })
                    .ToListAsync();

                return new DataTablesResponse<AssessmentConfigDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<AssessmentConfigDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<AssessmentConfigDto>(),
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Returns all assessment configs for a single class (to pre-populate the edit modal).
        /// Results are ordered by DisplayOrder ascending.
        /// </summary>
        public async Task<List<AssessmentConfigDto>> GetAssessmentConfigsByClassId(int classId)
        {
            try
            {
                return await _context.AssessmentConfigurations
                    .Where(a => a.SchoolClassId == classId)
                    .OrderBy(a => a.DisplayOrder)
                    .Select(a => new AssessmentConfigDto
                    {
                        Id = a.Id,
                        AssessmentName = a.AssessmentName,
                        AssessmentScore = a.AssessmentScore,
                        DisplayOrder = a.DisplayOrder,
                        ClassName = a.SchoolClasses.Name,
                        SchoolClassId = a.SchoolClassId,
                        CreatedDate = a.CreatedDate,
                        UpdatedDate = a.UpdatedDate
                    })
                    .ToListAsync();
            }
            catch
            {
                return new List<AssessmentConfigDto>();
            }
        }

        /// <summary>
        /// Saves (replace-all) the given assessment list for every classId supplied.
        /// Existing configs for each class are deleted in the same transaction before inserting the new set.
        /// </summary>
        public async Task<ApiResponse<bool>> SaveAssessmentConfigs(
            List<int> classIds,
            List<ResultConfigAssessmentViewModel> assessments)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (classIds == null || !classIds.Any())
                    return new ApiResponse<bool> { Success = false, Message = "No classes selected." };

                if (assessments == null || !assessments.Any())
                    return new ApiResponse<bool> { Success = false, Message = "No assessments provided." };

                foreach (var classId in classIds)
                {
                    // Verify the class exists
                    var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == classId);
                    if (!classExists)
                        return new ApiResponse<bool> { Success = false, Message = $"Class ID {classId} not found." };

                    // Remove all existing configs for this class
                    var existing = await _context.AssessmentConfigurations
                        .Where(a => a.SchoolClassId == classId)
                        .ToListAsync();

                    _context.AssessmentConfigurations.RemoveRange(existing);

                    // Insert the new set
                    var newConfigs = assessments.Select(a => new AssessmentConfiguration
                    {
                        AssessmentName = a.Name.Trim(),
                        AssessmentScore = a.Score,
                        DisplayOrder = a.Order,
                        SchoolClassId = classId,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    }).ToList();

                    await _context.AssessmentConfigurations.AddRangeAsync(newConfigs);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Assessment configuration saved successfully for {classIds.Count} class(es).",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        /// <summary>Deletes a single AssessmentConfiguration row by Id.</summary>
        public async Task<ApiResponse<bool>> DeleteAssessmentConfig(int id)
        {
            try
            {
                var config = await _context.AssessmentConfigurations.FindAsync(id);
                if (config == null)
                    return new ApiResponse<bool> { Success = false, Message = "Assessment configuration not found." };

                _context.AssessmentConfigurations.Remove(config);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Assessment configuration deleted.", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        /// <summary>Deletes ALL assessment configurations for a given class (used before reconfiguring a class).</summary>
        public async Task<ApiResponse<bool>> DeleteAllAssessmentConfigsByClassId(int classId)
        {
            try
            {
                var configs = await _context.AssessmentConfigurations
                    .Where(a => a.SchoolClassId == classId)
                    .ToListAsync();

                if (!configs.Any())
                    return new ApiResponse<bool> { Success = false, Message = "No configurations found for this class." };

                _context.AssessmentConfigurations.RemoveRange(configs);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "All configurations removed for this class.", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
        /// <summary>Updates name, max score, and display order for a single assessment config.</summary>
        public async Task<ApiResponse<bool>> UpdateSingleAssessmentConfig(
            int id, string name, double score, int displayOrder)
        {
            try
            {
                var config = await _context.AssessmentConfigurations.FindAsync(id);
                if (config == null)
                    return new ApiResponse<bool> { Success = false, Message = "Assessment configuration not found." };

                config.AssessmentName = name.Trim();
                config.AssessmentScore = score;
                config.DisplayOrder = displayOrder;
                config.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Assessment updated successfully.", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
    }
}