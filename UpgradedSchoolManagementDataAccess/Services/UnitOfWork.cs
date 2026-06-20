using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.IServices;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork( IAuditLogService auditLogService, IClassService classService, IParentGuardianService parentGuardianService, IStudentService studentService, ISessionService sessionService, ISubjectService subjectService, ISidebarService sidebarService, ISubClassService subClassService, IViewSelectionService viewSelectionService, IUserPermissionService userPermissionService, ITermRegistrationServices termRegistrationServices, IStudentPaymentService studentPaymentService, IPaymentItemService paymentItemService, IPaymentSetupService paymentSetupService, IPaymentCategoryService paymentCategoryService, IPaymentReportService paymentReportService, IAppSettingsService appSettingsService, IClassTermInformationService classTermInformationService, ITermGeneralInformationService termGeneralInformationService, IResultManagerService resultManagerService, IResultSkillService resultSkillService, IEmployeeService employeeService)
        {
            AuditLogService = auditLogService;
            ClassService = classService;
            ParentGuardianService = parentGuardianService;
            StudentService = studentService;
            SessionService = sessionService;
            SubjectService = subjectService;
            SidebarService = sidebarService;
            SubClassService = subClassService;
            ViewSelectionService = viewSelectionService;
            UserPermissionService = userPermissionService;
            TermRegistrationServices = termRegistrationServices;
            StudentPaymentService = studentPaymentService;
            PaymentItemService = paymentItemService;
            PaymentSetupService = paymentSetupService;
            PaymentCategoryService = paymentCategoryService;
            PaymentReportService = paymentReportService;
            AppSettingsServices = appSettingsService;
            ClassTermInformationServices = classTermInformationService;
            TermGeneralInformationServices = termGeneralInformationService;
            ResultManagerServices = resultManagerService;
            ResultSkillServices = resultSkillService;
            EmployeeServices = employeeService;
        }

        public IAuditLogService AuditLogService { get; set; }

        public IClassService ClassService { get; set; }

        public IParentGuardianService ParentGuardianService { get; set; }

        public IStudentService StudentService { get; set; }

        public ISessionService SessionService { get; set; }

        public ISubjectService SubjectService { get; set; }

        public ISidebarService SidebarService { get; set; }

        public ISubClassService SubClassService { get; set; }
        public IViewSelectionService ViewSelectionService { get; set; }
        public IUserPermissionService UserPermissionService { get; private set; }
        public ITermRegistrationServices TermRegistrationServices { get; private set; }
        public IStudentPaymentService StudentPaymentService { get; private set; }
        public IPaymentItemService PaymentItemService { get; private set; }
        public IPaymentSetupService PaymentSetupService { get; private set; }
        public IPaymentCategoryService PaymentCategoryService { get; private set; }
        public IPaymentReportService PaymentReportService { get; private set; }
        public IAppSettingsService AppSettingsServices { get; private set; }
        public IClassTermInformationService ClassTermInformationServices { get; private set; }
        public ITermGeneralInformationService TermGeneralInformationServices { get; private set; }
        public IResultManagerService ResultManagerServices { get; private set; }
        public IResultSkillService ResultSkillServices { get; private set; }
        public IEmployeeService EmployeeServices { get; private set; }
    }
}
