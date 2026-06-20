using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.ViewModels
{
    public class ResultConfigViewModel
    {
        [Required, MinLength(1, ErrorMessage = "Select at least one class.")]
        public List<int> SchoolClassIds { get; set; } = new();

        [Required, MinLength(1, ErrorMessage = "Add at least one assessment.")]
        public List<ResultConfigAssessmentViewModel> Assessments { get; set; } = new();
    }

    public class ResultConfigAssessmentViewModel
    {
        [Required]
        public string Name { get; set; }

        [Range(1, 100)]
        public int Score { get; set; }

        [Range(1, 50)]
        public int Order { get; set; }
    }
}
