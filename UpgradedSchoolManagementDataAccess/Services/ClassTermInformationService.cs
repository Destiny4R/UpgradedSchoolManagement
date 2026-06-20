using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class ClassTermInformationService : IClassTermInformationService
    {
        private readonly ApplicationDbContext _db;

        public ClassTermInformationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DataTablesResponse<ClassTermInformationDto>> GetClassTermInformations(DataTablesRequest request)
        {
            try
            {
                var query = _db.ClassTermInformations
                    .Include(x => x.SchoolClasses)
                    .Include(x => x.SubClassTable)
                    .Include(x => x.SesseionTable)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var search = request.Search.Value.ToLower();
                    query = query.Where(x =>
                        x.ClassTeacherName.ToLower().Contains(search) ||
                        x.SchoolClasses.Name.ToLower().Contains(search) ||
                        x.SubClassTable.Name.ToLower().Contains(search) ||
                        x.SesseionTable.Name.ToLower().Contains(search));
                }

                var recordsFiltered = await query.CountAsync();

                if (request.Order != null && request.Order.Any())
                {
                    var order = request.Order.First();
                    query = (order.Column) switch
                    {
                        1 => order.Dir == "asc" ? query.OrderBy(x => x.Term) : query.OrderByDescending(x => x.Term),
                        2 => order.Dir == "asc" ? query.OrderBy(x => x.SchoolClasses.Name) : query.OrderByDescending(x => x.SchoolClasses.Name),
                        3 => order.Dir == "asc" ? query.OrderBy(x => x.SubClassTable.Name) : query.OrderByDescending(x => x.SubClassTable.Name),
                        4 => order.Dir == "asc" ? query.OrderBy(x => x.SesseionTable.Name) : query.OrderByDescending(x => x.SesseionTable.Name),
                        5 => order.Dir == "asc" ? query.OrderBy(x => x.NextTermFees) : query.OrderByDescending(x => x.NextTermFees),
                        _ => query.OrderByDescending(x => x.CreatedDate)
                    };
                }

                var data = await query
                    .Skip(request.Start)
                    .Take(request.Length > 0 ? request.Length : 10)
                    .ToListAsync();

                var dtoData = data.Select(x => new ClassTermInformationDto
                {
                    Id = x.Id,
                    Term = x.Term switch
                    {
                        ConstantEnums.Term.First => "First Term",
                        ConstantEnums.Term.Second => "Second Term",
                        ConstantEnums.Term.Third => "Third Term",
                        _ => x.Term.ToString()
                    },
                    SchoolClass = x.SchoolClasses?.Name ?? x.SchoolClassId.ToString(),
                    SubClass = x.SubClassTable?.Name ?? x.SubClassId.ToString(),
                    Session = x.SesseionTable?.Name ?? x.SessionId.ToString(),
                    NextTermFees = x.NextTermFees,
                    ClassTeacherName = x.ClassTeacherName,
                    CreatedDate = x.CreatedDate
                }).ToList();

                return new DataTablesResponse<ClassTermInformationDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = dtoData
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<ClassTermInformationDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<ClassTermInformationDto>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<ClassTermInformationRequest?> GetById(int id)
        {
            var entity = await _db.ClassTermInformations.FindAsync(id);
            if (entity == null) return null;

            return new ClassTermInformationRequest
            {
                Id = entity.Id,
                Term = (int)entity.Term,
                SchoolClassId = entity.SchoolClassId,
                SubClassId = entity.SubClassId,
                SessionId = entity.SessionId,
                NextTermFees = entity.NextTermFees,
                ClassTeacherName = entity.ClassTeacherName
            };
        }

        public async Task<ApiResponse<int>> Create(ClassTermInformationRequest request)
        {
            try
            {
                var existing = await _db.ClassTermInformations
                    .FirstOrDefaultAsync(x =>
                        x.Term == (ConstantEnums.Term)request.Term &&
                        x.SessionId == request.SessionId &&
                        x.SchoolClassId == request.SchoolClassId &&
                        x.SubClassId == request.SubClassId);
                if (existing != null)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "A record already exists for this combination."
                    };
                }

                var entity = new ClassTermInformation
                {
                    Term = (ConstantEnums.Term)request.Term,
                    SchoolClassId = request.SchoolClassId,
                    SubClassId = request.SubClassId,
                    SessionId = request.SessionId,
                    NextTermFees = request.NextTermFees,
                    ClassTeacherName = request.ClassTeacherName,
                    ApplicationUserId = request.ApplicationUserId ?? string.Empty,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _db.ClassTermInformations.Add(entity);
                await _db.SaveChangesAsync();

                return new ApiResponse<int>
                {
                    Success = true,
                    Message = "Class term information created successfully.",
                    Data = entity.Id
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> Update(ClassTermInformationRequest request)
        {
            try
            {
                var existing = await _db.ClassTermInformations.FindAsync(request.Id);
                if (existing == null)
                {
                    return new ApiResponse<bool> { Success = false, Message = "Record not found." };
                }

                var duplicate = await _db.ClassTermInformations
                    .FirstOrDefaultAsync(x =>
                        x.Term == (ConstantEnums.Term)request.Term &&
                        x.SessionId == request.SessionId &&
                        x.SchoolClassId == request.SchoolClassId &&
                        x.SubClassId == request.SubClassId &&
                        x.Id != request.Id);
                if (duplicate != null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "A record already exists for this combination."
                    };
                }

                existing.Term = (ConstantEnums.Term)request.Term;
                existing.SchoolClassId = request.SchoolClassId;
                existing.SubClassId = request.SubClassId;
                existing.SessionId = request.SessionId;
                existing.NextTermFees = request.NextTermFees;
                existing.ClassTeacherName = request.ClassTeacherName;
                existing.UpdatedDate = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Class term information updated successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> Delete(int id)
        {
            try
            {
                var entity = await _db.ClassTermInformations.FindAsync(id);
                if (entity == null)
                {
                    return new ApiResponse<bool> { Success = false, Message = "Record not found." };
                }

                _db.ClassTermInformations.Remove(entity);
                await _db.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Record deleted successfully.", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
    }
}
