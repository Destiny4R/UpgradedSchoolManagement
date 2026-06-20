using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class TermGeneralInformationService : ITermGeneralInformationService
    {
        private readonly ApplicationDbContext _db;

        public TermGeneralInformationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DataTablesResponse<TermGeneralInformationDto>> GetTermGeneralInformations(DataTablesRequest request)
        {
            try
            {
                var query = _db.TermGeneralInformations
                    .Include(x => x.SesseionTable)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var search = request.Search.Value.ToLower();
                    query = query.Where(x =>
                        x.PrincipalName!.ToLower().Contains(search) ||
                        x.Term.ToString().ToLower().Contains(search));
                }

                var recordsFiltered = await query.CountAsync();

                if (request.Order != null && request.Order.Any())
                {
                    var order = request.Order.First();
                    query = (order.Column) switch
                    {
                        1 => order.Dir == "asc" ? query.OrderBy(x => x.Term) : query.OrderByDescending(x => x.Term),
                        2 => order.Dir == "asc" ? query.OrderBy(x => x.SessionId) : query.OrderByDescending(x => x.SessionId),
                        3 => order.Dir == "asc" ? query.OrderBy(x => x.DaySchoolOpen) : query.OrderByDescending(x => x.DaySchoolOpen),
                        _ => query.OrderByDescending(x => x.CreatedDate)
                    };
                }

                var data = await query
                    .Skip(request.Start)
                    .Take(request.Length > 0 ? request.Length : 10)
                    .ToListAsync();

                var dtoData = data.Select(x => new TermGeneralInformationDto
                {
                    Id = x.Id,
                    Term = x.Term switch
                    {
                        ConstantEnums.Term.First => "First Term",
                        ConstantEnums.Term.Second => "Second Term",
                        ConstantEnums.Term.Third => "Third Term",
                        _ => x.Term.ToString()
                    },
                    Session = x.SesseionTable?.Name ?? x.SessionId.ToString(),
                    DaySchoolOpen = x.DaySchoolOpen,
                    PrincipalName = x.PrincipalName,
                    NextTermStart = x.NextTermStart,
                    NextTermEnd = x.NextTermEnd,
                    CreatedDate = x.CreatedDate
                }).ToList();

                return new DataTablesResponse<TermGeneralInformationDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = dtoData
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<TermGeneralInformationDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<TermGeneralInformationDto>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<TermGeneralInformationRequest?> GetById(int id)
        {
            var entity = await _db.TermGeneralInformations.FindAsync(id);
            if (entity == null) return null;

            return new TermGeneralInformationRequest
            {
                Id = entity.Id,
                Term = (int)entity.Term,
                SessionId = entity.SessionId,
                DaySchoolOpen = entity.DaySchoolOpen,
                PrincipalName = entity.PrincipalName,
                NextTermStart = entity.NextTermStart,
                NextTermEnd = entity.NextTermEnd
            };
        }

        public async Task<TermGeneralInformationRequest> GetBySessionAndTerm(int sessionId, int termId)
        {
            var entity = await _db.TermGeneralInformations.FirstOrDefaultAsync(x => x.SessionId == sessionId && (int)x.Term == termId);
            if (entity == null) return null;

            return  new TermGeneralInformationRequest
            {
                Id = entity.Id,
                Term = (int)entity.Term,
                SessionId = entity.SessionId,
                DaySchoolOpen = entity.DaySchoolOpen,
                PrincipalName = entity.PrincipalName,
                NextTermStart = entity.NextTermStart,
                NextTermEnd = entity.NextTermEnd
            };
        }

        public async Task<ApiResponse<int>> Create(TermGeneralInformationRequest request)
        {
            try
            {
                var existing = await _db.TermGeneralInformations
                    .FirstOrDefaultAsync(x => x.Term == (ConstantEnums.Term)request.Term && x.SessionId == request.SessionId);
                if (existing != null)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "A record already exists for this term and session."
                    };
                }

                var entity = new TermGeneralInformation
                {
                    Term = (ConstantEnums.Term)request.Term,
                    SessionId = request.SessionId,
                    DaySchoolOpen = request.DaySchoolOpen,
                    PrincipalName = request.PrincipalName,
                    NextTermStart = request.NextTermStart,
                    NextTermEnd = request.NextTermEnd,
                    ApplicationUserId = request.ApplicationUserId ?? string.Empty,
                    CreatedDate = DateTime.UtcNow
                };

                _db.TermGeneralInformations.Add(entity);
                await _db.SaveChangesAsync();

                return new ApiResponse<int>
                {
                    Success = true,
                    Message = "Term general information created successfully.",
                    Data = entity.Id
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> Update(TermGeneralInformationRequest request)
        {
            try
            {
                var existing = await _db.TermGeneralInformations.FindAsync(request.Id);
                if (existing == null)
                {
                    return new ApiResponse<bool> { Success = false, Message = "Record not found." };
                }

                var duplicate = await _db.TermGeneralInformations
                    .FirstOrDefaultAsync(x => x.Term == (ConstantEnums.Term)request.Term && x.SessionId == request.SessionId && x.Id != request.Id);
                if (duplicate != null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "A record already exists for this term and session."
                    };
                }

                existing.Term = (ConstantEnums.Term)request.Term;
                existing.SessionId = request.SessionId;
                existing.DaySchoolOpen = request.DaySchoolOpen;
                existing.PrincipalName = request.PrincipalName;
                existing.NextTermStart = request.NextTermStart;
                existing.NextTermEnd = request.NextTermEnd;

                await _db.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Term general information updated successfully.",
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
                var entity = await _db.TermGeneralInformations.FindAsync(id);
                if (entity == null)
                {
                    return new ApiResponse<bool> { Success = false, Message = "Record not found." };
                }

                _db.TermGeneralInformations.Remove(entity);
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
