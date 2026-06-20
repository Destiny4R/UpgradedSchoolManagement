using UpgradedSchoolManagementModels.Models;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.DTOs
{
    public class ResultSkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public ResultSkillDomain Domain { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateResultSkillDto
    {
        public string Name { get; set; } = string.Empty;
        public ResultSkillDomain Domain { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateResultSkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ResultSkillDomain Domain { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssignSkillsToClassDto
    {
        public int SchoolClassId { get; set; }
        public List<int> ResultSkillIds { get; set; } = new();
    }

    public class StudentResultSkillRatingDto
    {
        public int ResultSkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public ResultSkillDomain Domain { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public byte Score { get; set; }
        public string ScoreLabel { get; set; } = string.Empty;
    }

    public class TerminalResultAcademicRowDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public List<ResultTableAssessmentDto> Assessments { get; set; } = new();
        public decimal TotalScore { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public string SubjectPosition { get; set; } = string.Empty;
        public decimal HighestScore { get; set; }
        public decimal LowestScore { get; set; }
        public decimal ClassAverage { get; set; }
    }

    public class ResultTableAssessmentDto
    {
        public string AssessmentName { get; set; } = string.Empty;
        public double AssessmentScore { get; set; }
        public double? StudentScore { get; set; }
    }

    public class TerminalResultDto
    {
        public long TermRegId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string ResultType { get; set; } = string.Empty;
        public int? Attendance { get; set; }
        public decimal AcademicTotal { get; set; }
        public decimal AcademicAverage { get; set; }
        public List<TerminalResultAcademicRowDto> AcademicRows { get; set; } = new();
        public List<StudentResultSkillRatingDto> AffectiveSkills { get; set; } = new();
        public List<StudentResultSkillRatingDto> PsychomotorSkills { get; set; } = new();
        public bool RatingsGenerated { get; set; }
        public string RatingMessage { get; set; } = string.Empty;
    }
}
