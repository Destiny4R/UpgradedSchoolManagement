namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface ISidebarService
    {
        Task<SidebarModel> GetSidebarAsync(string userId);
    }

    public class SidebarModel
    {
        public List<SidebarSection> Sections { get; set; } = new();
    }

    public class SidebarSection
    {
        public string Label { get; set; } = string.Empty;
        public List<SidebarItem> Items { get; set; } = new();
    }

    public class SidebarItem
    {
        public string Text { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? Badge { get; set; }
        public List<SidebarItem>? Children { get; set; }
    }
}
