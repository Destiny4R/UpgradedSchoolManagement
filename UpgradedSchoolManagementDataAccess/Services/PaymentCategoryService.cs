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
    public class PaymentCategoryService : IPaymentCategoryService
    {
        private readonly ApplicationDbContext _context;

        public PaymentCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentCategoriesAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc")
        {
            try
            {
                var query = _context.PaymentCategories.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.Name.Contains(searchTerm) || x.Description.Contains(searchTerm));
                }

                int recordsTotal = await _context.PaymentCategories.CountAsync();
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
                        ItemCount = x.PaymentItems.Count,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToListAsync();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment categories: {ex.Message}", ex);
            }
        }

        public async Task<PaymentCategoryViewModel> GetPaymentCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.PaymentCategories.FirstOrDefaultAsync(x => x.Id == id);
                if (category == null) return null;

                return new PaymentCategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment category: {ex.Message}", ex);
            }
        }

        public async Task<List<PaymentCategoryViewModel>> GetActiveCategoriesAsync()
        {
            return await _context.PaymentCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new PaymentCategoryViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<ApiResponse<int>> CreatePaymentCategoryAsync(PaymentCategoryViewModel model)
        {
            try
            {
                var exists = await _context.PaymentCategories.AnyAsync(x => x.Name == model.Name.Trim());
                if (exists)
                    return new ApiResponse<int> { Success = false, Message = "A payment category with this name already exists" };

                var category = new PaymentCategory
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PaymentCategories.Add(category);
                await _context.SaveChangesAsync();

                return new ApiResponse<int> { Success = true, Message = "Payment category created successfully", Data = category.Id };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = $"Error creating payment category: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> UpdatePaymentCategoryAsync(PaymentCategoryViewModel model)
        {
            try
            {
                var category = await _context.PaymentCategories.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (category == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment category not found" };

                var exists = await _context.PaymentCategories.AnyAsync(x => x.Name == model.Name.Trim() && x.Id != model.Id);
                if (exists)
                    return new ApiResponse<bool> { Success = false, Message = "Another payment category with this name already exists" };

                category.Name = model.Name.Trim();
                category.Description = model.Description?.Trim();
                category.IsActive = model.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                _context.PaymentCategories.Update(category);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Payment category updated successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error updating payment category: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> DeletePaymentCategoryAsync(int id)
        {
            try
            {
                var category = await _context.PaymentCategories.FirstOrDefaultAsync(x => x.Id == id);
                if (category == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment category not found" };

                var hasItems = await _context.PaymentItems.AnyAsync(x => x.CategoryId == id);
                if (hasItems)
                    return new ApiResponse<bool> { Success = false, Message = "Cannot delete category with dependent payment items. Remove items first." };

                _context.PaymentCategories.Remove(category);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Payment category deleted successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error deleting payment category: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> TogglePaymentCategoryAsync(int id)
        {
            try
            {
                var category = await _context.PaymentCategories.FirstOrDefaultAsync(x => x.Id == id);
                if (category == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment category not found" };

                category.IsActive = !category.IsActive;
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var status = category.IsActive ? "activated" : "deactivated";
                return new ApiResponse<bool> { Success = true, Message = $"Payment category {status} successfully", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error toggling payment category: {ex.Message}" };
            }
        }
    }
}
