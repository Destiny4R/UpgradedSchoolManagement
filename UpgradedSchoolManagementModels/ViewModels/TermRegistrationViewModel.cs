using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.ViewModels
{
    public class TermRegistrationViewModel
    {
        public long? Id { get; set; }
        [Required, Display(Name = "School Class")]
        public int SchoolClassId { get; set; }
        [Required, Display(Name = "Academic Session")]
        public int SessionId { get; set; }

        [Required, Display(Name = "Term")]
        public Term Term { get; set; }
        [Required, Display(Name = "School Subclass")]
        public int SchoolSubclassId { get; set; }
        [Required]//Required but hidden field
        public int StudentId { get; set; }
        public List<int>? SubjectsId { get; set; }
        //For display purposes
        public string? StudentName { get; set; }
        //For display purposes
        public string? StudentRegNumber { get; set; }

        public List<RegisteredSubjectDto>? RegisteredSubjects { get; set; } = new();
    }

    public class RegisteredSubjectDto
    {
        public long ResultTableId { get; set; }
        public string SubjectName { get; set; }
        public bool HasRecordedResults { get; set; }
    }
}
