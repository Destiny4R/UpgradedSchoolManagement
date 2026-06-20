namespace UpgradedSchoolManagementModels.DTOs
{
    /// <summary>
    /// DTO for creating or updating a parent/guardian record.
    /// </summary>
    public class ParentGuardianCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Occupation { get; set; }
        public string? Address { get; set; }
        public string Phone1 { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
    }

    /// <summary>
    /// DTO for returning parent/guardian data to the client.
    /// </summary>
    public class ParentGuardianDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Occupation { get; set; }
        public string? Address { get; set; }
        public string Phone1 { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<StudentChildDto> Children { get; set; } = new();
    }

    public class StudentChildDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AdmissionNumber { get; set; }
    }

    /// <summary>
    /// DTO for the response when a phone conflict is detected.
    /// Indicates an existing parent/guardian and asks user for confirmation.
    /// </summary>
    public class ParentPhoneConflictDto
    {
        public int ExistingParentId { get; set; }
        public ParentGuardianDto ExistingParent { get; set; } = new();
        public string Message { get; set; } = "A parent/guardian with this phone number already exists.";
    }

    /// <summary>
    /// DTO for linking an existing parent to a student.
    /// </summary>
    public class LinkParentToStudentDto
    {
        public int StudentId { get; set; }
        public int ParentGuardianId { get; set; }
        public bool IsPrimaryContact { get; set; } = false;
    }

    /// <summary>
    /// Response DTO for add-or-link operations.
    /// </summary>
    public class ParentOperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ParentGuardianDto? Parent { get; set; }
        public bool IsConflict { get; set; } = false; // True if phone conflict detected
    }
}
