namespace UpgradedSchoolManagementModels
{
    public static class PermissionConstants
    {
        public static class Students
        {
            public const string FullControl = "student.fullcontrol";
            public const string View = "student.view";
            public const string Create = "student.create";
            public const string Edit = "student.edit";
            public const string Delete = "student.delete";
            public const string Promote = "student.promote";
            public const string Import = "student.import";
            public const string Export = "student.export";
        }

        public static class Teachers
        {
            public const string FullControl = "teacher.fullcontrol";
            public const string View = "teacher.view";
            public const string Create = "teacher.create";
            public const string Edit = "teacher.edit";
            public const string Delete = "teacher.delete";
            public const string Promote = "teacher.promote";
            public const string Import = "teacher.import";
            public const string Export = "teacher.export";
        }

        public static class Attendance
        {
            public const string FullControl = "attendance.fullcontrol";
            public const string View = "attendance.view";
            public const string Take = "attendance.take";
            public const string Edit = "attendance.edit";
            public const string Approve = "attendance.approve";
            public const string Export = "attendance.export";
        }

        public static class Results
        {
            public const string FullControl = "result.fullcontrol";
            public const string View = "result.view";
            public const string Upload = "result.upload";
            public const string Edit = "result.edit";
            public const string Approve = "result.approve";
            public const string Publish = "result.publish";
            public const string Export = "result.export";
        }

        public static class Finance
        {
            public const string FullControl = "finance.fullcontrol";
            public const string View = "finance.view";
            public const string InvoiceCreate = "finance.invoice.create";
            public const string InvoiceEdit = "finance.invoice.edit";
            public const string InvoiceDelete = "finance.invoice.delete";
            public const string PaymentRecord = "finance.payment.record";
            public const string PaymentApprove = "finance.payment.approve";
            public const string Report = "finance.report.view";
            public const string Export = "finance.export";
        }

        public static class Subjects
        {
            public const string FullControl = "subject.fullcontrol";
            public const string View = "subject.view";
            public const string Create = "subject.create";
            public const string Edit = "subject.edit";
            public const string Delete = "subject.delete";
            public const string Assign = "subject.assign";
        }

        public static class Classes
        {
            public const string FullControl = "class.fullcontrol";
            public const string View = "class.view";
            public const string Create = "class.create";
            public const string Edit = "class.edit";
            public const string Delete = "class.delete";
            public const string ManageArms = "class.manage_arms";
        }

        public static class Sessions
        {
            public const string FullControl = "session.fullcontrol";
            public const string View = "session.view";
            public const string Create = "session.create";
            public const string Edit = "session.edit";
            public const string Delete = "session.delete";
            public const string Activate = "session.activate";
        }

        public static class AcademicStructure
        {
            public const string FullControl = "academic_structure.fullcontrol";
            public const string View = "academic_structure.view";
            public const string Manage = "academic_structure.manage";
        }

        public static class Users
        {
            public const string FullControl = "user.fullcontrol";
            public const string View = "user.view";
            public const string Create = "user.create";
            public const string Edit = "user.edit";
            public const string Delete = "user.delete";
            public const string AssignRole = "user.assign_role";
        }

        public static class Roles
        {
            public const string FullControl = "role.fullcontrol";
            public const string View = "role.view";
            public const string Create = "role.create";
            public const string Edit = "role.edit";
            public const string Delete = "role.delete";
            public const string AssignPermission = "role.assign_permission";
        }

        public static class Reports
        {
            public const string FullControl = "report.fullcontrol";
            public const string View = "report.view";
            public const string Academic = "report.academic";
            public const string Finance = "report.finance";
            public const string Attendance = "report.attendance";
            public const string Export = "report.export";
        }

        public static class Settings
        {
            public const string FullControl = "settings.fullcontrol";
            public const string View = "settings.view";
            public const string Manage = "settings.manage";
        }

        public static class AuditLogs
        {
            public const string FullControl = "audit_log.fullcontrol";
            public const string View = "audit_log.view";
            public const string Export = "audit_log.export";
        }

        public static class Dashboard
        {
            public const string FullControl = "dashboard.fullcontrol";
            public const string View = "dashboard.view";
        }

        public static class Notifications
        {
            public const string FullControl = "notification.fullcontrol";
            public const string View = "notification.view";
            public const string Send = "notification.send";
            public const string Manage = "notification.manage";
        }
    }
}