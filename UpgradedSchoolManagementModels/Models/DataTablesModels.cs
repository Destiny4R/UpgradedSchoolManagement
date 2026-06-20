namespace UpgradedSchoolManagementModels.Models
{
    public class DataTablesResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public string? Error { get; set; }
    }

    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public int PageSize => Length;
        public int PageIndex => Start / Length;
        public SearchInfo? Search { get; set; }
        public List<OrderInfo> Order { get; set; } = new();
        public List<ColumnInfo> Columns { get; set; } = new();
    }

    public class SearchInfo
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class OrderInfo
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    public class ColumnInfo
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public SearchInfo? Search { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
    }
}