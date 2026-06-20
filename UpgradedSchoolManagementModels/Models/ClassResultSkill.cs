using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    public class ClassResultSkill
    {
        public int Id { get; set; }
        public int SchoolClassId { get; set; }
        public int ResultSkillId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClasses SchoolClass { get; set; }

        [ForeignKey(nameof(ResultSkillId))]
        public ResultSkill ResultSkill { get; set; }
    }
}
