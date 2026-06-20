using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IClassService _classService;
        private readonly ISubClassService _subClassService;
        private readonly ISubjectService _subjectService;
        private readonly ISidebarService _sidebarService;
        private readonly IStudentService _studentService;
        private readonly IParentGuardianService _parentGuardianService;
        private readonly ITermRegistrationServices _termRegistrationServices;
        private readonly IPaymentCategoryService _paymentCategoryService;
        private readonly IPaymentItemService _paymentItemService;
        private readonly IPaymentSetupService _paymentSetupService;
        private readonly IStudentPaymentService _studentPaymentService;
        private readonly IPaymentReportService _paymentReportService;
        private readonly ITermGeneralInformationService _termGeneralInfoService;
        private readonly IClassTermInformationService _classTermInfoService;
        private readonly IEmployeeService _employeeService;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(
            ISessionService sessionService,
            IClassService classService,
            ISubClassService subClassService,
            ISubjectService subjectService,
            ISidebarService sidebarService,
            IStudentService studentService,
            IParentGuardianService parentGuardianService,
            ITermRegistrationServices termRegistrationServices,
            IPaymentCategoryService paymentCategoryService,
            IPaymentItemService paymentItemService,
            IPaymentSetupService paymentSetupService,
            IStudentPaymentService studentPaymentService,
            IPaymentReportService paymentReportService,
            ITermGeneralInformationService termGeneralInfoService,
            IClassTermInformationService classTermInfoService,
            IEmployeeService employeeService,
            ApplicationDbContext dbContext)
        {
            _sessionService = sessionService;
            _classService = classService;
            _subClassService = subClassService;
            _subjectService = subjectService;
            _sidebarService = sidebarService;
            _studentService = studentService;
            _parentGuardianService = parentGuardianService;
            _termRegistrationServices = termRegistrationServices;
            _paymentCategoryService = paymentCategoryService;
            _paymentItemService = paymentItemService;
            _paymentSetupService = paymentSetupService;
            _studentPaymentService = studentPaymentService;
            _paymentReportService = paymentReportService;
            _termGeneralInfoService = termGeneralInfoService;
            _classTermInfoService = classTermInfoService;
            _employeeService = employeeService;
            _dbContext = dbContext;
        }

        [HttpPost]
        [Authorize(Policy = "Session.View")]
        public async Task<IActionResult> GetSessions([FromBody] DataTablesRequest request)
        {
            var result = await _sessionService.GetSessions(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.View")]
        public async Task<IActionResult> GetClasses([FromBody] DataTablesRequest request)
        {
            var result = await _classService.GetClasses(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.View")]
        public async Task<IActionResult> GetSubClasses([FromBody] DataTablesRequest request)
        {
            var result = await _subClassService.GetSubClasses(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Subject.View")]
        public async Task<IActionResult> GetSubjects([FromBody] DataTablesRequest request)
        {
            var result = await _subjectService.GetSubjects(request);
            return Json(result);
        }


        [HttpPost]
        [Authorize(Policy = "Class.View")]
        public async Task<IActionResult> GetAllSubClasses()
        {
            var result = await _subClassService.GetSubClasses();
            return Json(result);
        }

        [HttpGet]
        [Authorize(Policy = "Class.View")]
        public async Task<IActionResult> GetAllClasses()
        {
            var request = new DataTablesRequest
            {
                Draw = 1,
                Start = 0,
                Length = 1000,
                Search = new SearchInfo { Value = "" }
            };
            var result = await _classService.GetClasses(request);
            return Json(result.Data);
        }

        [HttpPost]
        [Authorize(Policy = "Student.View")]
        public async Task<IActionResult> GetStudents([FromBody] DataTablesRequest request)
        {
            var result = await _studentService.GetStudents(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.View")]
        public async Task<IActionResult> GetParents([FromBody] DataTablesRequest request)
        {
            var result = await _parentGuardianService.GetParents(request);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployees([FromBody] DataTablesRequest request)
        {
            var result = await _employeeService.GetEmployees(request);
            return Json(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSidebar()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not found" });
            }

            var sidebar = await _sidebarService.GetSidebarAsync(userId);
            return Json(new { success = true, data = sidebar });
        }

        /// <summary>
        /// Server-side DataTables endpoint for term registrations.
        /// Accepts the standard DataTables parameters plus custom filter fields
        /// (termFilter, sessionFilter, classFilter, subclassFilter) sent as extra POST body properties.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Student.View")]
        public async Task<IActionResult> GetTermRegistrations([FromBody] TermRegDataTablesRequest request)
        {
            var searchTerm    = request.Search?.Value ?? "";
            var sortCol       = request.Order?.FirstOrDefault()?.Column ?? 0;
            var sortDir       = request.Order?.FirstOrDefault()?.Dir    ?? "asc";

            var (data, total, filtered) = await _termRegistrationServices.GetStudentTermRegistrationAsync(
                skip:          request.Start,
                pageSize:      request.Length,
                searchTerm:    searchTerm,
                sortColumn:    sortCol,
                sortDirection: sortDir,
                termFilter:    request.TermFilter,
                sessionFilter: request.SessionFilter,
                classFilter:   request.ClassFilter,
                subclassFilter: request.SubclassFilter);

            return Json(new
            {
                draw            = request.Draw,
                recordsTotal    = total,
                recordsFiltered = filtered,
                data
            });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPaymentItems([FromBody] PaymentItemDataTablesRequest request)
        {
            var searchTerm = request.Search?.Value ?? "";
            var sortCol = request.Order?.FirstOrDefault()?.Column ?? 0;
            var sortDir = request.Order?.FirstOrDefault()?.Dir ?? "asc";

            var (data, total, filtered) = await _paymentItemService.GetPaymentItemsAsync(
                skip: request.Start,
                pageSize: request.Length,
                searchTerm: searchTerm,
                sortColumn: sortCol,
                sortDirection: sortDir,
                categoryFilter: request.CategoryFilter);

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPaymentSetups([FromBody] PaymentSetupDataTablesRequest request)
        {
            var searchTerm = request.Search?.Value ?? "";
            var sortCol = request.Order?.FirstOrDefault()?.Column ?? 0;
            var sortDir = request.Order?.FirstOrDefault()?.Dir ?? "asc";

            var (data, total, filtered) = await _paymentSetupService.GetPaymentSetupsAsync(
                skip: request.Start,
                pageSize: request.Length,
                searchTerm: searchTerm,
                sortColumn: sortCol,
                sortDirection: sortDir,
                sessionFilter: request.SessionFilter,
                termFilter: request.TermFilter,
                classFilter: request.ClassFilter);

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPayments([FromBody] PaymentDataTablesRequest request)
        {
            var searchTerm = request.Search?.Value ?? "";
            var sortCol = request.Order?.FirstOrDefault()?.Column ?? 0;
            var sortDir = request.Order?.FirstOrDefault()?.Dir ?? "asc";

            var (data, total, filtered) = await _studentPaymentService.GetPaymentsDataTableAsync(
                skip: request.Start,
                pageSize: request.Length,
                searchTerm: searchTerm,
                sortColumn: sortCol,
                sortDirection: sortDir,
                sessionFilter: request.SessionFilter,
                termFilter: request.TermFilter,
                classFilter: request.ClassFilter,
                statusFilter: request.StatusFilter,
                stateFilter: request.StateFilter);

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            });
        }

        [HttpGet]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPaymentCategoriesForDropdown()
        {
            var categories = await _paymentCategoryService.GetActiveCategoriesAsync();
            return Json(categories);
        }

        [HttpGet]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPaymentItemsForDropdown(int? categoryId)
        {
            var items = await _paymentItemService.GetActiveItemsAsync(categoryId);
            return Json(items);
        }

        [HttpPost]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPaymentCategories([FromBody] DataTablesRequest request)
        {
            var searchTerm = request.Search?.Value ?? "";
            var sortCol = request.Order?.FirstOrDefault()?.Column ?? 0;
            var sortDir = request.Order?.FirstOrDefault()?.Dir ?? "asc";

            var (data, total, filtered) = await _paymentCategoryService.GetPaymentCategoriesAsync(
                skip: request.Start,
                pageSize: request.Length,
                searchTerm: searchTerm,
                sortColumn: sortCol,
                sortDirection: sortDir);

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            });
        }
        [HttpPost]
        [Authorize(Policy = "Settings.View")]
        public async Task<IActionResult> GetTermGeneralInformations([FromBody] DataTablesRequest request)
        {
            var result = await _termGeneralInfoService.GetTermGeneralInformations(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Settings.View")]
        public async Task<IActionResult> GetClassTermInformations([FromBody] DataTablesRequest request)
        {
            var result = await _classTermInfoService.GetClassTermInformations(request);
            return Json(result);
        }
        // ════════════════════════════════════════════════════════════
        // STUDENT-FACING ENDPOINTS (for student dashboard/profile/payment pages)
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyPayments([FromBody] DataTablesRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var student = await _studentService.GetStudentByUserId(userId);
            if (student == null)
                return Json(new DataTablesResponse<object> { Draw = request.Draw, RecordsTotal = 0, RecordsFiltered = 0, Data = new List<object>() });

            var result = await _studentPaymentService.GetStudentPaymentsPagedAsync(student.Id, request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyTermRegistrations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var student = await _studentService.GetStudentByUserId(userId);
            if (student == null)
                return Json(new { success = false, data = new List<object>() });

            var regs = await _termRegistrationServices.GetStudentTermRegistrationsAsync(student.Id);
            return Json(new { success = true, data = regs });
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var student = await _studentService.GetStudentByUserId(userId);
            if (student == null)
                return Json(new { success = false, message = "Student record not found" });

            var parents = await _parentGuardianService.GetParentsByStudentAsync(student.Id);
            var regs = await _termRegistrationServices.GetStudentTermRegistrationsAsync(student.Id);

            return Json(new
            {
                success = true,
                data = new
                {
                    student = new
                    {
                        id = student.Id,
                        admissionNumber = student.AdmissionNumber,
                        firstName = student.FirstName,
                        surname = student.Surname,
                        otherName = student.OtherName,
                        fullName = student.FullName,
                        gender = student.Gender.ToString(),
                        dateOfBirth = student.DateOfBirth.ToString("dd-MMM-yyyy"),
                        nationality = student.Nationality,
                        state = student.State,
                        localGov = student.LocalGov,
                        address = student.Address,
                        picturePath = student.PicturePath,
                        isActive = student.ApplicationUser?.IsActive ?? false,
                        email = student.ApplicationUser?.Email
                    },
                    parents = parents.Select(p => new
                    {
                        fullName = p.FullName,
                        relationship = p.Relationship,
                        occupation = p.Occupation,
                        phone1 = p.Phone1,
                        phone2 = p.Phone2,
                        address = p.Address
                    }),
                    currentRegistration = regs.OrderByDescending(r => r.Session).ThenByDescending(r => r.Term).FirstOrDefault()
                }
            });
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var student = await _studentService.GetStudentByUserId(userId);
            if (student == null)
                return Json(new { success = false, message = "Student record not found" });

            var regs = await _termRegistrationServices.GetStudentTermRegistrationsAsync(student.Id);

            return Json(new
            {
                success = true,
                data = new
                {
                    studentName = student.FullName,
                    studentPicture = student.PicturePath,
                    currentRegistration = regs.OrderByDescending(r => r.Session).ThenByDescending(r => r.Term).FirstOrDefault(),
                    totalRegistrations = regs.Count,
                    allRegistrations = regs
                }
            });
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyPerformanceData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var student = await _studentService.GetStudentByUserId(userId);
            if (student == null)
                return Json(new { success = false, message = "Student record not found" });

            var termRegIds = await _dbContext.TermRegistrations
                .Where(tr => tr.StudentId == student.Id)
                .Select(tr => tr.Id)
                .ToListAsync();

            // Query all result rows for this student with status=true (recorded)
            var results = await _dbContext.ResultTables
                .Include(rt => rt.TermRegistration).ThenInclude(tr => tr.SesseionTable)
                .Where(rt => termRegIds.Contains(rt.TermRegId) && rt.Status)
                .Select(rt => new
                {
                    rt.TermRegId,
                    SessionName = rt.TermRegistration.SesseionTable.Name,
                    Term = rt.TermRegistration.Term,
                    ScoreOne = rt.ScoreOne ?? 0,
                    ScoreTwo = rt.ScoreTwo ?? 0,
                    ScoreThree = rt.ScoreThree ?? 0,
                    ScoreFour = rt.ScoreFour ?? 0,
                    ScoreFive = rt.ScoreFive ?? 0,
                    ScoreSix = rt.ScoreSix ?? 0
                })
                .ToListAsync();

            // Aggregate: per term registration, compute total score
            var termScores = results
                .GroupBy(r => new { r.TermRegId, r.SessionName, r.Term })
                .Select(g => new
                {
                    g.Key.SessionName,
                    g.Key.Term,
                    TotalScore = g.Sum(r => r.ScoreOne + r.ScoreTwo + r.ScoreThree + r.ScoreFour + r.ScoreFive + r.ScoreSix)
                })
                .OrderByDescending(x => x.SessionName)
                .ThenBy(x => x.Term)
                .ToList();

            // Cumulative by session
            var cumulative = termScores
                .GroupBy(x => x.SessionName)
                .Select(g => new
                {
                    session = g.Key,
                    totalScore = g.Sum(x => x.TotalScore)
                })
                .OrderBy(x => x.session)
                .ToList();

            // Term breakdown by session
            var termBreakdown = termScores
                .GroupBy(x => x.SessionName)
                .Select(g => new
                {
                    session = g.Key,
                    firstTerm = g.Where(x => x.Term == ConstantEnums.Term.First).Sum(x => x.TotalScore),
                    secondTerm = g.Where(x => x.Term == ConstantEnums.Term.Second).Sum(x => x.TotalScore),
                    thirdTerm = g.Where(x => x.Term == ConstantEnums.Term.Third).Sum(x => x.TotalScore)
                })
                .OrderBy(x => x.session)
                .ToList();

            return Json(new { success = true, data = new { cumulative, termBreakdown } });
        }
    }

    /// <summary>
    /// Extends DataTablesRequest with the term-registration filter dropdowns.
    /// </summary>
    public class TermRegDataTablesRequest : DataTablesRequest
    {
        public int? TermFilter      { get; set; }
        public int? SessionFilter   { get; set; }
        public int? ClassFilter     { get; set; }
        public int? SubclassFilter  { get; set; }
    }

    public class PaymentItemDataTablesRequest : DataTablesRequest
    {
        public int? CategoryFilter { get; set; }
    }

    public class PaymentSetupDataTablesRequest : DataTablesRequest
    {
        public int? SessionFilter { get; set; }
        public int? TermFilter { get; set; }
        public int? ClassFilter { get; set; }
    }

    public class PaymentDataTablesRequest : DataTablesRequest
    {
        public int? SessionFilter { get; set; }
        public int? TermFilter { get; set; }
        public int? ClassFilter { get; set; }
        public string? StatusFilter { get; set; }
        public int? StateFilter { get; set; }
    }
}
