using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Controllers
{
    [Authorize]
    public class V1Controller : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly IClassService _classService;
        private readonly ISubClassService _subClassService;
        private readonly ISubjectService _subjectService;
        private readonly IStudentService _studentService;
        private readonly IParentGuardianService _parentGuardianService;
        private readonly ITermRegistrationServices _termRegistrationServices;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly IPaymentCategoryService _paymentCategoryService;
        private readonly IPaymentItemService _paymentItemService;
        private readonly IPaymentSetupService _paymentSetupService;
        private readonly IStudentPaymentService _studentPaymentService;
        private readonly IPaymentReportService _paymentReportService;
        private readonly ITermGeneralInformationService _termGeneralInfoService;
        private readonly IClassTermInformationService _classTermInfoService;
        private readonly IRoleService _roleService;
        private readonly IUserManagementService _userManagementService;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmployeeService _employeeService;

    public V1Controller(
        IUnitOfWork unitOfWork,
        ISessionService sessionService,
        IClassService classService,
        ISubClassService subClassService,
        ISubjectService subjectService,
        IStudentService studentService,
        IParentGuardianService parentGuardianService,
        ITermRegistrationServices termRegistrationServices,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment env,
        IPaymentCategoryService paymentCategoryService,
        IPaymentItemService paymentItemService,
        IPaymentSetupService paymentSetupService,
        IStudentPaymentService studentPaymentService,
        IPaymentReportService paymentReportService,
        ITermGeneralInformationService termGeneralInfoService,
        IClassTermInformationService classTermInfoService,
        IRoleService roleService,
        IUserManagementService userManagementService,
        IAuditLogService auditLogService,
        IEmployeeService employeeService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _classService = classService;
            _subClassService = subClassService;
            _subjectService = subjectService;
            _studentService = studentService;
            _parentGuardianService = parentGuardianService;
            _termRegistrationServices = termRegistrationServices;
            _signInManager = signInManager;
            _env = env;
            _paymentCategoryService = paymentCategoryService;
            _paymentItemService = paymentItemService;
            _paymentSetupService = paymentSetupService;
            _studentPaymentService = studentPaymentService;
            _paymentReportService = paymentReportService;
            _termGeneralInfoService = termGeneralInfoService;
            _classTermInfoService = classTermInfoService;
            _roleService = roleService;
            _userManagementService = userManagementService;
            _auditLogService = auditLogService;
            _employeeService = employeeService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }

        [HttpPost]
        [Authorize(Policy = "Session.Create")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Session name is required" });
            }
            var result = await _sessionService.CreateSession(request.Name);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Session.Edit")]
        public async Task<IActionResult> UpdateSession([FromBody] UpdateSessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Session name is required" });
            }
            var result = await _sessionService.UpdateSession(request.Id, request.Name);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Session.Edit")]
        public async Task<IActionResult> DeleteSession([FromBody] IdRequest request)
        {
            var result = await _sessionService.DeleteSession(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Session.Edit")]
        public async Task<IActionResult> ToggleSessionStatus([FromBody] IdRequest request)
        {
            var result = await _sessionService.ToggleSessionStatus(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Create")]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Class name is required" });
            }
            var result = await _classService.CreateClass(request.Name, request.DisplayOrder, request.ResultType);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Edit")]
        public async Task<IActionResult> UpdateClass([FromBody] UpdateClassRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Class name is required" });
            }
            var result = await _classService.UpdateClass(request.Id, request.Name, request.DisplayOrder, request.ResultType);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Edit")]
        public async Task<IActionResult> DeleteClass([FromBody] IdRequest request)
        {
            var result = await _classService.DeleteClass(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Edit")]
        public async Task<IActionResult> ToggleClassStatus([FromBody] IdRequest request)
        {
            var result = await _classService.ToggleClassStatus(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Settings.View")]
        public async Task<IActionResult> GetTermGeneralInformationById([FromBody] IdRequest request)
        {
            var dto = await _termGeneralInfoService.GetById(request.Id);
            if (dto == null)
            {
                return Json(new { success = false, message = "Record not found." });
            }
            return Json(new { success = true, data = dto });
        }

        [HttpPost]
        [Authorize(Policy = "Settings.View")]
        public async Task<IActionResult> GetClassTermInformationById([FromBody] IdRequest request)
        {
            var dto = await _classTermInfoService.GetById(request.Id);
            if (dto == null)
            {
                return Json(new { success = false, message = "Record not found." });
            }
            return Json(new { success = true, data = dto });
        }

        [HttpPost]
        [Authorize(Policy = "Settings.Manage")]
        public async Task<IActionResult> CreateTermGeneralInformation([FromBody] TermGeneralInformationRequest request)
        {
            if (request.NextTermStart == default)
            {
                return Json(new { success = false, message = "Next term start date is required" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            request.ApplicationUserId = userId;
            var result = await _termGeneralInfoService.Create(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Settings.Manage")]
        public async Task<IActionResult> UpdateTermGeneralInformation([FromBody] TermGeneralInformationRequest request)
        {
            if (request.NextTermStart == default)
            {
                return Json(new { success = false, message = "Next term start date is required" });
            }
            var result = await _termGeneralInfoService.Update(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Settings.Manage")]
        public async Task<IActionResult> DeleteTermGeneralInformation([FromBody] IdRequest request)
        {
            var result = await _termGeneralInfoService.Delete(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Settings.Manage")]
        public async Task<IActionResult> CreateClassTermInformation([FromBody] ClassTermInformationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClassTeacherName))
            {
                return Json(new { success = false, message = "Class teacher name is required" });
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                request.ApplicationUserId = userId;
            }
            var result = await _classTermInfoService.Create(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Settings.Manage")]
        public async Task<IActionResult> UpdateClassTermInformation([FromBody] ClassTermInformationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClassTeacherName))
            {
                return Json(new { success = false, message = "Class teacher name is required" });
            }
            var result = await _classTermInfoService.Update(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Settings.Manage")]
        public async Task<IActionResult> DeleteClassTermInformation([FromBody] IdRequest request)
        {
            var result = await _classTermInfoService.Delete(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Create")]
        public async Task<IActionResult> CreateSubClass([FromBody] CreateSubClassRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Sub-class name is required" });
            }
            var result = await _subClassService.CreateSubClass(request.Name);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Edit")]
        public async Task<IActionResult> UpdateSubClass([FromBody] UpdateSubClassRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Sub-class name is required" });
            }
            var result = await _subClassService.UpdateSubClass(request.Id, request.Name);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Edit")]
        public async Task<IActionResult> DeleteSubClass([FromBody] IdRequest request)
        {
            var result = await _subClassService.DeleteSubClass(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Class.Edit")]
        public async Task<IActionResult> ToggleSubClassStatus([FromBody] IdRequest request)
        {
            var result = await _subClassService.ToggleSubClassStatus(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Subject.Create")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Subject name is required" });
            }
            var result = await _subjectService.CreateSubject(request.Name, request.Code);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Subject.Edit")]
        public async Task<IActionResult> UpdateSubject([FromBody] UpdateSubjectRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Subject name is required" });
            }
            var result = await _subjectService.UpdateSubject(request.Id, request.Name, request.Code);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Subject.Edit")]
        public async Task<IActionResult> DeleteSubject([FromBody] IdRequest request)
        {
            var result = await _subjectService.DeleteSubject(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Subject.Edit")]
        public async Task<IActionResult> ToggleSubjectStatus([FromBody] IdRequest request)
        {
            var result = await _subjectService.ToggleSubjectStatus(request.Id);
            return Json(result);
        }


        // ── STUDENTS ────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Policy = "Student.Create")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.Surname))
                return Json(new { success = false, message = "First name and surname are required" });
            if (string.IsNullOrWhiteSpace(request.Password))
                return Json(new { success = false, message = "Password is required" });

            var input = new CreateStudentInput
            {
                FirstName   = request.FirstName,
                Surname     = request.Surname,
                OtherName   = request.OtherName,
                Gender      = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Nationality = request.Nationality,
                State       = request.State,
                LocalGov    = request.LocalGov,
                Address     = request.Address,
                PicturePath = request.PicturePath,
                Password    = request.Password
            };
            var result = await _studentService.CreateStudent(input);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.Surname))
                return Json(new { success = false, message = "First name and surname are required" });

            var input = new UpdateStudentInput
            {
                Id          = request.Id,
                FirstName   = request.FirstName,
                Surname     = request.Surname,
                OtherName   = request.OtherName,
                Gender      = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Nationality = request.Nationality,
                State       = request.State,
                LocalGov    = request.LocalGov,
                Address     = request.Address,
                PicturePath = request.PicturePath
            };
            var result = await _studentService.UpdateStudent(input);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Delete")]
        public async Task<IActionResult> DeleteStudent([FromBody] IdRequest request)
        {
            var result = await _studentService.DeleteStudent(request.Id, _env.WebRootPath);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Json(new { success = false, message = "Full name is required" });
            if (string.IsNullOrWhiteSpace(request.Password))
                return Json(new { success = false, message = "Password is required" });

            var input = new CreateEmployeeInput
            {
                FullName = request.FullName,
                Gender = request.Gender,
                EmployeeType = request.EmployeeType,
                Address = request.Address,
                Password = request.Password
            };
            var result = await _employeeService.CreateEmployee(input);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Json(new { success = false, message = "Full name is required" });

            var input = new UpdateEmployeeInput
            {
                Id = request.Id,
                FullName = request.FullName,
                Gender = request.Gender,
                EmployeeType = request.EmployeeType,
                Address = request.Address
            };
            var result = await _employeeService.UpdateEmployee(input);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmployee([FromBody] IdRequest request)
        {
            var result = await _employeeService.DeleteEmployee(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> ResetStudentPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return Json(new { success = false, message = "New password is required" });
            if (request.NewPassword.Length < 6)
                return Json(new { success = false, message = "Password must be at least 6 characters" });

            var result = await _studentService.ResetStudentPassword(request.StudentId, request.NewPassword);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> ToggleStudentStatus([FromBody] IdRequest request)
        {
            var result = await _studentService.ToggleStudentStatus(request.Id);
            return Json(result);
        }

        // ── TERM REGISTRATIONS ─────────────────────────────────────────

        [HttpPost]
        [Authorize(Policy = "Student.Delete")]
        public async Task<IActionResult> DeleteTermRegistration([FromBody] IdRequest request)
        {
            var result = await _termRegistrationServices.DeleteStudentTermRegistrationAsync(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Delete")]
        public async Task<IActionResult> DeleteTermRegistrations([FromBody] IdListRequest request)
        {
            if (request.Ids == null || !request.Ids.Any())
            {
                return Json(new { success = false, message = "No registrations selected for deletion." });
            }
            var result = await _termRegistrationServices.DeleteStudentsTermRegistrationAsync(request.Ids);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> BatchExcelRegistration([FromForm] BatchRegistrationRequest request)
        {
            if (request.ExcelFile == null || request.ExcelFile.Length == 0)
                return Json(new { success = false, message = "Please upload a valid Excel file." });

            if (request.SessionId <= 0 || request.SchoolClassId <= 0 || request.SubClassId <= 0)
                return Json(new { success = false, message = "Session, Class, and Sub Class are required." });

            var subjectIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.Subjects))
            {
                subjectIds = request.Subjects.Split(',').Select(int.Parse).ToList();
            }

            var regNumbers = new List<string>();

            // Ensure CodePagesEncodingProvider is registered in Program.cs as well
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            try
            {
                using var stream = request.ExcelFile.OpenReadStream();
                using var reader = ExcelReaderFactory.CreateReader(stream);

                do
                {
                    while (reader.Read())
                    {
                        var val = reader.GetValue(0); // Assuming reg number is in first column
                        if (val != null)
                        {
                            var regNum = val.ToString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(regNum))
                            {
                                regNumbers.Add(regNum);
                            }
                        }
                    }
                } while (reader.NextResult());
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to parse Excel file: " + ex.Message });
            }

            var result = await _termRegistrationServices.BatchRegisterStudentsAsync(
                regNumbers, 
                request.SchoolClassId, 
                request.SessionId, 
                request.SubClassId, 
                (Term)request.Term, 
                subjectIds);

            return Json(new 
            { 
                success = result.Success, 
                message = result.Message,
                totalProcessed = result.TotalProcessed,
                successCount = result.SuccessCount,
                failureCount = result.FailureCount,
                errors = result.Errors
            });
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> SearchPromotionStudents([FromBody] SearchPromotionStudentsRequest request)
        {
            var students = await _termRegistrationServices.GetStudentsForBatchPromotionAsync(
                request.Term, request.SessionId, request.SchoolClassId, request.SubClassId);
            return Json(new { success = true, data = students });
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> BatchPromote([FromBody] BatchPromoteRequest request)
        {
            if (request.TermRegIds == null || !request.TermRegIds.Any())
                return Json(new { success = false, message = "No students selected for promotion." });

            if (request.SessionId <= 0 || request.SchoolClassId <= 0 || request.SubClassId <= 0)
                return Json(new { success = false, message = "Target Session, Class, and Sub Class are required." });

            var result = await _termRegistrationServices.BatchPromoteStudentsAsync(
                request.TermRegIds, 
                request.SchoolClassId, 
                request.SessionId, 
                request.SubClassId, 
                (Term)request.Term);

            return Json(new 
            { 
                success = result.Success, 
                message = result.Message,
                totalProcessed = result.TotalProcessed,
                successCount = result.SuccessCount,
                failureCount = result.FailureCount,
                errors = result.Errors
            });
        }

        [HttpPost]
        [Authorize(Policy = "Result.Upload")]
        public async Task<IActionResult> ImportAssessmentExcel([FromForm] ImportAssessmentRequest request)
        {
            if (request.ExcelFile == null || request.ExcelFile.Length == 0)
                return Json(new { success = false, message = "Please upload a valid Excel file." });

            if (request.SessionId <= 0 || request.Term <= 0 || request.SchoolClassId <= 0 || request.SubClassId <= 0)
                return Json(new { success = false, message = "Session, Term, Class, and Sub Class are required." });

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = request.ExcelFile.OpenReadStream();
            var result = await _unitOfWork.ResultManagerServices.ImportAssessmentScoresAsync(
                request.SessionId,
                (Term)request.Term,
                request.SchoolClassId,
                request.SubClassId,
                stream);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> RemoveRegisteredSubject([FromBody] RemoveRegisteredSubjectRequest request)
        {
            if (request.ResultTableId <= 0)
                return Json(new { success = false, message = "Invalid subject record ID." });

            var result = await _termRegistrationServices.RemoveSubjectFromResultTableAsync(request.ResultTableId);

            return Json(new { success = result.Success, message = result.Message });
        }

        // ── PARENT / GUARDIAN ─────────────────────────────────────────

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> CreateParent([FromBody] ParentGuardianCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return Json(new { success = false, message = "Full name is required" });
            if (string.IsNullOrWhiteSpace(dto.Relationship))
                return Json(new { success = false, message = "Relationship is required" });
            if (string.IsNullOrWhiteSpace(dto.Phone1))
                return Json(new { success = false, message = "Phone number is required" });

            var result = await _parentGuardianService.CreateParentAsync(dto);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> UpdateParent([FromBody] UpdateParentRequest request)
        {
            if (request.Id <= 0)
                return Json(new { success = false, message = "Invalid parent ID" });
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Json(new { success = false, message = "Full name is required" });
            if (string.IsNullOrWhiteSpace(request.Relationship))
                return Json(new { success = false, message = "Relationship is required" });

            var dto = new ParentGuardianCreateDto
            {
                FullName = request.FullName,
                Relationship = request.Relationship,
                Occupation = request.Occupation,
                Address = request.Address,
                Phone1 = request.Phone1,
                Phone2 = request.Phone2
            };

            var result = await _parentGuardianService.UpdateParentAsync(request.Id, dto);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> DeleteParent([FromBody] IdRequest request)
        {
            var result = await _parentGuardianService.DeleteParentAsync(request.Id);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.View")]
        public async Task<IActionResult> GetParent([FromBody] IdRequest request)
        {
            var parent = await _parentGuardianService.GetParentByIdAsync(request.Id);
            if (parent == null)
                return Json(new { success = false, message = "Parent not found" });
            return Json(new { success = true, data = parent });
        }

        // ── PARENT add-or-link (student-aware) ─────────────────────────────

        [HttpPost]
        [Authorize(Policy = "Student.View")]
        public async Task<IActionResult> SearchStudent([FromBody] SearchStudentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return Json(new { success = false, message = "Please enter an admission number or email." });

            var student = await _studentService.FindStudentByAdmissionNumberAsync(request.Query);
            if (student == null)
                return Json(new { success = false, message = "No student found with that admission number or email." });

            return Json(new { success = true, data = student });
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> AddOrLinkParent([FromBody] AddOrLinkParentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Json(new { success = false, message = "Full name is required" });
            if (string.IsNullOrWhiteSpace(request.Relationship))
                return Json(new { success = false, message = "Relationship is required" });
            if (string.IsNullOrWhiteSpace(request.Phone1))
                return Json(new { success = false, message = "Phone number is required" });
            if (request.StudentId <= 0)
                return Json(new { success = false, message = "Please search and select a student first." });

            var dto = new ParentGuardianCreateDto
            {
                FullName = request.FullName,
                Relationship = request.Relationship,
                Occupation = request.Occupation,
                Address = request.Address,
                Phone1 = request.Phone1,
                Phone2 = request.Phone2
            };

            var result = await _parentGuardianService.AddOrLinkParentAsync(dto, request.StudentId);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> LinkParentToStudent([FromBody] LinkParentToStudentRequest request)
        {
            if (request.ParentGuardianId <= 0)
                return Json(new { success = false, message = "Invalid parent ID" });
            if (request.StudentId <= 0)
                return Json(new { success = false, message = "Invalid student ID" });

            var result = await _parentGuardianService.LinkExistingParentToStudentAsync(
                request.ParentGuardianId, request.StudentId, request.IsPrimaryContact);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Student.Edit")]
        public async Task<IActionResult> UploadStudentPhoto(IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return Json(new { success = false, message = "No file received" });

                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                    return Json(new { success = false, message = "Only PNG, JPG and JPEG files are accepted" });

                var relativePath = await ImageCompressor.CompressAndSaveImageAsync(photo, _env.WebRootPath, "uploads/students");
                return Json(new { success = true, path = relativePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════════════
        // PAYMENT CATEGORIES
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> CreatePaymentCategory([FromBody] PaymentCategoryViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Category name is required" });
            var result = await _paymentCategoryService.CreatePaymentCategoryAsync(model);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> UpdatePaymentCategory([FromBody] PaymentCategoryViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Category name is required" });
            var result = await _paymentCategoryService.UpdatePaymentCategoryAsync(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> DeletePaymentCategory([FromBody] IdRequest request)
        {
            var result = await _paymentCategoryService.DeletePaymentCategoryAsync(request.Id);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> TogglePaymentCategory([FromBody] IdRequest request)
        {
            var result = await _paymentCategoryService.TogglePaymentCategoryAsync(request.Id);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ════════════════════════════════════════════════════════════
        // PAYMENT ITEMS
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> CreatePaymentItem([FromBody] PaymentItemViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Item name is required" });
            var result = await _paymentItemService.CreatePaymentItemAsync(model);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> UpdatePaymentItem([FromBody] PaymentItemViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Item name is required" });
            var result = await _paymentItemService.UpdatePaymentItemAsync(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> DeletePaymentItem([FromBody] IdRequest request)
        {
            var result = await _paymentItemService.DeletePaymentItemAsync(request.Id);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> TogglePaymentItem([FromBody] IdRequest request)
        {
            var result = await _paymentItemService.TogglePaymentItemAsync(request.Id);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ════════════════════════════════════════════════════════════
        // PAYMENT SETUPS
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> CreatePaymentSetup([FromBody] PaymentSetupViewModel model)
        {
            var result = await _paymentSetupService.CreatePaymentSetupAsync(model);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> CreateBatchPaymentSetup([FromBody] PaymentSetupViewModel model)
        {
            var result = await _paymentSetupService.CreateBatchPaymentSetupAsync(model);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> UpdatePaymentSetup([FromBody] PaymentSetupViewModel model)
        {
            var result = await _paymentSetupService.UpdatePaymentSetupAsync(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> DeletePaymentSetup([FromBody] IdRequest request)
        {
            var result = await _paymentSetupService.DeletePaymentSetupAsync(request.Id);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceCreate")]
        public async Task<IActionResult> TogglePaymentSetup([FromBody] IdRequest request)
        {
            var result = await _paymentSetupService.TogglePaymentSetupAsync(request.Id);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ════════════════════════════════════════════════════════════
        // STUDENT PAYMENTS
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> LookupStudentPayment([FromBody] PaymentLookupViewModel model)
        {
            var result = await _studentPaymentService.LookupPayableItemsAsync(
                model.AdmissionNo, model.ClassId, model.CategoryId);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.PaymentRecord")]
        public async Task<IActionResult> CreateStudentPayment([FromBody] CreatePaymentViewModel model)
        {
            var result = await _studentPaymentService.CreatePaymentAsync(model);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpGet]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPaymentReceipt(int paymentId)
        {
            var receipt = await _studentPaymentService.GetReceiptAsync(paymentId);
            if (receipt == null)
                return Json(new { success = false, message = "Payment not found" });
            return Json(new { success = true, data = receipt });
        }

        [HttpGet]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPaymentDetail(int paymentId)
        {
            var detail = await _studentPaymentService.GetPaymentDetailAsync(paymentId);
            if (detail == null)
                return Json(new { success = false, message = "Payment not found" });
            return Json(new { success = true, data = detail });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.PaymentApprove")]
        public async Task<IActionResult> UpdatePaymentState([FromBody] UpdatePaymentStateRequest request)
        {
            var state = (PaymentState)request.PaymentState;
            var result = await _studentPaymentService.UpdatePaymentStateAsync(request.PaymentId, state, request.RejectMessage);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> GetPendingPaymentNotifications()
        {
            var notifications = await _studentPaymentService.GetPendingPaymentNotificationsAsync();
            return Json(notifications);
        }

        // ── NEW: Single-item payment flow ─────────────────────────────

        [HttpPost]
        [Authorize(Policy = "Finance.View")]
        public async Task<IActionResult> LookupByItem([FromBody] PaymentItemLookupRequest request)
        {
            var result = await _studentPaymentService.LookupByItemAsync(
                request.SessionId, request.Term, request.PaymentItemId, request.AdmissionNo);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.PaymentRecord")]
        public async Task<IActionResult> CreateSingleItemPayment([FromBody] CreateSingleItemPaymentVM request)
        {
            var username = User.Identity?.Name;
            var result = await _studentPaymentService.CreateSingleItemPaymentAsync(request, username);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        [Authorize(Policy = "Finance.InvoiceEdit")]
        public async Task<IActionResult> UpdatePaymentAmount([FromBody] UpdatePaymentAmountVM request)
        {
            var username = User.Identity?.Name;
            var result = await _studentPaymentService.UpdatePaymentAmountAsync(request, username);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ════════════════════════════════════════════════════════════
        // REPORTS
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Policy = "Report.View")]
        public async Task<IActionResult> GetClassReport([FromBody] ClassReportRequest request)
        {
            var result = await _paymentReportService.GetClassReportAsync(
                sessionId: request.SessionId,
                term: request.Term,
                classId: request.ClassId,
                subClassId: request.SubClassId,
                categoryId: request.CategoryId,
                paymentItemId: request.PaymentItemId,
                skip: request.Start,
                pageSize: request.Length,
                searchTerm: request.Search?.Value ?? "",
                sortColumn: request.Order?.FirstOrDefault()?.Column ?? 0,
                sortDirection: request.Order?.FirstOrDefault()?.Dir ?? "asc");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = result.RecordsTotal,
                recordsFiltered = result.RecordsFiltered,
                data = result.Rows,
                summary = result.Summary
            });
        }

        [HttpPost]
        [Authorize(Policy = "Report.View")]
        public async Task<IActionResult> GetSchoolReport([FromBody] SchoolReportRequest request)
        {
            var result = await _paymentReportService.GetSchoolReportAsync(
                sessionId: request.SessionId,
                term: request.Term,
                skip: request.Start,
                pageSize: request.Length,
                searchTerm: request.Search?.Value ?? "",
                sortColumn: request.Order?.FirstOrDefault()?.Column ?? 0,
                sortDirection: request.Order?.FirstOrDefault()?.Dir ?? "asc");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = result.RecordsTotal,
                recordsFiltered = result.RecordsFiltered,
                data = result.Rows,
                summary = result.Summary
            });
        }

        [HttpPost]
        [Authorize(Policy = "Report.View")]
        public async Task<IActionResult> GetCategoryItemReport([FromBody] CategoryItemReportRequest request)
        {
            var result = await _paymentReportService.GetCategoryItemReportAsync(
                sessionId: request.SessionId,
                term: request.Term,
                categoryId: request.CategoryId,
                classId: request.ClassId,
                skip: request.Start,
                pageSize: request.Length,
                searchTerm: request.Search?.Value ?? "",
                sortColumn: request.Order?.FirstOrDefault()?.Column ?? 0,
                sortDirection: request.Order?.FirstOrDefault()?.Dir ?? "asc");
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = result.RecordsTotal,
                recordsFiltered = result.RecordsFiltered,
                data = result.Rows,
                summary = result.Summary
            });
        }

        // ════════════════════════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize(Policy = "Dashboard.View")]
        public async Task<IActionResult> GetDashboardCategorySummary(int sessionId, int term)
        {
            var result = await _paymentReportService.GetDashboardCategorySummaryAsync(sessionId, term);
            return Json(result);
        }

        [HttpGet]
        [Authorize(Policy = "Dashboard.View")]
        public async Task<IActionResult> GetDashboardItemSummary(int sessionId, int term)
        {
            var result = await _paymentReportService.GetDashboardItemSummaryAsync(sessionId, term);
            return Json(result);
        }

        [HttpGet]
        [Authorize(Policy = "Dashboard.View")]
        public async Task<IActionResult> GetDashboardCategoryTrend(int recentSessionCount = 4)
        {
            var result = await _paymentReportService.GetDashboardCategoryTrendAsync(recentSessionCount);
            return Json(result);
        }

        [HttpGet]
        [Authorize(Policy = "Dashboard.View")]
        public async Task<IActionResult> GetDashboardItemChart(int sessionId, int term)
        {
            var result = await _paymentReportService.GetDashboardItemChartAsync(sessionId, term);
            return Json(result);
        }

        [HttpGet]
        [Authorize(Policy = "Dashboard.View")]
        public async Task<IActionResult> GetDashboardTermRegistrationChart(int sessionId)
        {
            var result = await _paymentReportService.GetDashboardTermRegistrationChartAsync(sessionId);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Dashboard.View")]
        public async Task<IActionResult> GetDashboardData([FromBody] DashboardDataRequest request)
        {
            try
            {
                var sessionId = request.SessionId
                    ?? await _sessionService.GetLatestActiveSessionIdAsync()
                    ?? 0;

                if (sessionId == 0)
                    return Json(new { success = false, message = "No active session found." });

                var terms = request.TermId.HasValue
                    ? new List<int> { request.TermId.Value }
                    : new List<int> { 1, 2, 3 };

                decimal totalExpected = 0, totalCollected = 0;
                var aggregatedCategories = new Dictionary<int, DashboardCategorySummary>();

                foreach (var term in terms)
                {
                    var catSummary = await _paymentReportService.GetDashboardCategorySummaryAsync(sessionId, term);
                    foreach (var cat in catSummary)
                    {
                        totalExpected += cat.Expected;
                        totalCollected += cat.Collected;
                        if (aggregatedCategories.TryGetValue(cat.CategoryId, out var existing))
                        {
                            existing = new DashboardCategorySummary
                            {
                                CategoryId = cat.CategoryId,
                                CategoryName = cat.CategoryName,
                                Expected = existing.Expected + cat.Expected,
                                Collected = existing.Collected + cat.Collected
                            };
                            aggregatedCategories[cat.CategoryId] = existing;
                        }
                        else
                        {
                            aggregatedCategories[cat.CategoryId] = cat;
                        }
                    }
                }

                var categorySummary = aggregatedCategories.Values
                    .Where(c => c.CategoryId > 0)
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                var trend = await _paymentReportService.GetDashboardCategoryTrendAsync(4);
                var trendData = trend.Sessions.Select((session, i) => new DashboardTrendPoint
                {
                    Label = session,
                    Amount = trend.Series.Sum(s => s.Amounts.Count > i ? s.Amounts[i] : 0)
                }).ToList();

                var recentPayments = await _paymentReportService.GetRecentPaymentsAsync(10, sessionId, request.TermId);

                var data = new
                {
                    totalExpected,
                    totalCollected,
                    categorySummary = categorySummary.Select(c => new
                    {
                        c.CategoryId,
                        c.CategoryName,
                        c.Expected,
                        c.Collected,
                        Outstanding = c.Expected - c.Collected
                    }).ToList(),
                    trendData,
                    recentPayments
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════════════
        // ROLE MANAGEMENT
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Policy = "Role.View")]
        public async Task<IActionResult> GetRoles([FromBody] DataTablesRequest request)
        {
            var result = await _roleService.GetRoles(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Role.Create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Json(new { success = false, message = "Role name is required" });
            var result = await _roleService.CreateRole(request.Name, request.Description);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                    User.Identity?.Name ?? "",
                    "CREATE_ROLE", "Roles",
                    $"Created role: {request.Name}");
            }
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Role.Edit")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Json(new { success = false, message = "Role name is required" });
            var result = await _roleService.UpdateRole(request.Id, request.Name, request.Description);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Role.Delete")]
        public async Task<IActionResult> DeleteRole([FromBody] IdRequest request)
        {
            var result = await _roleService.DeleteRole(request.Id.ToString());
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "Role.View")]
        public async Task<IActionResult> GetRoleById([FromBody] RoleIdRequest request)
        {
            var role = await _roleService.GetRoleById(request.Id);
            if (role == null)
                return Json(new { success = false, message = "Role not found" });
            return Json(new { success = true, data = new { role.Id, role.Name, role.Description, role.IsActive } });
        }

        [HttpPost]
        [Authorize(Policy = "Role.View")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _roleService.GetAllPermissions();
            return Json(permissions);
        }

        [HttpPost]
        [Authorize(Policy = "Role.AssignPermission")]
        public async Task<IActionResult> GetRolePermissions([FromBody] RoleIdRequest request)
        {
            var permissionCodes = await _roleService.GetRolePermissionCodes(request.Id);
            return Json(permissionCodes);
        }

        [HttpPost]
        [Authorize(Policy = "Role.AssignPermission")]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsRequest request)
        {
            var result = await _roleService.AssignPermissions(request.RoleId, request.PermissionIds ?? new List<int>());
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                    User.Identity?.Name ?? "",
                    "ASSIGN_PERMISSIONS", "Roles",
                    $"Updated permissions for role: {request.RoleId}");
            }
            return Json(result);
        }

        // ════════════════════════════════════════════════════════════
        // USER MANAGEMENT
        // ════════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize(Policy = "User.View")]
        public async Task<IActionResult> GetUsers([FromBody] DataTablesRequest request)
        {
            var result = await _userManagementService.GetUsers(request);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "User.Create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Json(new { success = false, message = "Email and password are required" });
            var result = await _userManagementService.CreateUser(request.Email, request.Password, request.FullName, request.Roles);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                    User.Identity?.Name ?? "",
                    "CREATE_USER", "Users",
                    $"Created user: {request.Email}");
            }
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "User.Edit")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var result = await _userManagementService.UpdateUser(request.Id, request.FullName, request.IsActive);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "User.Delete")]
        public async Task<IActionResult> DeleteUser([FromBody] UserIdRequest request)
        {
            var result = await _userManagementService.DeleteUser(request.Id);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ResetUserPassword([FromBody] ResetUserPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return Json(new { success = false, message = "New password is required" });
            if (request.NewPassword.Length < 6)
                return Json(new { success = false, message = "Password must be at least 6 characters" });

            var result = await _userManagementService.ResetUserPassword(request.UserId, request.NewPassword);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Policy = "User.View")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _userManagementService.GetAllRoles();
            return Json(roles.Select(r => new { r.Id, r.Name, r.Description }));
        }

        [HttpPost]
        [Authorize(Policy = "User.AssignRole")]
        public async Task<IActionResult> GetUserRoles([FromBody] UserIdRequest request)
        {
            var roleIds = await _userManagementService.GetUserRoleIds(request.Id);
            return Json(roleIds);
        }

        [HttpPost]
        [Authorize(Policy = "User.AssignRole")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesRequest request)
        {
            var result = await _userManagementService.AssignRoles(request.UserId, request.RoleIds ?? new List<string>());
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                    User.Identity?.Name ?? "",
                    "ASSIGN_ROLES", "Users",
                    $"Updated roles for user: {request.UserId}");
            }
            return Json(result);
        }
    }

    // ════════════════════════════════════════════════════════════════
    // REQUEST MODELS
    // ════════════════════════════════════════════════════════════════

    public class UpdatePaymentStateRequest
    {
        public int PaymentId { get; set; }
        public int PaymentState { get; set; }
        public string? RejectMessage { get; set; }
    }

    public class ClassReportRequest : DataTablesRequest
    {
        public int SessionId { get; set; }
        public int? Term { get; set; }
        public int? ClassId { get; set; }
        public int? SubClassId { get; set; }
        public int? CategoryId { get; set; }
        public int? PaymentItemId { get; set; }
    }

    public class SchoolReportRequest : DataTablesRequest
    {
        public int SessionId { get; set; }
        public int Term { get; set; }
    }

    public class CategoryItemReportRequest : DataTablesRequest
    {
        public int SessionId { get; set; }
        public int Term { get; set; }
        public int? CategoryId { get; set; }
        public int? ClassId { get; set; }
    }

    public class IdRequest
    {
        public int Id { get; set; }
    }

    public class ResetPasswordRequest
    {
        public int StudentId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }

    public class IdListRequest
    {
        public List<int> Ids { get; set; }
    }

    public class BatchRegistrationRequest
    {
        public int Term { get; set; }
        public int SessionId { get; set; }
        public int SchoolClassId { get; set; }
        public int SubClassId { get; set; }
        public string? Subjects { get; set; }
        public IFormFile ExcelFile { get; set; }
    }

    public class SearchPromotionStudentsRequest
    {
        public int Term { get; set; }
        public int SessionId { get; set; }
        public int SchoolClassId { get; set; }
        public int SubClassId { get; set; }
    }

    public class BatchPromoteRequest
    {
        public List<long> TermRegIds { get; set; }
        public int Term { get; set; }
        public int SessionId { get; set; }
        public int SchoolClassId { get; set; }
        public int SubClassId { get; set; }
    }

    public class RemoveRegisteredSubjectRequest
    {
        public long ResultTableId { get; set; }
    }

    public class ImportAssessmentRequest
    {
        public int Term { get; set; }
        public int SessionId { get; set; }
        public int SchoolClassId { get; set; }
        public int SubClassId { get; set; }
        public IFormFile ExcelFile { get; set; }
    }

    public class CreateSessionRequest
    {
        public string Name { get; set; }
    }

    public class UpdateSessionRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateClassRequest
    {
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public int ResultType { get; set; }
    }

    public class UpdateClassRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public int ResultType { get; set; }
    }

    public class CreateSubClassRequest
    {
        public string Name { get; set; }
    }

    public class UpdateSubClassRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateSubjectRequest
    {
        public string Name { get; set; }
        public string? Code { get; set; }
    }

    public class UpdateSubjectRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
    }

    public class AssignSubjectRequest
    {
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int? SubClassId { get; set; }
    }

    public class CreateStudentRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? OtherName { get; set; }
        public int Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? State { get; set; }
        public string? LocalGov { get; set; }
        public string? Address { get; set; }
        public string? PicturePath { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateStudentRequest
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? OtherName { get; set; }
        public int Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? State { get; set; }
        public string? LocalGov { get; set; }
        public string? Address { get; set; }
        public string? PicturePath { get; set; }
    }

    public class UpdateParentRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Occupation { get; set; }
        public string? Address { get; set; }
        public string Phone1 { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
    }

    public class SearchStudentRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class AddOrLinkParentRequest
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Occupation { get; set; }
        public string? Address { get; set; }
        public string Phone1 { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
    }

    public class LinkParentToStudentRequest
    {
        public int StudentId { get; set; }
        public int ParentGuardianId { get; set; }
        public bool IsPrimaryContact { get; set; }
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class RoleIdRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    public class AssignPermissionsRequest
    {
        public string RoleId { get; set; } = string.Empty;
        public List<int>? PermissionIds { get; set; }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class UserIdRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    public class AssignRolesRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<string>? RoleIds { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public string? EmployeeType { get; set; }
        public string? Address { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateEmployeeRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public string? EmployeeType { get; set; }
        public string? Address { get; set; }
    }

    public class ResetUserPasswordRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}