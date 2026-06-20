using Microsoft.AspNetCore.Identity;

namespace UpgradedSchoolManagementModels.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public long? SchoolId { get; set; }
        public string? Department { get; set; }
        public string? StaffId { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public StudentsTable? Student { get; set; }
        public EmployeeTable? Employee { get; set; }
    }
}
