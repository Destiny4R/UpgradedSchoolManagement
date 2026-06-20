using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    /// <summary>
    /// Junction table linking students to their parents/guardians.
    /// Supports many-to-many relationship: one student can have multiple guardians,
    /// and one guardian can be linked to multiple students.
    /// </summary>
    public class StudentParentLink
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public StudentsTable? Student { get; set; }

        [Required]
        public int ParentGuardianId { get; set; }

        [ForeignKey(nameof(ParentGuardianId))]
        public ParentGuardian? ParentGuardian { get; set; }

        /// <summary>
        /// Timestamp when the link was created.
        /// </summary>
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates if this is the primary/emergency contact.
        /// </summary>
        public bool IsPrimaryContact { get; set; } = false;
    }
}
