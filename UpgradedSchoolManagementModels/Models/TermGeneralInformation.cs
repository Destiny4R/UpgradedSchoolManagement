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
    public class TermGeneralInformation
    {
        public int Id { get; set; }
        public Term Term { get; set; }
        public int SessionId { get; set; }
        public int DaySchoolOpen { get; set; }
        [StringLength(100)]
        public string? PrincipalName { get; set; }
        public DateTime NextTermStart { get; set; }
        public DateTime NextTermEnd { get; set; }
        [StringLength(450)]
        public string ApplicationUserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(SessionId))]
        public SesseionTable SesseionTable { get; set; }
        //the User Id of the user inputing the data, not necessarily the class teacher, but can be used for auditing purposes
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
