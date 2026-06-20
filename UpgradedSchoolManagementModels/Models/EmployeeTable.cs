using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    public class EmployeeTable
    {
        public int Id { get; set; }
        [StringLength(150)]
        public string FullName { get; set; }
        public ConstantEnums.Gender Gender { get; set; }
        [StringLength(50)]
        public string? EmployeeType { get; set; }
        public string? Address { get; set; }
        [StringLength(50)]
        public string? EmployeeCode { get; set; }//Auto generated employee code e.g . EMP-0001, EMP-0002, EMP-0003 etc
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        [StringLength(50)]
        public string? ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
