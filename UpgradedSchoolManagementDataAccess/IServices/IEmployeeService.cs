using System.ComponentModel.DataAnnotations;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IEmployeeService
    {
        Task<DataTablesResponse<EmployeeDto>> GetEmployees(DataTablesRequest request);
        Task<EmployeeTable?> GetEmployeeById(int id);
        Task<ApiResponse<EmployeeTable>> CreateEmployee(CreateEmployeeInput input);
        Task<ApiResponse<EmployeeTable>> UpdateEmployee(UpdateEmployeeInput input);
        Task<ApiResponse<bool>> DeleteEmployee(int id);
        Task<string> GenerateEmployeeCode();
    }

    public class CreateEmployeeInput
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public int Gender { get; set; }
        public string? EmployeeType { get; set; }
        public string? Address { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateEmployeeInput
    {
        public int Id { get; set; }
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public int Gender { get; set; }
        public string? EmployeeType { get; set; }
        public string? Address { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? EmployeeType { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
