namespace UpgradedSchoolManagementModels.DTOs
{
    public class DashboardViewModel
    {
        public DashboardSummary Summary { get; set; } = new();
        public List<RevenueTrendPoint> RevenueTrend { get; set; } = new();
        public List<RegistrationTrendPoint> RegistrationTrend { get; set; } = new();
        public List<CompulsoryComplianceDto> CompulsoryCompliance { get; set; } = new();
        public StudentDistributionDto StudentDistribution { get; set; } = new();
        public List<AdmissionTrendDto> AdmissionTrend { get; set; } = new();
        public AcademicPerformanceDto AcademicPerformance { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<DashboardAlertDto> Alerts { get; set; } = new();
        public List<QuickActionDto> QuickActions { get; set; } = new();
        public TeacherActivityDto TeacherActivities { get; set; } = new();
        public List<ClassPerformanceDto> ClassPerformance { get; set; } = new();
    }

    public class DashboardSummary
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int AssignedSubjects { get; set; }
        public int TotalParents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalSessions { get; set; }
        public decimal TotalRevenueCollected { get; set; }
        public decimal OutstandingPayments { get; set; }
        public int TotalTermRegistrations { get; set; }
        public int ResultsPublished { get; set; }
        public int PendingResults { get; set; }
        public int TotalStudentsNew { get; set; }
        public int StudentsWithResults { get; set; }
        public int StudentsWithOutstandingCompliance { get; set; }
    }

    public class RevenueTrendPoint
    {
        public string SessionName { get; set; } = "";
        public string Term { get; set; } = "";
        public decimal Amount { get; set; }
        public int SortOrder { get; set; }
    }

    public class RegistrationTrendPoint
    {
        public string SessionName { get; set; } = "";
        public string Term { get; set; } = "";
        public int Count { get; set; }
        public int SortOrder { get; set; }
    }

    public class CompulsoryComplianceDto
    {
        public string SessionName { get; set; } = "";
        public string Term { get; set; } = "";
        public decimal Expected { get; set; }
        public decimal Collected { get; set; }
        public decimal Outstanding => Expected - Collected;
        public double CompliancePercent => Expected > 0 ? Math.Round((double)(Collected / Expected) * 100, 1) : 0;
        public int SortOrder { get; set; }
    }

    public class StudentDistributionDto
    {
        public List<NamedCount> ByClass { get; set; } = new();
        public List<NamedCount> BySubClass { get; set; } = new();
        public List<NamedCount> ByGender { get; set; } = new();
        public List<NamedCount> BySection { get; set; } = new();
    }

    public class NamedCount
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    public class AdmissionTrendDto
    {
        public string SessionName { get; set; } = "";
        public int Count { get; set; }
    }

    public class AcademicPerformanceDto
    {
        public double OverallAverage { get; set; }
        public double PassRate { get; set; }
        public double DistinctionRate { get; set; }
        public int TotalResults { get; set; }
        public List<NamedCount> PassRateByTerm { get; set; } = new();
        public List<NamedCount> DistinctionRateByTerm { get; set; } = new();
    }

    public class ClassPerformanceDto
    {
        public string ClassName { get; set; } = "";
        public double AverageScore { get; set; }
        public int StudentCount { get; set; }
        public double PassRate { get; set; }
    }

    public class RecentActivityDto
    {
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public string TimeAgo { get; set; } = "";
        public string Link { get; set; } = "";
    }

    public class DashboardAlertDto
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public string ActionLink { get; set; } = "";
        public string ActionText { get; set; } = "";
    }

    public class QuickActionDto
    {
        public string Title { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Link { get; set; } = "";
        public string Color { get; set; } = "";
    }

    public class TeacherActivityDto
    {
        public int TotalTeachers { get; set; }
        public int AssignedSubjects { get; set; }
        public int AssignedClasses { get; set; }
        public int ResultsSubmitted { get; set; }
        public int ResultsPending { get; set; }
    }

    public class DashboardTrendPoint
    {
        public string Label { get; set; } = "";
        public decimal Amount { get; set; }
    }
}
