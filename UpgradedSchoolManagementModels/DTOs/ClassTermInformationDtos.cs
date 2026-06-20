namespace UpgradedSchoolManagementModels.DTOs
{
    public class ClassTermInformationRequest
    {
        public int Id { get; set; }
        public int Term { get; set; }
        public int SchoolClassId { get; set; }
        public int SubClassId { get; set; }
        public int SessionId { get; set; }
        public decimal NextTermFees { get; set; }
        public string ClassTeacherName { get; set; } = string.Empty;
        public string? ApplicationUserId { get; set; }
    }

    public class ClassTermInformationDto
    {
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public string SchoolClass { get; set; } = string.Empty;
        public string SubClass { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public decimal NextTermFees { get; set; }
        public string ClassTeacherName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
