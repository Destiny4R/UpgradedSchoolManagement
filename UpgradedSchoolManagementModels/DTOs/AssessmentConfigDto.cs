using System;

namespace UpgradedSchoolManagementModels.DTOs
{
    /// <summary>
    /// Flat DTO used to project AssessmentConfiguration rows for DataTables display.
    /// </summary>
    public class AssessmentConfigDto
    {
        public int Id { get; set; }
        public string AssessmentName { get; set; }
        public double AssessmentScore { get; set; }
        public int DisplayOrder { get; set; }

        /// <summary>The name of the SchoolClass this config belongs to.</summary>
        public string ClassName { get; set; }
        public int SchoolClassId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
