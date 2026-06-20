namespace UpgradedSchoolManagementModels.DTOs
{
    public class TermGeneralInformationRequest
    {
        public int Id { get; set; }
        public int Term { get; set; }
        public int SessionId { get; set; }
        public int DaySchoolOpen { get; set; }
        public string? PrincipalName { get; set; }
        public DateTime NextTermStart { get; set; }
        public DateTime NextTermEnd { get; set; }
        public string? ApplicationUserId { get; set; }
    }

    public class TermGeneralInformationDto
    {
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public int DaySchoolOpen { get; set; }
        public string? PrincipalName { get; set; }
        public DateTime NextTermStart { get; set; }
        public DateTime NextTermEnd { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
