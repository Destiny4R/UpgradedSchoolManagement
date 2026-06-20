using Microsoft.EntityFrameworkCore;
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
    //Make ApplicationUserId Unique
    [Index(nameof(ApplicationUserId), IsUnique = true)]
    public class AppSettings
    {
        public int Id { get; set; }
        public Term? Term { get; set; }
        public int? SchoolClassId { get; set; }
        public int? SubClassId { get; set; }
        public int? SessionId { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? PrincipalName { get; set; }
        [Column(TypeName = "nvarchar(256)")]
        public string? PrincipalSignature { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? CashierName { get; set; }
        [Column(TypeName = "nvarchar(256)")]
        public string? CashierSignature { get; set; }
        [StringLength(450)]
        public string ApplicationUserId { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool CanPrintResult { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        //Navigations
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
