namespace UpgradedSchoolManagementModels.DTOs
{
public class TermRegDto
    {
        public long   Id                 { get; set; }
        public int    StudentId          { get; set; }
        public int    SessionId          { get; set; }
        public int    SchoolClassId      { get; set; }
        public int    SubClassId         { get; set; }
        public string FirstName          { get; set; } = string.Empty;
        public string Surname            { get; set; } = string.Empty;
        public string? OtherName         { get; set; }
        public string FullName           { get; set; } = string.Empty;
        public string RegNumber          { get; set; } = string.Empty;
        public string Term               { get; set; } = string.Empty;
        public string Session            { get; set; } = string.Empty;
        public string SchoolClass        { get; set; } = string.Empty;
        public string SubClass           { get; set; } = string.Empty;
        public DateTime CreatedDate      { get; set; }
        public int NoOfSubjects          { get; set; }
        public bool HasRecordedResults   { get; set; }
        public int ResultType            { get; set; }
    }
}
