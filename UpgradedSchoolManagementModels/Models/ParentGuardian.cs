using System.ComponentModel.DataAnnotations;

namespace UpgradedSchoolManagementModels.Models
{
    /// <summary>
    /// Represents a Parent or Guardian of one or more students.
    /// Phone numbers are treated as unique identifiers to prevent duplicates.
    /// One parent can be linked to multiple students; one student can have multiple guardians.
    /// </summary>
    public class ParentGuardian
    {
        public int Id { get; set; }

        [StringLength(100)]
        [Required]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50)]
        [Required]
        public string Relationship { get; set; } = string.Empty; // e.g., Father, Mother, Guardian, Aunt, etc.

        [StringLength(100)]
        public string? Occupation { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        /// <summary>
        /// Primary phone number. Stored normalized (digits only).
        /// Unique constraint enforced at database level.
        /// </summary>
        [StringLength(20)]
        [Required]
        public string Phone1 { get; set; } = string.Empty;

        /// <summary>
        /// Secondary phone number (optional). Stored normalized (digits only).
        /// Unique constraint enforced at database level if provided.
        /// </summary>
        [StringLength(20)]
        public string? Phone2 { get; set; }

        /// <summary>
        /// Tracks when the record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tracks when the record was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property: students linked to this parent/guardian.
        /// </summary>
        public ICollection<StudentParentLink> StudentLinks { get; set; } = new List<StudentParentLink>();
    }
}
