using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    /// <summary>
    /// Stores the score a student earned for one subject in one term registration.
    /// Individual assessment scores are stored in child ResultScore rows,
    /// each mapped to an AssessmentConfiguration row (e.g. Class Work, Exam).
    /// </summary>
    public class ResultTable
    {
        public long Id { get; set; }
        public long TermRegId { get; set; }
        public int SubjectId { get; set; }
        public bool Status { get; set; } = false;
        public double? ScoreOne { get; set; }
        public double? ScoreTwo { get; set; }
        public double? ScoreThree { get; set; }
        public double? ScoreFour { get; set; }
        public double? ScoreFive { get; set; }
        public double? ScoreSix { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TermRegId))]
        public TermRegistration TermRegistration { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public SubjectTable Subject { get; set; }

        /// <summary>
        /// The individual assessment scores for this result entry.
        /// Each row corresponds to one AssessmentConfiguration (e.g. one row for "Class Work", one for "Exam").
        /// </summary>
        //public ICollection<ResultScore> Scores { get; set; } = new List<ResultScore>();
    }

    /// <summary>
    /// Stores one assessment score (e.g. Class Work = 8) for a given ResultTable row.
    /// Maps to an AssessmentConfiguration to know the assessment name, max score, and display order.
    /// </summary>
    //public class ResultScore
    //{
    //    public long Id { get; set; }

    //    public long ResultTableId { get; set; }
    //    [ForeignKey(nameof(ResultTableId))]
    //    public ResultTable ResultTable { get; set; }

    //    /// <summary>Links to the specific assessment type (e.g. "Class Work" with max 10).</summary>
    //    public int AssessmentConfigId { get; set; }
    //    [ForeignKey(nameof(AssessmentConfigId))]
    //    public AssessmentConfiguration AssessmentConfiguration { get; set; }

    //    /// <summary>The actual score the student received (must not exceed AssessmentConfiguration.AssessmentScore).</summary>
    //    public double Score { get; set; }
    //}
}
