using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    public class StudentResultSkillRating
    {
        public int Id { get; set; }
        public long TermRegId { get; set; }
        public int ResultSkillId { get; set; }
        public byte Score { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey(nameof(TermRegId))]
        public TermRegistration TermRegistration { get; set; }

        [ForeignKey(nameof(ResultSkillId))]
        public ResultSkill ResultSkill { get; set; }
    }
}
