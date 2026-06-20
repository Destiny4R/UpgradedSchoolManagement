using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Seeders
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var permissions = new List<Permission>
            {
                new() { Name = "View Dashboard", Code = PermissionConstants.Dashboard.View, Module = "Dashboard", Description = "Access to view dashboard" },

                new() { Name = "View Students", Code = PermissionConstants.Students.View, Module = "Students", Description = "View student records" },
                new() { Name = "Create Student", Code = PermissionConstants.Students.Create, Module = "Students", Description = "Create new student records" },
                new() { Name = "Edit Student", Code = PermissionConstants.Students.Edit, Module = "Students", Description = "Edit student records" },
                new() { Name = "Delete Student", Code = PermissionConstants.Students.Delete, Module = "Students", Description = "Delete student records" },
                new() { Name = "Promote Student", Code = PermissionConstants.Students.Promote, Module = "Students", Description = "Promote students to next class" },
                new() { Name = "Import Students", Code = PermissionConstants.Students.Import, Module = "Students", Description = "Import students from Excel" },
                new() { Name = "Export Students", Code = PermissionConstants.Students.Export, Module = "Students", Description = "Export student records" },

                new() { Name = "View Teachers", Code = PermissionConstants.Teachers.View, Module = "Teachers", Description = "View teacher records" },
                new() { Name = "Create Teacher", Code = PermissionConstants.Teachers.Create, Module = "Teachers", Description = "Create new teacher records" },
                new() { Name = "Edit Teacher", Code = PermissionConstants.Teachers.Edit, Module = "Teachers", Description = "Edit teacher records" },
                new() { Name = "Delete Teacher", Code = PermissionConstants.Teachers.Delete, Module = "Teachers", Description = "Delete teacher records" },

                new() { Name = "View Attendance", Code = PermissionConstants.Attendance.View, Module = "Attendance", Description = "View attendance records" },
                new() { Name = "Take Attendance", Code = PermissionConstants.Attendance.Take, Module = "Attendance", Description = "Record attendance" },
                new() { Name = "Edit Attendance", Code = PermissionConstants.Attendance.Edit, Module = "Attendance", Description = "Edit attendance records" },
                new() { Name = "Approve Attendance", Code = PermissionConstants.Attendance.Approve, Module = "Attendance", Description = "Approve attendance records" },

                new() { Name = "View Results", Code = PermissionConstants.Results.View, Module = "Results", Description = "View student results" },
                new() { Name = "Upload Results", Code = PermissionConstants.Results.Upload, Module = "Results", Description = "Upload student results" },
                new() { Name = "Edit Results", Code = PermissionConstants.Results.Edit, Module = "Results", Description = "Edit student results" },
                new() { Name = "Approve Results", Code = PermissionConstants.Results.Approve, Module = "Results", Description = "Approve student results" },
                new() { Name = "Publish Results", Code = PermissionConstants.Results.Publish, Module = "Results", Description = "Publish results for viewing" },

                new() { Name = "View Subjects", Code = PermissionConstants.Subjects.View, Module = "Subjects", Description = "View subjects" },
                new() { Name = "Create Subject", Code = PermissionConstants.Subjects.Create, Module = "Subjects", Description = "Create new subjects" },
                new() { Name = "Edit Subject", Code = PermissionConstants.Subjects.Edit, Module = "Subjects", Description = "Edit subjects" },
                new() { Name = "Delete Subject", Code = PermissionConstants.Subjects.Delete, Module = "Subjects", Description = "Delete subjects" },
                new() { Name = "Assign Subject", Code = PermissionConstants.Subjects.Assign, Module = "Subjects", Description = "Assign subjects to classes" },

                new() { Name = "View Classes", Code = PermissionConstants.Classes.View, Module = "Classes", Description = "View classes" },
                new() { Name = "Create Class", Code = PermissionConstants.Classes.Create, Module = "Classes", Description = "Create new classes" },
                new() { Name = "Edit Class", Code = PermissionConstants.Classes.Edit, Module = "Classes", Description = "Edit classes" },
                new() { Name = "Delete Class", Code = PermissionConstants.Classes.Delete, Module = "Classes", Description = "Delete classes" },
                new() { Name = "Manage Arms", Code = PermissionConstants.Classes.ManageArms, Module = "Classes", Description = "Manage class arms/sub-classes" },

                new() { Name = "View Sessions", Code = PermissionConstants.Sessions.View, Module = "Sessions", Description = "View academic sessions" },
                new() { Name = "Create Session", Code = PermissionConstants.Sessions.Create, Module = "Sessions", Description = "Create new sessions" },
                new() { Name = "Edit Session", Code = PermissionConstants.Sessions.Edit, Module = "Sessions", Description = "Edit sessions" },
                new() { Name = "Delete Session", Code = PermissionConstants.Sessions.Delete, Module = "Sessions", Description = "Delete sessions" },
                new() { Name = "Activate Session", Code = PermissionConstants.Sessions.Activate, Module = "Sessions", Description = "Activate/deactivate sessions" },

                new() { Name = "View Academic Structure", Code = PermissionConstants.AcademicStructure.View, Module = "Academic Structure", Description = "View academic structure" },
                new() { Name = "Manage Academic Structure", Code = PermissionConstants.AcademicStructure.Manage, Module = "Academic Structure", Description = "Manage academic structure" },

                new() { Name = "View Users", Code = PermissionConstants.Users.View, Module = "Users", Description = "View users" },
                new() { Name = "Create User", Code = PermissionConstants.Users.Create, Module = "Users", Description = "Create new users" },
                new() { Name = "Edit User", Code = PermissionConstants.Users.Edit, Module = "Users", Description = "Edit users" },
                new() { Name = "Delete User", Code = PermissionConstants.Users.Delete, Module = "Users", Description = "Delete users" },
                new() { Name = "Assign Role", Code = PermissionConstants.Users.AssignRole, Module = "Users", Description = "Assign roles to users" },

                new() { Name = "View Roles", Code = PermissionConstants.Roles.View, Module = "Roles", Description = "View roles" },
                new() { Name = "Create Role", Code = PermissionConstants.Roles.Create, Module = "Roles", Description = "Create new roles" },
                new() { Name = "Edit Role", Code = PermissionConstants.Roles.Edit, Module = "Roles", Description = "Edit roles" },
                new() { Name = "Delete Role", Code = PermissionConstants.Roles.Delete, Module = "Roles", Description = "Delete roles" },
                new() { Name = "Assign Permission", Code = PermissionConstants.Roles.AssignPermission, Module = "Roles", Description = "Assign permissions to roles" },

                new() { Name = "View Finance", Code = PermissionConstants.Finance.View, Module = "Finance", Description = "View financial records" },
                new() { Name = "Create Invoice", Code = PermissionConstants.Finance.InvoiceCreate, Module = "Finance", Description = "Create invoices" },
                new() { Name = "Record Payment", Code = PermissionConstants.Finance.PaymentRecord, Module = "Finance", Description = "Record payments" },

                new() { Name = "View Reports", Code = PermissionConstants.Reports.View, Module = "Reports", Description = "View reports" },
                new() { Name = "View Audit Logs", Code = PermissionConstants.AuditLogs.View, Module = "Audit Logs", Description = "View audit logs" },

                new() { Name = "View Settings", Code = PermissionConstants.Settings.View, Module = "Settings", Description = "View system settings" },
                new() { Name = "Manage Settings", Code = PermissionConstants.Settings.Manage, Module = "Settings", Description = "Manage system settings" },

                new() { Name = "Send Notification", Code = PermissionConstants.Notifications.Send, Module = "Notifications", Description = "Send notifications" },

                // FullControl permissions
                new() { Name = "Students Full Control", Code = PermissionConstants.Students.FullControl, Module = "Students", Description = "Full control over student management" },
                new() { Name = "Teachers Full Control", Code = PermissionConstants.Teachers.FullControl, Module = "Teachers", Description = "Full control over teacher management" },
                new() { Name = "Attendance Full Control", Code = PermissionConstants.Attendance.FullControl, Module = "Attendance", Description = "Full control over attendance" },
                new() { Name = "Results Full Control", Code = PermissionConstants.Results.FullControl, Module = "Results", Description = "Full control over results" },
                new() { Name = "Finance Full Control", Code = PermissionConstants.Finance.FullControl, Module = "Finance", Description = "Full control over finance" },
                new() { Name = "Subjects Full Control", Code = PermissionConstants.Subjects.FullControl, Module = "Subjects", Description = "Full control over subjects" },
                new() { Name = "Classes Full Control", Code = PermissionConstants.Classes.FullControl, Module = "Classes", Description = "Full control over classes" },
                new() { Name = "Sessions Full Control", Code = PermissionConstants.Sessions.FullControl, Module = "Sessions", Description = "Full control over sessions" },
                new() { Name = "Academic Structure Full Control", Code = PermissionConstants.AcademicStructure.FullControl, Module = "Academic Structure", Description = "Full control over academic structure" },
                new() { Name = "Users Full Control", Code = PermissionConstants.Users.FullControl, Module = "Users", Description = "Full control over users" },
                new() { Name = "Roles Full Control", Code = PermissionConstants.Roles.FullControl, Module = "Roles", Description = "Full control over roles" },
                new() { Name = "Reports Full Control", Code = PermissionConstants.Reports.FullControl, Module = "Reports", Description = "Full control over reports" },
                new() { Name = "Settings Full Control", Code = PermissionConstants.Settings.FullControl, Module = "Settings", Description = "Full control over settings" },
                new() { Name = "Audit Logs Full Control", Code = PermissionConstants.AuditLogs.FullControl, Module = "Audit Logs", Description = "Full control over audit logs" },
                new() { Name = "Dashboard Full Control", Code = PermissionConstants.Dashboard.FullControl, Module = "Dashboard", Description = "Full control over dashboard" },
                new() { Name = "Notifications Full Control", Code = PermissionConstants.Notifications.FullControl, Module = "Notifications", Description = "Full control over notifications" },
            };

            foreach (var permission in permissions)
            {
                var exists = await context.Permissions.AnyAsync(p => p.Code == permission.Code);
                if (!exists)
                {
                    context.Permissions.Add(permission);
                }
            }

            await context.SaveChangesAsync();
        }
    }

    public static class RoleSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var roles = new List<ApplicationRole>
            {
                new() { Name = "SuperAdmin", Description = "Full system access - can manage all schools and settings", NormalizedName = "SUPERADMIN" },
                new() { Name = "SchoolAdmin", Description = "Manages a specific school - teachers, students, classes, and academic structure", NormalizedName = "SCHOOLADMIN" },
                new() { Name = "Principal", Description = "Academic and administrative oversight", NormalizedName = "PRINCIPAL" },
                new() { Name = "Teacher", Description = "Classroom activities and student management", NormalizedName = "TEACHER" },
                new() { Name = "Accountant", Description = "Financial operations and fee management", NormalizedName = "ACCOUNTANT" },
                new() { Name = "Parent", Description = "Access to children's records only", NormalizedName = "PARENT" },
                new() { Name = "Student", Description = "Academic access to own records", NormalizedName = "STUDENT" },
            };

            foreach (var role in roles)
            {
                var exists = await context.Roles.AnyAsync(r => r.Name == role.Name);
                if (!exists)
                {
                    context.Roles.Add(role);
                }
            }

            await context.SaveChangesAsync();
        }
    }

    public static class RolePermissionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            var schoolAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SchoolAdmin");
            var principalRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Principal");
            var teacherRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Teacher");
            var accountantRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Accountant");
            var parentRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Parent");
            var studentRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");

            var allPermissions = await context.Permissions.ToListAsync();

            if (superAdminRole != null)
            {
                foreach (var permission in allPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == permission.Id);
                    if (!exists)
                    {
                        context.RolePermissions.Add(new RolePermission { RoleId = superAdminRole.Id, PermissionId = permission.Id });
                    }
                }
            }

            if (schoolAdminRole != null)
            {
                var schoolAdminPermissions = new[]
                {
                    PermissionConstants.Dashboard.View,
                    PermissionConstants.Students.View, PermissionConstants.Students.Create, PermissionConstants.Students.Edit, PermissionConstants.Students.Delete, PermissionConstants.Students.Import, PermissionConstants.Students.Export,
                    PermissionConstants.Teachers.View, PermissionConstants.Teachers.Create, PermissionConstants.Teachers.Edit,
                    PermissionConstants.Attendance.View, PermissionConstants.Attendance.Take, PermissionConstants.Attendance.Edit, PermissionConstants.Attendance.Approve,
                    PermissionConstants.Results.View, PermissionConstants.Results.Upload, PermissionConstants.Results.Edit, PermissionConstants.Results.Approve, PermissionConstants.Results.Publish,
                    PermissionConstants.Subjects.View, PermissionConstants.Subjects.Create, PermissionConstants.Subjects.Edit, PermissionConstants.Subjects.Assign,
                    PermissionConstants.Classes.View, PermissionConstants.Classes.Create, PermissionConstants.Classes.Edit, PermissionConstants.Classes.ManageArms,
                    PermissionConstants.Sessions.View, PermissionConstants.Sessions.Create, PermissionConstants.Sessions.Edit, PermissionConstants.Sessions.Activate,
                    PermissionConstants.AcademicStructure.View, PermissionConstants.AcademicStructure.Manage,
                    PermissionConstants.Users.View, PermissionConstants.Users.Create, PermissionConstants.Users.Edit,
                    PermissionConstants.Finance.View, PermissionConstants.Finance.InvoiceCreate, PermissionConstants.Finance.PaymentRecord,
                    PermissionConstants.Reports.View,
                    PermissionConstants.Settings.View, PermissionConstants.Settings.Manage,
                    PermissionConstants.AuditLogs.View,
                    PermissionConstants.Notifications.Send,
                };

                foreach (var code in schoolAdminPermissions)
                {
                    var permission = allPermissions.FirstOrDefault(p => p.Code == code);
                    if (permission != null)
                    {
                        var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == schoolAdminRole.Id && rp.PermissionId == permission.Id);
                        if (!exists)
                        {
                            context.RolePermissions.Add(new RolePermission { RoleId = schoolAdminRole.Id, PermissionId = permission.Id });
                        }
                    }
                }
            }

            if (principalRole != null)
            {
                var principalPermissions = new[]
                {
                    PermissionConstants.Dashboard.View,
                    PermissionConstants.Students.View,
                    PermissionConstants.Teachers.View,
                    PermissionConstants.Attendance.View,
                    PermissionConstants.Results.View, PermissionConstants.Results.Approve, PermissionConstants.Results.Publish,
                    PermissionConstants.Subjects.View,
                    PermissionConstants.Classes.View,
                    PermissionConstants.Sessions.View,
                    PermissionConstants.AcademicStructure.View,
                    PermissionConstants.Reports.View, PermissionConstants.Reports.Academic,
                    PermissionConstants.Settings.View,
                    PermissionConstants.Notifications.Send,
                };

                foreach (var code in principalPermissions)
                {
                    var permission = allPermissions.FirstOrDefault(p => p.Code == code);
                    if (permission != null)
                    {
                        var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == principalRole.Id && rp.PermissionId == permission.Id);
                        if (!exists)
                        {
                            context.RolePermissions.Add(new RolePermission { RoleId = principalRole.Id, PermissionId = permission.Id });
                        }
                    }
                }
            }

            if (teacherRole != null)
            {
                var teacherPermissions = new[]
                {
                    PermissionConstants.Dashboard.View,
                    PermissionConstants.Students.View,
                    PermissionConstants.Attendance.View, PermissionConstants.Attendance.Take,
                    PermissionConstants.Results.View, PermissionConstants.Results.Upload, PermissionConstants.Results.Edit,
                    PermissionConstants.Subjects.View,
                    PermissionConstants.Classes.View,
                    PermissionConstants.Sessions.View,
                    PermissionConstants.Notifications.View,
                };

                foreach (var code in teacherPermissions)
                {
                    var permission = allPermissions.FirstOrDefault(p => p.Code == code);
                    if (permission != null)
                    {
                        var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == teacherRole.Id && rp.PermissionId == permission.Id);
                        if (!exists)
                        {
                            context.RolePermissions.Add(new RolePermission { RoleId = teacherRole.Id, PermissionId = permission.Id });
                        }
                    }
                }
            }

            if (accountantRole != null)
            {
                var accountantPermissions = new[]
                {
                    PermissionConstants.Dashboard.View,
                    PermissionConstants.Finance.View, PermissionConstants.Finance.InvoiceCreate, PermissionConstants.Finance.InvoiceEdit, PermissionConstants.Finance.PaymentRecord,
                    PermissionConstants.Reports.View, PermissionConstants.Reports.Finance,
                    PermissionConstants.Settings.View,
                };

                foreach (var code in accountantPermissions)
                {
                    var permission = allPermissions.FirstOrDefault(p => p.Code == code);
                    if (permission != null)
                    {
                        var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == accountantRole.Id && rp.PermissionId == permission.Id);
                        if (!exists)
                        {
                            context.RolePermissions.Add(new RolePermission { RoleId = accountantRole.Id, PermissionId = permission.Id });
                        }
                    }
                }
            }

            if (parentRole != null)
            {
                var parentPermissions = new[]
                {
                    PermissionConstants.Dashboard.View,
                    PermissionConstants.Students.View,
                    PermissionConstants.Attendance.View,
                    PermissionConstants.Results.View,
                    PermissionConstants.Notifications.View,
                };

                foreach (var code in parentPermissions)
                {
                    var permission = allPermissions.FirstOrDefault(p => p.Code == code);
                    if (permission != null)
                    {
                        var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == parentRole.Id && rp.PermissionId == permission.Id);
                        if (!exists)
                        {
                            context.RolePermissions.Add(new RolePermission { RoleId = parentRole.Id, PermissionId = permission.Id });
                        }
                    }
                }
            }

            if (studentRole != null)
            {
                var studentPermissions = new[]
                {
                    PermissionConstants.Dashboard.View,
                    PermissionConstants.Results.View,
                    PermissionConstants.Attendance.View,
                    PermissionConstants.Notifications.View,
                };

                foreach (var code in studentPermissions)
                {
                    var permission = allPermissions.FirstOrDefault(p => p.Code == code);
                    if (permission != null)
                    {
                        var exists = await context.RolePermissions.AnyAsync(rp => rp.RoleId == studentRole.Id && rp.PermissionId == permission.Id);
                        if (!exists)
                        {
                            context.RolePermissions.Add(new RolePermission { RoleId = studentRole.Id, PermissionId = permission.Id });
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }

    public static class AdminUserSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext _context)
        {
            var adminEmail = "admin@edusphere.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                {
                    var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
                    if (superAdminRole != null)
                    {
                        await userManager.AddToRoleAsync(admin, superAdminRole.Name);
                    }

                    var settings = _context.Appsettings.FirstOrDefault(n => n.ApplicationUserId == admin.Id);
                    if (settings == null)
                    {
                        _context.Appsettings.Add(new AppSettings
                        {
                            ApplicationUserId = admin.Id,
                            IsAdmin = true,
                            CanPrintResult = true,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}