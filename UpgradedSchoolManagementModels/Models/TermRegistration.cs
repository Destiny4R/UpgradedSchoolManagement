using System.ComponentModel.DataAnnotations.Schema;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.Models
{
    public class TermRegistration
    {
        public long Id { get; set; }
        public Term Term { get; set; }
        public int SchoolClassId { get; set; }
        public int SubClassId { get; set; }
        public int SessionId { get; set; }
        public int StudentId { get; set; }
        public int? Attendance { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<ResultTable> ResultTable { get; set; }
        //Navigations
        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClasses SchoolClasses { get; set; }
        [ForeignKey(nameof(SubClassId))]
        public SubClassTable SubClassTable { get; set; }
        [ForeignKey(nameof(SessionId))]
        public SesseionTable SesseionTable { get; set; }
        [ForeignKey(nameof(StudentId))]
        public StudentsTable StudentsTable { get; set; }
        public virtual StudentRating StudentRatings { get; set; }
    }
}
