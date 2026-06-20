namespace UpgradedSchoolManagementModels
{
    public static class PermissionHelper
    {
        private static readonly Dictionary<string, string[]> ModulePermissions = new()
        {
            ["student"] = new[]
            {
                PermissionConstants.Students.View,
                PermissionConstants.Students.Create,
                PermissionConstants.Students.Edit,
                PermissionConstants.Students.Delete,
                PermissionConstants.Students.Promote,
                PermissionConstants.Students.Import,
                PermissionConstants.Students.Export,
            },
            ["teacher"] = new[]
            {
                PermissionConstants.Teachers.View,
                PermissionConstants.Teachers.Create,
                PermissionConstants.Teachers.Edit,
                PermissionConstants.Teachers.Delete,
                PermissionConstants.Teachers.Promote,
                PermissionConstants.Teachers.Import,
                PermissionConstants.Teachers.Export,
            },
            ["attendance"] = new[]
            {
                PermissionConstants.Attendance.View,
                PermissionConstants.Attendance.Take,
                PermissionConstants.Attendance.Edit,
                PermissionConstants.Attendance.Approve,
                PermissionConstants.Attendance.Export,
            },
            ["result"] = new[]
            {
                PermissionConstants.Results.View,
                PermissionConstants.Results.Upload,
                PermissionConstants.Results.Edit,
                PermissionConstants.Results.Approve,
                PermissionConstants.Results.Publish,
                PermissionConstants.Results.Export,
            },
            ["finance"] = new[]
            {
                PermissionConstants.Finance.View,
                PermissionConstants.Finance.InvoiceCreate,
                PermissionConstants.Finance.InvoiceEdit,
                PermissionConstants.Finance.InvoiceDelete,
                PermissionConstants.Finance.PaymentRecord,
                PermissionConstants.Finance.PaymentApprove,
                PermissionConstants.Finance.Report,
                PermissionConstants.Finance.Export,
            },
            ["subject"] = new[]
            {
                PermissionConstants.Subjects.View,
                PermissionConstants.Subjects.Create,
                PermissionConstants.Subjects.Edit,
                PermissionConstants.Subjects.Delete,
                PermissionConstants.Subjects.Assign,
            },
            ["class"] = new[]
            {
                PermissionConstants.Classes.View,
                PermissionConstants.Classes.Create,
                PermissionConstants.Classes.Edit,
                PermissionConstants.Classes.Delete,
                PermissionConstants.Classes.ManageArms,
            },
            ["session"] = new[]
            {
                PermissionConstants.Sessions.View,
                PermissionConstants.Sessions.Create,
                PermissionConstants.Sessions.Edit,
                PermissionConstants.Sessions.Delete,
                PermissionConstants.Sessions.Activate,
            },
            ["academic_structure"] = new[]
            {
                PermissionConstants.AcademicStructure.View,
                PermissionConstants.AcademicStructure.Manage,
            },
            ["user"] = new[]
            {
                PermissionConstants.Users.View,
                PermissionConstants.Users.Create,
                PermissionConstants.Users.Edit,
                PermissionConstants.Users.Delete,
                PermissionConstants.Users.AssignRole,
            },
            ["role"] = new[]
            {
                PermissionConstants.Roles.View,
                PermissionConstants.Roles.Create,
                PermissionConstants.Roles.Edit,
                PermissionConstants.Roles.Delete,
                PermissionConstants.Roles.AssignPermission,
            },
            ["report"] = new[]
            {
                PermissionConstants.Reports.View,
                PermissionConstants.Reports.Academic,
                PermissionConstants.Reports.Finance,
                PermissionConstants.Reports.Attendance,
                PermissionConstants.Reports.Export,
            },
            ["settings"] = new[]
            {
                PermissionConstants.Settings.View,
                PermissionConstants.Settings.Manage,
            },
            ["audit_log"] = new[]
            {
                PermissionConstants.AuditLogs.View,
                PermissionConstants.AuditLogs.Export,
            },
            ["dashboard"] = new[]
            {
                PermissionConstants.Dashboard.View,
            },
            ["notification"] = new[]
            {
                PermissionConstants.Notifications.View,
                PermissionConstants.Notifications.Send,
                PermissionConstants.Notifications.Manage,
            },
        };

        public static List<string> ExpandPermissions(IEnumerable<string> permissions)
        {
            var result = new HashSet<string>();
            foreach (var perm in permissions)
            {
                result.Add(perm);
                if (!perm.EndsWith(".fullcontrol")) continue;
                var prefix = perm[..^".fullcontrol".Length];
                if (ModulePermissions.TryGetValue(prefix, out var expanded))
                {
                    foreach (var p in expanded)
                    {
                        result.Add(p);
                    }
                }
            }
            return result.ToList();
        }

        public static string GetModulePrefix(string permissionCode)
        {
            var dotIndex = permissionCode.IndexOf('.');
            return dotIndex > 0 ? permissionCode[..dotIndex] : permissionCode;
        }
    }
}
