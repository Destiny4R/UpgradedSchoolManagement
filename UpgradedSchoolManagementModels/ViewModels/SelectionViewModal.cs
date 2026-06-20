using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.ViewModels
{
    public class SelectionViewModal
    {
        public IEnumerable<SelectListItem> SchoolClasses { get; set; }
        public IEnumerable<SelectListItem> AcademicSession { get; set; }
        public IEnumerable<SelectListItem> Terms { get; set; }
        public IEnumerable<SelectListItem> SubClass { get; set; }
        public IEnumerable<SelectListItem> PaymentItems { get; set; }
        public IEnumerable<SelectListItem> Subjects { get; set; }
    }
}
