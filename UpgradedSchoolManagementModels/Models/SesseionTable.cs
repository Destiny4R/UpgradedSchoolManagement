using System.ComponentModel.DataAnnotations;

namespace UpgradedSchoolManagementModels.Models
{
    public class SesseionTable
    {
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public string Name { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}
