using Microsoft.AspNetCore.Authorization;
using UpgradedSchoolManagementModels;

namespace UpgradedSchoolManagementWeb.Authorization
{
    public static class PermissionPolicies
    {
        public static void AddPolicies(this AuthorizationOptions options)
        {
            // Students
            options.AddPolicy("Student.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.View)));
            options.AddPolicy("Student.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.Create)));
            options.AddPolicy("Student.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.Edit)));
            options.AddPolicy("Student.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.Delete)));
            options.AddPolicy("Student.Promote", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Students.Promote)));

            // Teachers
            options.AddPolicy("Teacher.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Teachers.View)));
            options.AddPolicy("Teacher.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Teachers.Create)));
            options.AddPolicy("Teacher.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Teachers.Edit)));
            options.AddPolicy("Teacher.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Teachers.Delete)));

            // Attendance
            options.AddPolicy("Attendance.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Attendance.View)));
            options.AddPolicy("Attendance.Take", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Attendance.Take)));
            options.AddPolicy("Attendance.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Attendance.Edit)));

            // Results
            options.AddPolicy("Result.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Results.View)));
            options.AddPolicy("Result.Upload", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Results.Upload)));
            options.AddPolicy("Result.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Results.Edit)));
            options.AddPolicy("Result.Approve", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Results.Approve)));
            options.AddPolicy("Result.Publish", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Results.Publish)));

            // Subjects
            options.AddPolicy("Subject.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Subjects.View)));
            options.AddPolicy("Subject.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Subjects.Create)));
            options.AddPolicy("Subject.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Subjects.Edit)));
            options.AddPolicy("Subject.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Subjects.Delete)));

            // Classes
            options.AddPolicy("Class.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Classes.View)));
            options.AddPolicy("Class.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Classes.Create)));
            options.AddPolicy("Class.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Classes.Edit)));
            options.AddPolicy("Class.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Classes.Delete)));

            // Sessions
            options.AddPolicy("Session.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Sessions.View)));
            options.AddPolicy("Session.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Sessions.Create)));
            options.AddPolicy("Session.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Sessions.Edit)));
            options.AddPolicy("Session.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Sessions.Delete)));

            // Academic Structure
            options.AddPolicy("AcademicStructure.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.AcademicStructure.View)));
            options.AddPolicy("AcademicStructure.Manage", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.AcademicStructure.Manage)));

            // Users
            options.AddPolicy("User.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Users.View)));
            options.AddPolicy("User.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Users.Create)));
            options.AddPolicy("User.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Users.Edit)));
            options.AddPolicy("User.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Users.Delete)));
            options.AddPolicy("User.AssignRole", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Users.AssignRole)));

            // Roles
            options.AddPolicy("Role.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Roles.View)));
            options.AddPolicy("Role.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Roles.Create)));
            options.AddPolicy("Role.Edit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Roles.Edit)));
            options.AddPolicy("Role.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Roles.Delete)));
            options.AddPolicy("Role.AssignPermission", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Roles.AssignPermission)));

            // Finance
            options.AddPolicy("Finance.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Finance.View)));
            options.AddPolicy("Finance.InvoiceCreate", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Finance.InvoiceCreate)));
            options.AddPolicy("Finance.InvoiceEdit", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Finance.InvoiceEdit)));
            options.AddPolicy("Finance.InvoiceDelete", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Finance.InvoiceDelete)));
            options.AddPolicy("Finance.PaymentRecord", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Finance.PaymentRecord)));
            options.AddPolicy("Finance.PaymentApprove", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Finance.PaymentApprove)));
            options.AddPolicy("Finance.Report", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Finance.Report)));

            // Reports
            options.AddPolicy("Report.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Reports.View)));
            options.AddPolicy("Report.Academic", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Reports.Academic)));
            options.AddPolicy("Report.Finance", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Reports.Finance)));

            // Settings
            options.AddPolicy("Settings.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Settings.View)));
            options.AddPolicy("Settings.Manage", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Settings.Manage)));

            // Audit Logs
            options.AddPolicy("AuditLog.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.AuditLogs.View)));

            // Dashboard
            options.AddPolicy("Dashboard.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Dashboard.View)));

            // Notifications
            options.AddPolicy("Notification.View", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Notifications.View)));
            options.AddPolicy("Notification.Send", policy =>
                policy.Requirements.Add(new PermissionRequirement(PermissionConstants.Notifications.Send)));
        }
    }
}
