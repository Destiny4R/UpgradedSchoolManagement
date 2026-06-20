using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class SidebarService : ISidebarService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public SidebarService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<SidebarModel> GetSidebarAsync(string userId)
        {
            try
            {
                var permissions = await GetUserPermissionsAsync(userId);

                var sidebar = new SidebarModel();

                sidebar.Sections.Add(new SidebarSection
                {
                    Label = "Overview",
                    Items = new List<SidebarItem>
                    {
                        new SidebarItem { Text = "Dashboard", Icon = "bi-grid-1x2-fill", Url = "/Admin/Dashboard" }
                    }
                });

                if (permissions.Any(p => p.Code == PermissionConstants.Students.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "People",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem
                            {
                                Text = "Students",
                                Icon = "bi-people-fill",
                                Children = BuildStudentChildren(permissions)
                            }
                        }
                    });
                }

                if (permissions.Any(p => p.Code == PermissionConstants.Teachers.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "Staff",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem
                            {
                                Text = "Teachers",
                                Icon = "bi-person-badge-fill",
                                Children = BuildTeacherChildren(permissions)
                            }
                        }
                    });
                }

                if (permissions.Any(p => p.Code == PermissionConstants.AcademicStructure.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "Academic",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem { Text = "Sessions", Icon = "bi-calendar-range-fill", Url = "/Admin/Academic/manage-session" },
                            new SidebarItem { Text = "Classes", Icon = "bi-mortarboard-fill", Url = "/Admin/Academic/manage-school" },
                            new SidebarItem { Text = "Sub-Classes", Icon = "bi-intersect", Url = "/Admin/Academic/manage-subclass" },
                            new SidebarItem { Text = "Subjects", Icon = "bi-journal-bookmark-fill", Url = "/Admin/Academic/manage-subjects" }
                        }
                    });
                }

                if (permissions.Any(p => p.Code == PermissionConstants.Attendance.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "Attendance",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem { Text = "Daily Attendance", Icon = "bi-clipboard2-check-fill", Url = "/Admin/Attendance" },
                            new SidebarItem { Text = "Reports", Icon = "bi-file-earmark-bar-graph-fill", Url = "/Admin/Attendance/Reports" }
                        }
                    });
                }

                if (permissions.Any(p => p.Code == PermissionConstants.Results.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "Results",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem { Text = "Results Entry", Icon = "bi-pencil-square", Url = "/Admin/Results/Entry" },
                            new SidebarItem { Text = "Report Cards", Icon = "bi-file-earmark-text-fill", Url = "/Admin/Results/Reports" }
                        }
                    });
                }

                if (permissions.Any(p => p.Code == PermissionConstants.Finance.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "Finance",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem { Text = "Fee Collection", Icon = "bi-cash-coin", Url = "/Admin/Finance/Fees" },
                            new SidebarItem { Text = "Invoices", Icon = "bi-receipt", Url = "/Admin/Finance/Invoices" },
                            new SidebarItem { Text = "Reports", Icon = "bi-bar-chart-fill", Url = "/Admin/Finance/Reports" }
                        }
                    });
                }

                if (permissions.Any(p => p.Code == PermissionConstants.Users.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "System",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem { Text = "Users", Icon = "bi-people-fill", Url = "/Admin/Users" },
                            new SidebarItem { Text = "Roles", Icon = "bi-shield-fill", Url = "/Admin/Roles" }
                        }
                    });
                }

                if (permissions.Any(p => p.Code == PermissionConstants.Settings.View))
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Label = "Settings",
                        Items = new List<SidebarItem>
                        {
                            new SidebarItem { Text = "General Settings", Icon = "bi-gear-fill", Url = "/Admin/Settings" }
                        }
                    });
                }

                return sidebar;
            }
            catch
            {
                // Return a minimal sidebar with just Dashboard so the app remains navigable
                return new SidebarModel
                {
                    Sections = new List<SidebarSection>
                    {
                        new SidebarSection
                        {
                            Label = "Overview",
                            Items = new List<SidebarItem>
                            {
                                new SidebarItem { Text = "Dashboard", Icon = "bi-grid-1x2-fill", Url = "/Admin/Dashboard" }
                            }
                        }
                    }
                };
            }
        }

        private List<SidebarItem> BuildStudentChildren(List<Permission> permissions)
        {
            var items = new List<SidebarItem>();

            if (permissions.Any(p => p.Code == PermissionConstants.Students.View))
                items.Add(new SidebarItem { Text = "All Students", Url = "/Admin/Students" });

            if (permissions.Any(p => p.Code == PermissionConstants.Students.Create))
                items.Add(new SidebarItem { Text = "New Enrollment", Url = "/Admin/Students/Enroll" });

            return items.Any() ? items : new List<SidebarItem> { new SidebarItem { Text = "All Students", Url = "/Admin/Students" } };
        }

        private List<SidebarItem> BuildTeacherChildren(List<Permission> permissions)
        {
            var items = new List<SidebarItem>();

            if (permissions.Any(p => p.Code == PermissionConstants.Teachers.View))
                items.Add(new SidebarItem { Text = "All Teachers", Url = "/Admin/Teachers" });

            if (permissions.Any(p => p.Code == PermissionConstants.Teachers.Create))
                items.Add(new SidebarItem { Text = "Add Teacher", Url = "/Admin/Teachers/Create" });

            return items.Any() ? items : new List<SidebarItem> { new SidebarItem { Text = "All Teachers", Url = "/Admin/Teachers" } };
        }

        private async Task<List<Permission>> GetUserPermissionsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return new List<Permission>();

                var userRoleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var permissions = await _context.RolePermissions
                    .Where(rp => userRoleIds.Contains(rp.RoleId))
                    .Include(rp => rp.Permission)
                    .Select(rp => rp.Permission)
                    .Distinct()
                    .ToListAsync();

                return permissions;
            }
            catch
            {
                return new List<Permission>();
            }
        }
    }
}
