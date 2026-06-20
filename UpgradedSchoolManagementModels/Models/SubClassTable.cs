using System.ComponentModel.DataAnnotations;

namespace UpgradedSchoolManagementModels.Models
{
    public class SubClassTable
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}
