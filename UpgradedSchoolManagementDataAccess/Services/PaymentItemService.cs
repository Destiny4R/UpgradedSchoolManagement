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

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class PaymentItemService : IPaymentItemService
    {
        private readonly ApplicationDbContext _context;

        public PaymentItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentItemsAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? categoryFilter = null)
        {
            try
            {
                var query = _context.PaymentItems.Include(x => x.PaymentCategory).AsQueryable();

                if (categoryFilter.HasValue && categoryFilter.Value > 0)
                {
                    query = query.Where(x => x.CategoryId == categoryFilter.Value);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.Name.Contains(searchTerm) || x.Description.Contains(searchTerm)
                        || x.PaymentCategory.Name.Contains(searchTerm));
                }

                int recordsTotal = categoryFilter.HasValue && categoryFilter.Value > 0
                    ? await _context.PaymentItems.Where(x => x.CategoryId == categoryFilter.Value).CountAsync()
                    : await _context.PaymentItems.CountAsync();
                int recordsFiltered = await query.CountAsync();

                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(x => x.Id)
                    : query.OrderBy(x => x.Id);

                var data = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Description,
                        x.IsActive,
                        x.CategoryId,
                        CategoryName = x.PaymentCategory.Name,
                        SetupCount = x.PaymentSetups.Count,
                        CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm tt"),
                        UpdatedAt = x.UpdatedAt.ToString("dd/MM/yyyy hh:mm tt")
                    })
                    .ToListAsync();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment items: {ex.Message}", ex);
            }
        }

        public async Task<PaymentItemViewModel> GetPaymentItemByIdAsync(int id)
        {
            try
            {
                var item = await _context.PaymentItems
                    .Include(x => x.PaymentCategory)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (item == null) return null;

                return new PaymentItemViewModel
                {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    Name = item.Name,
                    Description = item.Description,
                    IsActive = item.IsActive,
                    CategoryName = item.PaymentCategory?.Name
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment item: {ex.Message}", ex);
            }
        }

        public async Task<List<PaymentItemViewModel>> GetActiveItemsAsync(int? categoryId = null)
        {
            var query = _context.PaymentItems
                .Include(x => x.PaymentCategory)
                .Where(x => x.IsActive);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new PaymentItemViewModel
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CategoryName = x.PaymentCategory.Name
                })
                .ToListAsync();
        }

        public async Task<ApiResponse<int>> CreatePaymentItemAsync(PaymentItemViewModel model)
        {
            try
            {
                var categoryExists = await _context.PaymentCategories.AnyAsync(x => x.Id == model.CategoryId && x.IsActive);
                if (!categoryExists)
                    return new ApiResponse<int> { Success = false, Message = "Selected payment category does not exist or is inactive" };

                var exists = await _context.PaymentItems.AnyAsync(x => x.Name == model.Name.Trim() && x.CategoryId == model.CategoryId);
                if (exists)
                    return new ApiResponse<int> { Success = false, Message = "A payment item with this name already exists in this category" };

                var item = new PaymentItem
                {
                    CategoryId = model.CategoryId,
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PaymentItems.Add(item);
                await _context.SaveChangesAsync();

                return new ApiResponse<int> { Success = true, Message = "Payment item created successfully", Data = item.Id };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = $"Error creating payment item: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> UpdatePaymentItemAsync(PaymentItemViewModel model)
        {
            try
            {
                var item = await _context.PaymentItems.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (item == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment item not found" };

                var exists = await _context.PaymentItems.AnyAsync(x => x.Name == model.Name.Trim() && x.CategoryId == model.CategoryId && x.Id != model.Id);
                if (exists)
                    return new ApiResponse<bool> { Success = false, Message = "Another payment item with this name already exists in this category" };

                item.CategoryId = model.CategoryId;
                item.Name = model.Name.Trim();
                item.Description = model.Description?.Trim();
                item.IsActive = model.IsActive;
                item.UpdatedAt = DateTime.UtcNow;

                _context.PaymentItems.Update(item);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Payment item updated successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error updating payment item: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> DeletePaymentItemAsync(int id)
        {
            try
            {
                var item = await _context.PaymentItems.FirstOrDefaultAsync(x => x.Id == id);
                if (item == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment item not found" };

                var hasSetups = await _context.PaymentSetups.AnyAsync(x => x.PaymentItemId == id);
                if (hasSetups)
                    return new ApiResponse<bool> { Success = false, Message = "Cannot delete payment item with existing payment setups. Remove setups first." };

                _context.PaymentItems.Remove(item);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Payment item deleted successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error deleting payment item: {ex.Message}" };
            }
        }

        public async Task<List<PaymentItemViewModel>> GetActiveItemsWithCategoryAsync()
        {
            return await _context.PaymentItems
                .Include(x => x.PaymentCategory)
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new PaymentItemViewModel
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    Name = x.Name + " (" + x.PaymentCategory.Name + ")",
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CategoryName = x.PaymentCategory.Name
                })
                .ToListAsync();
        }

        public async Task<ApiResponse<bool>> TogglePaymentItemAsync(int id)
        {
            try
            {
                var item = await _context.PaymentItems.FirstOrDefaultAsync(x => x.Id == id);
                if (item == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment item not found" };

                item.IsActive = !item.IsActive;
                item.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var status = item.IsActive ? "activated" : "deactivated";
                return new ApiResponse<bool> { Success = true, Message = $"Payment item {status} successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error toggling payment item: {ex.Message}" };
            }
        }
    }
}
