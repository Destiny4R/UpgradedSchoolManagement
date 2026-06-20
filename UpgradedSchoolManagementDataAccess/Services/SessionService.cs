using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataTablesResponse<SesseionTable>> GetSessions(DataTablesRequest request)
        {
            try
            {
                var query = _context.SesseionTables.AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var searchValue = request.Search.Value.ToLower();
                    query = query.Where(s => s.Name.ToLower().Contains(searchValue));
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
                        nameof(SesseionTable.Name) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.Name)
                            : query.OrderByDescending(s => s.Name),
                        nameof(SesseionTable.IsActive) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.IsActive)
                            : query.OrderByDescending(s => s.IsActive),
                        nameof(SesseionTable.CreatedDate) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(s => s.CreatedDate)
                            : query.OrderByDescending(s => s.CreatedDate),
                        _ => query.OrderByDescending(s => s.CreatedDate)
                    };
                }
                else
                {
                    query = query.OrderByDescending(s => s.CreatedDate);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .ToListAsync();

                return new DataTablesResponse<SesseionTable>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<SesseionTable>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<SesseionTable>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<SesseionTable?> GetSessionById(int id)
        {
            try
            {
                return await _context.SesseionTables.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<SesseionTable>> CreateSession(string name)
        {
            try
            {
                var existingSession = await _context.SesseionTables
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());

                if (existingSession != null)
                {
                    return new ApiResponse<SesseionTable>
                    {
                        Success = false,
                        Message = "Session with this name already exists"
                    };
                }

                var session = new SesseionTable
                {
                    Name = name.Trim(),
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow
                };

                _context.SesseionTables.Add(session);
                await _context.SaveChangesAsync();

                return new ApiResponse<SesseionTable>
                {
                    Success = true,
                    Message = "Session created successfully",
                    Data = session
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SesseionTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<SesseionTable>> UpdateSession(int id, string name)
        {
            try
            {
                var session = await _context.SesseionTables.FindAsync(id);
                if (session == null)
                {
                    return new ApiResponse<SesseionTable>
                    {
                        Success = false,
                        Message = "Session not found"
                    };
                }

                var existingSession = await _context.SesseionTables
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower() && s.Id != id);

                if (existingSession != null)
                {
                    return new ApiResponse<SesseionTable>
                    {
                        Success = false,
                        Message = "Session with this name already exists"
                    };
                }

                session.Name = name.Trim();
                session.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<SesseionTable>
                {
                    Success = true,
                    Message = "Session updated successfully",
                    Data = session
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SesseionTable> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> DeleteSession(int id)
        {
            try
            {
                var session = await _context.SesseionTables.FindAsync(id);
                if (session == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Session not found"
                    };
                }

                var hasRegistrations = await _context.TermRegistrations
                    .AnyAsync(tr => tr.SessionId == id);

                if (hasRegistrations)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete session with existing term registrations"
                    };
                }

                _context.SesseionTables.Remove(session);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Session deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> ToggleSessionStatus(int id)
        {
            try
            {
                var session = await _context.SesseionTables.FindAsync(id);
                if (session == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Session not found"
                    };
                }

                if (session.IsActive)
                {
                    session.IsActive = false;
                }
                else
                {
                    var activeSessions = await _context.SesseionTables
                        .Where(s => s.IsActive)
                        .ToListAsync();

                    foreach (var s in activeSessions)
                    {
                        s.IsActive = false;
                    }

                    session.IsActive = true;
                }

                session.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = session.IsActive ? "Session activated" : "Session deactivated",
                    Data = session.IsActive
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<int?> GetLatestActiveSessionIdAsync()
        {
            return await _context.SesseionTables
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.Id)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();
        }
    }
}