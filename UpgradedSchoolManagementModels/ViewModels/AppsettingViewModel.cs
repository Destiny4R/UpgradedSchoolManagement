using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.ViewModels
{
    public class AppsettingViewModel
    {
        public int? Id { get; set; }
        public Term? Term { get; set; }
        [Display(Name ="School Class")]
        public int SchoolClassId { get; set; }
        [Display(Name ="Sub Class")]
        public int SubClassId { get; set; }
        [Display(Name = "Academic Session")]
        public int SessionId { get; set; }
        [Display(Name = "Principal Name")]
        public string? PrincipalName { get; set; }
        [Display(Name = "Principal Signature")]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile? PrincipalSignature { get; set; }
        [Display(Name = "Cashier Name")]
        public string? CashierName { get; set; }
        [Display(Name = "Cashier Signature")]
        [RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.png|.jpg|.jpeg)$", ErrorMessage = "Only image files (.png, .jpg, .jpeg) are allowed.")]
        public IFormFile? CashierSignature { get; set; }
        [Display(Name = "Examination Access Control")]
        public bool CanPrintResult { get; set; } = false;

    }
}
