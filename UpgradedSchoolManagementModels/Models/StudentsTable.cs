using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    public class StudentsTable
    {
        public int Id { get; set; }
        [StringLength(30)]
        public string? AdmissionNumber { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(50)]
        public string Surname { get; set; } = string.Empty;
        [StringLength(50)]
        public string? OtherName { get; set; }
        public ConstantEnums.Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        [StringLength(50)]
        public string? Nationality { get; set; }
        [StringLength(50)]
        public string? State { get; set; }
        [StringLength(50)]
        public string? LocalGov { get; set; }
        [StringLength(150)]
        public string? Address { get; set; }
        [StringLength(450)]
        public string? PicturePath { get; set; }
        public string? ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

        [NotMapped]
        public string FullName => $"{Surname} {OtherName} {FirstName}";
    }
}