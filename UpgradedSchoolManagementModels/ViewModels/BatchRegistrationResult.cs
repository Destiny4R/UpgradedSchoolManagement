using System.Collections.Generic;

namespace UpgradedSchoolManagementModels.ViewModels
{
    public class BatchRegistrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
