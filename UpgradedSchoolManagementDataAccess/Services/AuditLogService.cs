using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string userName, string action, string module, string? description = null, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    Module = module,
                    Description = description,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedDate = DateTime.UtcNow
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Audit logging should never crash the calling operation
            }
        }

        public async Task<DataTablesResponse<AuditLog>> GetLogs(DataTablesRequest request)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var searchValue = request.Search.Value.ToLower();
                    query = query.Where(l => l.UserName.ToLower().Contains(searchValue) ||
                                            l.Action.ToLower().Contains(searchValue) ||
                                            l.Module.ToLower().Contains(searchValue));
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
                        nameof(AuditLog.UserName) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.UserName)
                            : query.OrderByDescending(l => l.UserName),
                        nameof(AuditLog.Action) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.Action)
                            : query.OrderByDescending(l => l.Action),
                        nameof(AuditLog.Module) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.Module)
                            : query.OrderByDescending(l => l.Module),
                        nameof(AuditLog.CreatedDate) => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.CreatedDate)
                            : query.OrderByDescending(l => l.CreatedDate),
                        _ => query.OrderByDescending(l => l.CreatedDate)
                    };
                }
                else
                {
                    query = query.OrderByDescending(l => l.CreatedDate);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var data = await query
                    .Skip(request.Start)
                    .Take(length)
                    .ToListAsync();

                return new DataTablesResponse<AuditLog>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<AuditLog>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<AuditLog>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteOldLogs(int daysOld)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                var oldLogs = await _context.AuditLogs
                    .Where(l => l.CreatedDate < cutoffDate)
                    .ToListAsync();

                _context.AuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Deleted {oldLogs.Count} old log entries",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
    }
}