using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.ViewModels
{
    // ── Summary card data returned alongside every report ──

    public class ClassReportSummary
    {
        public int TotalStudents { get; set; }
        public int StudentsPaid { get; set; }
        public int StudentsOutstanding => TotalStudents - StudentsPaid;
        public decimal TotalExpected { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding => TotalExpected - TotalCollected;
    }

    public class SchoolReportSummary
    {
        public int TotalStudents { get; set; }
        public int StudentsPaid { get; set; }
        public decimal TotalExpected { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding => TotalExpected - TotalCollected;
        public List<CategoryRevenueSummary> CategoryBreakdown { get; set; } = new();
    }

    public class CategoryRevenueSummary
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalExpected { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal Outstanding => TotalExpected - TotalCollected;
    }

    public class CategoryItemReportSummary
    {
        public int TotalItems { get; set; }
        public int TotalStudentsPaid { get; set; }
        public decimal TotalAmountCollected { get; set; }
    }

    // ── Row-level data for DataTables ──

    /// <summary>
    /// One row per student in the class report table
    /// </summary>
    public class ClassReportRow
    {
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string CategoryName { get; set; }
        public string PaymentItemName { get; set; }
        public decimal Expected { get; set; }
        public decimal Paid { get; set; }
        public decimal Outstanding => Expected - Paid;
        public string Status => Paid >= Expected ? "Fully Paid" : Paid > 0 ? "Partial" : "Unpaid";
    }

    /// <summary>
    /// One row per payment item in the school-wide report table (grouped by category)
    /// </summary>
    public class SchoolReportRow
    {
        public string CategoryName { get; set; }
        public string ItemName { get; set; }
        public int TotalStudents { get; set; }
        public int StudentsPaid { get; set; }
        public decimal TotalExpected { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal Outstanding => TotalExpected - TotalCollected;
        /// <summary>Number of items in this category (for rowspan rendering)</summary>
        public int CategoryItemCount { get; set; }
        /// <summary>True for the first item row in a category group</summary>
        public bool IsFirstInCategory { get; set; }
    }

    /// <summary>
    /// One row per payment item in the category-item report table
    /// </summary>
    public class CategoryItemReportRow
    {
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public int TotalStudents { get; set; }
        public int StudentsPaid { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal AmountCollected { get; set; }
        public decimal Outstanding => ExpectedAmount - AmountCollected;
    }

    // ── Wrapper responses ──

    public class ClassReportResponse
    {
        public ClassReportSummary Summary { get; set; } = new();
        public List<ClassReportRow> Rows { get; set; } = new();
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
    }

    public class SchoolReportResponse
    {
        public SchoolReportSummary Summary { get; set; } = new();
        public List<SchoolReportRow> Rows { get; set; } = new();
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
    }

    public class CategoryItemReportResponse
    {
        public CategoryItemReportSummary Summary { get; set; } = new();
        public List<CategoryItemReportRow> Rows { get; set; } = new();
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
    }

    // ── Dashboard ViewModels ──

    public class DashboardCategorySummary
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal Expected { get; set; }
        public decimal Collected { get; set; }
        public decimal Outstanding => Expected - Collected;
    }

    public class DashboardItemSummary
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal Expected { get; set; }
        public decimal Collected { get; set; }
        public decimal Outstanding => Expected - Collected;
    }

    public class DashboardCategoryTrend
    {
        public List<string> Sessions { get; set; } = new();
        public List<DashboardCategoryTrendSeries> Series { get; set; } = new();
    }

    public class DashboardCategoryTrendSeries
    {
        public string CategoryName { get; set; } = "";
        public List<decimal> Amounts { get; set; } = new();
    }

    public class DashboardItemChart
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Collected { get; set; } = new();
        public List<decimal> Expected { get; set; } = new();
    }

    public class DashboardDataRequest
    {
        public int? SessionId { get; set; }
        public int? TermId { get; set; }
    }

    public class DashboardTermRegistrationChart
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Counts { get; set; } = new();
    }

    public class RecentPaymentItem
    {
        public int Id { get; set; }
        public string Reference { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public DateTime PaymentDate { get; set; }
    }
}
