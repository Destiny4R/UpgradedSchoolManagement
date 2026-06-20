using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.DTOs
{
    public class ResultSheetDto
    {
        public long termRegId { get; set; }
        public string name { get; set; }
        public string regnumber { get; set; }
        public string term { get; set; }
        public string session { get; set; }
        public string schoolclass { get; set; }
        public string subjects { get; set; }
        public bool resultStatus { get; set; }
        public bool attendance { get; set; }
    }
    public class SubjectResultDto
    {
        public long ResultTableId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public double? ScoreOne { get; set; }
        public double? ScoreTwo { get; set; }
        public double? ScoreThree { get; set; }
        public double? ScoreFour { get; set; }
        public double? ScoreFive { get; set; }
        public double? ScoreSix { get; set; }
        public decimal TotalScore { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    public class EditResultDto
    {
        public long TermRegId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public List<AssessmentConfigDto> AssessmentConfigs { get; set; } = new();
        public List<SubjectResultDto> Subjects { get; set; } = new();
    }

    public class SaveResultDto
    {
        public long TermRegId { get; set; }
        public List<SaveSubjectResultDto> Subjects { get; set; } = new();
    }

    public class SaveSubjectResultDto
    {
        public long ResultTableId { get; set; }
        public int SubjectId { get; set; }
        public double? ScoreOne { get; set; }
        public double? ScoreTwo { get; set; }
        public double? ScoreThree { get; set; }
        public double? ScoreFour { get; set; }
        public double? ScoreFive { get; set; }
        public double? ScoreSix { get; set; }
    }

    public class TermObjects
    {
        public int SessionId { get; set; }
        public Term Term { get; set; }
        public int schoolClassId { get; set; }
        public int SubclassId { get; set; }
    }

    public class AssessmentSheetDto
    {
        public int Sn { get; set; }
        public long TermRegId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNumber { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public double? ScoreOne { get; set; }
        public double? ScoreTwo { get; set; }
        public double? ScoreThree { get; set; }
        public double? ScoreFour { get; set; }
        public double? ScoreFive { get; set; }
        public double? ScoreSix { get; set; }
    }

    public class AssessmentImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
