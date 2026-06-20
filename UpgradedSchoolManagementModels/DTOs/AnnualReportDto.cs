namespace UpgradedSchoolManagementModels.DTOs
{
    public class AnnualReportViewModel
    {
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolAddress { get; set; } = string.Empty;
        public string SchoolPhone { get; set; } = string.Empty;
        public string SchoolEmail { get; set; } = string.Empty;
        public string SchoolLogoUrl { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? PicturePath { get; set; }
        public string AdmissionNumber { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string House { get; set; } = string.Empty;
        public int? Term1Present { get; set; }
        public int? Term1OutOf { get; set; }
        public int? Term2Present { get; set; }
        public int? Term2OutOf { get; set; }
        public int? Term3Present { get; set; }
        public int? Term3OutOf { get; set; }
        public int AnnualPresent { get; set; }
        public int AnnualOutOf { get; set; }
        public decimal AnnualAttendancePercent { get; set; }
        public List<AnnualSubjectReport> Subjects { get; set; } = new();
        public decimal GrandTotal { get; set; }
        public decimal OverallAverage { get; set; }
        public string OverallGrade { get; set; } = string.Empty;
        public string OverallRemark { get; set; } = string.Empty;
        public int OverallPosition { get; set; }
        public int TotalStudents { get; set; }
        public decimal? Term1Avg { get; set; }
        public decimal? Term2Avg { get; set; }
        public decimal? Term3Avg { get; set; }
        public int? Term1OverallPos { get; set; }
        public int? Term2OverallPos { get; set; }
        public int? Term3OverallPos { get; set; }
        public List<SkillRatingItem> PsychomotorSkills { get; set; } = new();
        public List<SkillRatingItem> AffectiveDomain { get; set; } = new();
        public string FormTeacherRemark { get; set; } = string.Empty;
        public string FormTeacherName { get; set; } = string.Empty;
        public string PrincipalRemark { get; set; } = string.Empty;
        public string PrincipalName { get; set; } = string.Empty;
        public string PromotedToClass { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class AnnualSubjectReport
    {
        public string SubjectName { get; set; } = string.Empty;
        public double? Term1Score { get; set; }
        public string Term1Grade { get; set; } = string.Empty;
        public int Term1Pos { get; set; }
        public int Term1TotalStudents { get; set; }
        public double? Term2Score { get; set; }
        public string Term2Grade { get; set; } = string.Empty;
        public int Term2Pos { get; set; }
        public int Term2TotalStudents { get; set; }
        public double? Term3Score { get; set; }
        public string Term3Grade { get; set; } = string.Empty;
        public int Term3Pos { get; set; }
        public int Term3TotalStudents { get; set; }
        public decimal AnnualAvg { get; set; }
        public string AnnualGrade { get; set; } = string.Empty;
        public int AnnualPos { get; set; }
        public int AnnualTotalStudents { get; set; }
    }

    public class SkillRatingItem
    {
        public string SkillName { get; set; } = string.Empty;
        public double? Term1Score { get; set; }
        public double? Term2Score { get; set; }
        public double? Term3Score { get; set; }
    }

    public class AnnualReportFilters
    {
        public int StudentId { get; set; }
        public int SessionId { get; set; }
        public int ClassId { get; set; }
        public int SubClassId { get; set; }
    }
}
