using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.Models
{
    public class ClassTermInformation
    {
        public int Id { get; set; }
        public Term Term { get; set; }
        public int SchoolClassId { get; set; }
        public int SubClassId { get; set; }
        public int SessionId { get; set; }
        public decimal NextTermFees { get; set; }
        [StringLength(450)]
        public string ClassTeacherName { get; set; }
        [StringLength(450)]
        public string ApplicationUserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        //the User Id of the user inputing the data, not necessarily the class teacher, but can be used for auditing purposes
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }
        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClasses SchoolClasses { get; set; }
        [ForeignKey(nameof(SubClassId))]
        public SubClassTable SubClassTable { get; set; }
        [ForeignKey(nameof(SessionId))]
        public SesseionTable SesseionTable { get; set; }
    }
}
