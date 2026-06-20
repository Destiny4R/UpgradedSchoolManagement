namespace UpgradedSchoolManagementModels.Models
{
    public class SchoolConfigurationSetup
    {
        public const string SectionName = "SchoolConfigurationSetup";
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Motto { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }
}
