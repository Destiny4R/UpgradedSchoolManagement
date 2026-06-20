using UpgradedSchoolManagementModels.Models;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.DTOs
{
    public class TerminalResultPageViewModel
    {
        public bool IsAllowed { get; set; } = true;
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolLogoUrl { get; set; } = string.Empty;
        public string AccessMessage { get; set; } = string.Empty;
        public long TermRegId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? PicturePath { get; set; }
        public string AdmissionNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string ResultType { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public int? Attendance { get; set; }
        public int DaysPresent { get; set; }
        public int DaysAbsent { get; set; }
        public int TimesLate { get; set; }
        public int ClassSize { get; set; }
        public string ClassPosition { get; set; } = string.Empty;
        public string NextClass { get; set; } = string.Empty;
        public string NextTermFees { get; set; } = string.Empty;
        public string NextTermStart { get; set; } = string.Empty;
        public string ClassTeacherName { get; set; } = string.Empty;
        public string PrincipalName { get; set; } = string.Empty;
        public decimal AcademicTotal { get; set; }
        public decimal AcademicAverage { get; set; }
        public string OverallRemark { get; set; } = string.Empty;
        public string ClassTeacherRemark { get; set; } = string.Empty;
        public string PrincipalRemark { get; set; } = string.Empty;
        public List<TerminalResultAcademicRowDto> AcademicRows { get; set; } = new();
        public List<StudentResultSkillRatingDto> AffectiveSkills { get; set; } = new();
        public List<StudentResultSkillRatingDto> PsychomotorSkills { get; set; } = new();
        public List<OutstandingPaymentDto> OutstandingPayments { get; set; } = new();
        public decimal TotalOutstanding { get; set; }
    }

    public class OutstandingPaymentDto
    {
        public string PaymentItemName { get; set; } = string.Empty;
        public decimal ExpectedAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
    }
}
