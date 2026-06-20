using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class PaymentSetupService : IPaymentSetupService
    {
        private readonly ApplicationDbContext _context;

        public PaymentSetupService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string GetTermName(Term term)
        {
            return term switch
            {
                Term.First => "First Term",
                Term.Second => "Second Term",
                Term.Third => "Third Term",
                _ => "Unknown"
            };
        }

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentSetupsAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null)
        {
            try
            {
                var query = _context.PaymentSetups
                    .Include(x => x.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Include(x => x.SesseionTable)
                    .Include(x => x.SchoolClass)
                    .AsQueryable();
                query = query.OrderByDescending(k => k.CreatedAt);

                if (sessionFilter.HasValue && sessionFilter.Value > 0)
                    query = query.Where(x => x.SessionId == sessionFilter.Value);
                if (termFilter.HasValue && termFilter.Value > 0)
                    query = query.Where(x => x.Term == (Term)termFilter.Value);
                if (classFilter.HasValue && classFilter.Value > 0)
                    query = query.Where(x => x.SchoolClassId == classFilter.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x =>
                        x.PaymentItem.Name.Contains(searchTerm) ||
                        x.PaymentItem.PaymentCategory.Name.Contains(searchTerm) ||
                        x.SesseionTable.Name.Contains(searchTerm) ||
                        x.SchoolClass.Name.Contains(searchTerm));
                }

                int recordsTotal = await _context.PaymentSetups.CountAsync();
                int recordsFiltered = await query.CountAsync();

                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt);

                var rawData = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        PaymentItemName = x.PaymentItem.Name,
                        CategoryName = x.PaymentItem.PaymentCategory.Name,
                        SessionName = x.SesseionTable.Name,
                        x.Term,
                        ClassName = x.SchoolClass.Name,
                        x.Amount,
                        x.IsActive,
                        x.IsCompulsory,
                        CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm tt")
                    })
                    .ToListAsync();

                var data = rawData.Select(x => new
                {
                    x.Id,
                    x.PaymentItemName,
                    x.CategoryName,
                    x.SessionName,
                    TermName = GetTermName(x.Term),
                    x.ClassName,
                    Amount = SD.ToNaira(x.Amount),
                    x.IsActive,
                    x.IsCompulsory,
                    x.CreatedAt
                }).ToList();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment setups: {ex.Message}", ex);
            }
        }

        public async Task<PaymentSetupViewModel> GetPaymentSetupByIdAsync(int id)
        {
            try
            {
                var setup = await _context.PaymentSetups
                    .Include(x => x.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Include(x => x.SesseionTable)
                    .Include(x => x.SchoolClass)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (setup == null) return null;

                return new PaymentSetupViewModel
                {
                    Id = setup.Id,
                    PaymentItemId = setup.PaymentItemId,
                    SessionId = setup.SessionId,
                    Term = setup.Term,
                    ClassId = setup.SchoolClassId,
                    Amount = setup.Amount,
                    IsActive = setup.IsActive,
                    IsCompulsory = setup.IsCompulsory,
                    CategoryId = setup.PaymentItem?.CategoryId,
                    PaymentItemName = setup.PaymentItem?.Name,
                    SessionName = setup.SesseionTable?.Name,
                    TermName = GetTermName(setup.Term),
                    ClassName = setup.SchoolClass?.Name,
                    CategoryName = setup.PaymentItem?.PaymentCategory?.Name
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment setup: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<int>> CreatePaymentSetupAsync(PaymentSetupViewModel model)
        {
            try
            {
                if (model.Amount <= 0)
                    return new ApiResponse<int> { Success = false, Message = "Amount must be greater than 0" };

                var itemExists = await _context.PaymentItems.AnyAsync(x => x.Id == model.PaymentItemId && x.IsActive);
                if (!itemExists)
                    return new ApiResponse<int> { Success = false, Message = "Selected payment item does not exist or is inactive" };

                // Check uniqueness constraint
                var duplicate = await _context.PaymentSetups.AnyAsync(x =>
                    x.PaymentItemId == model.PaymentItemId &&
                    x.SessionId == model.SessionId &&
                    x.Term == model.Term &&
                    x.SchoolClassId == model.ClassId);

                if (duplicate)
                    return new ApiResponse<int> { Success = false, Message = "A payment setup already exists for this combination of item, session, term, and class" };

                var setup = new PaymentSetup
                {
                    PaymentItemId = model.PaymentItemId,
                    SessionId = model.SessionId,
                    Term = model.Term,
                    SchoolClassId = model.ClassId,
                    Amount = model.Amount,
                    IsActive = true,
                    IsCompulsory = model.IsCompulsory,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PaymentSetups.Add(setup);
                await _context.SaveChangesAsync();

                return new ApiResponse<int> { Success = true, Message = "Payment setup created successfully", Data = setup.Id };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = $"Error creating payment setup: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<string>> CreateBatchPaymentSetupAsync(PaymentSetupViewModel model)
        {
            try
            {
                if (model.ClassIds == null || model.ClassIds.Count == 0)
                    return new ApiResponse<string> { Success = false, Message = "At least one class must be selected" };

                if (model.Amount <= 0)
                    return new ApiResponse<string> { Success = false, Message = "Amount must be greater than 0" };

                var itemExists = await _context.PaymentItems.AnyAsync(x => x.Id == model.PaymentItemId && x.IsActive);
                if (!itemExists)
                    return new ApiResponse<string> { Success = false, Message = "Selected payment item does not exist or is inactive" };

                var createdCount = 0;
                var skippedClasses = new List<string>();

                foreach (var classId in model.ClassIds.Distinct())
                {
                    var duplicate = await _context.PaymentSetups.AnyAsync(x =>
                        x.PaymentItemId == model.PaymentItemId &&
                        x.SessionId == model.SessionId &&
                        x.Term == model.Term &&
                        x.SchoolClassId == classId);

                    if (duplicate)
                    {
                        var className = await _context.SchoolClasses
                            .Where(c => c.Id == classId)
                            .Select(c => c.Name)
                            .FirstOrDefaultAsync() ?? $"ID:{classId}";
                        skippedClasses.Add(className);
                        continue;
                    }

                    var setup = new PaymentSetup
                    {
                        PaymentItemId = model.PaymentItemId,
                        SessionId = model.SessionId,
                        Term = model.Term,
                        SchoolClassId = classId,
                        Amount = model.Amount,
                        IsActive = true,
                        IsCompulsory = model.IsCompulsory,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.PaymentSetups.Add(setup);
                    createdCount++;
                }

                if (createdCount > 0)
                    await _context.SaveChangesAsync();

                if (createdCount == 0 && skippedClasses.Count > 0)
                    return new ApiResponse<string> { Success = false, Message = $"All selected classes already have this fee configured: {string.Join(", ", skippedClasses)}" };

                var message = $"Payment setup created for {createdCount} class(es) successfully";
                if (skippedClasses.Count > 0)
                    message += $". Skipped {skippedClasses.Count} duplicate(s): {string.Join(", ", skippedClasses)}";

                return new ApiResponse<string> { Success = true, Message = message, Data = message };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Message = $"Error creating batch payment setup: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> UpdatePaymentSetupAsync(PaymentSetupViewModel model)
        {
            try
            {
                var setup = await _context.PaymentSetups.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (setup == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment setup not found" };

                if (model.Amount <= 0)
                    return new ApiResponse<bool> { Success = false, Message = "Amount must be greater than 0" };

                // Check uniqueness (excluding current record)
                var duplicate = await _context.PaymentSetups.AnyAsync(x =>
                    x.PaymentItemId == model.PaymentItemId &&
                    x.SessionId == model.SessionId &&
                    x.Term == model.Term &&
                    x.SchoolClassId == model.ClassId &&
                    x.Id != model.Id);

                if (duplicate)
                    return new ApiResponse<bool> { Success = false, Message = "A payment setup already exists for this combination of item, session, term, and class" };

                setup.PaymentItemId = model.PaymentItemId;
                setup.SessionId = model.SessionId;
                setup.Term = model.Term;
                setup.SchoolClassId = model.ClassId;
                setup.Amount = model.Amount;
                setup.IsActive = model.IsActive;
                setup.IsCompulsory = model.IsCompulsory;
                setup.UpdatedAt = DateTime.UtcNow;

                _context.PaymentSetups.Update(setup);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Payment setup updated successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error updating payment setup: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> DeletePaymentSetupAsync(int id)
        {
            try
            {
                // Check if any student payment items reference this payment setup's item
                var setup = await _context.PaymentSetups.FirstOrDefaultAsync(x => x.Id == id);
                if (setup == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment setup not found" };

                var hasPayments = await _context.StudentPaymentItems
                    .AnyAsync(spi => spi.PaymentItemId == setup.PaymentItemId);
                if (hasPayments)
                    return new ApiResponse<bool> { Success = false, Message = "Cannot proceed: the payment setup is referenced by existing payment records." };

                _context.PaymentSetups.Remove(setup);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Payment setup deleted successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error deleting payment setup: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> TogglePaymentSetupAsync(int id)
        {
            try
            {
                var setup = await _context.PaymentSetups.FirstOrDefaultAsync(x => x.Id == id);
                if (setup == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment setup not found" };

                setup.IsActive = !setup.IsActive;
                setup.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var status = setup.IsActive ? "activated" : "deactivated";
                return new ApiResponse<bool> { Success = true, Message = $"Payment setup {status} successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error toggling payment setup: {ex.Message}" };
            }
        }
    }
}
