using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    /// <summary>
    /// Represents a single assessment type (e.g. "Class Work", "Exam") for a specific school class.
    /// Each class can have multiple AssessmentConfiguration rows — one per assessment type.
    /// The DisplayOrder property controls the column order on the result sheet.
    /// </summary>
    public class AssessmentConfiguration
    {
        public int Id { get; set; }

        /// <summary>The name of this assessment, e.g. "Class Work", "Home Work", "Examination".</summary>
        [Required]
        [StringLength(50)]
        public string AssessmentName { get; set; }

        /// <summary>The maximum score for this assessment, e.g. 10, 20, 60.</summary>
        public double AssessmentScore { get; set; }

        /// <summary>
        /// Controls the display/column order on the result sheet.
        /// Lower values appear first. e.g. Class Work = 1, Home Work = 2, Exam = 5.
        /// </summary>
        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>The school class this assessment configuration belongs to.</summary>
        public int SchoolClassId { get; set; }
        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClasses SchoolClasses { get; set; }

        /// <summary>Navigation: the individual scores recorded against this assessment type.</summary>
        //public ICollection<ResultScore> ResultScores { get; set; } = new List<ResultScore>();
    }
}
