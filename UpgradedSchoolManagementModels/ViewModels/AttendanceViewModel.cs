using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.ViewModels
{
    public class AttendanceViewModel
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RegNumber { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public string SchoolClass { get; set; } = string.Empty;
        public int SchoolAttendance { get; set; }
        public int StudentAttendance { get; set; }
    }
}
