using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementDataAccess.Seeders;
using UpgradedSchoolManagementDataAccess.Services;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementWeb.Authorization;
using UpgradedSchoolManagementWeb.Services;
using Microsoft.Extensions.Options;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementDataAccess.Services;

var builder = WebApplication.CreateBuilder(args);

// Register encoding provider for ExcelDataReader
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Uniqueness is enforced by AdmissionNumberEmailValidator; format check is bypassed
    // so that admission numbers (EDU/STD/YYYY/NNN) are accepted as the email value.
    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/";

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddUserValidator<UpgradedSchoolManagementWeb.Authorization.AdmissionNumberEmailValidator>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicies();
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISubClassService, SubClassService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IParentGuardianService, ParentGuardianService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<ISidebarService, SidebarService>();
builder.Services.AddScoped<IViewSelectionService, ViewSelectionService>();
builder.Services.AddScoped<ITermRegistrationServices, TermRegistrationServices>();

builder.Services.AddScoped<IPaymentCategoryService, PaymentCategoryService>();
builder.Services.AddScoped<IPaymentItemService, PaymentItemService>();
builder.Services.AddScoped<IPaymentSetupService, PaymentSetupService>();
builder.Services.AddScoped<IStudentPaymentService, StudentPaymentService>();
builder.Services.AddScoped<IPaymentReportService, PaymentReportService>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.AddScoped<IClassTermInformationService, ClassTermInformationService>();
builder.Services.AddScoped<ITermGeneralInformationService, TermGeneralInformationService>();
builder.Services.AddScoped<IResultManagerService, ResultManagerService>();
builder.Services.AddScoped<IResultSkillService, ResultSkillService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<AnnualReportService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<SchoolConfigurationSetup>(
    builder.Configuration.GetSection(SchoolConfigurationSetup.SectionName));

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    await context.Database.EnsureCreatedAsync();

    await PermissionSeeder.SeedAsync(context);
    await RoleSeeder.SeedAsync(context);
    await RolePermissionSeeder.SeedAsync(context);
    await AdminUserSeeder.SeedAsync(userManager, roleManager, context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
