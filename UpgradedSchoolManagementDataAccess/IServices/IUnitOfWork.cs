using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IUnitOfWork
    {
        public IAuditLogService AuditLogService { get; }
        public IClassService ClassService { get; }
        public IParentGuardianService ParentGuardianService { get; }
        public IStudentService StudentService { get; }
        public ISessionService SessionService { get; }
        public ISubjectService SubjectService { get; }
        public ISidebarService SidebarService { get; }
        public ISubClassService SubClassService { get; }//
        public IViewSelectionService ViewSelectionService { get; }
        public IUserPermissionService UserPermissionService { get; }
        public ITermRegistrationServices TermRegistrationServices { get; }
        public IStudentPaymentService StudentPaymentService { get; }
        public IPaymentItemService PaymentItemService { get; }
        public IPaymentSetupService PaymentSetupService { get; }
        public IPaymentCategoryService PaymentCategoryService { get; }
        public IPaymentReportService PaymentReportService { get; }
        public IAppSettingsService AppSettingsServices { get; }
        public IClassTermInformationService ClassTermInformationServices { get; }
        public ITermGeneralInformationService TermGeneralInformationServices { get; }
        public IResultManagerService ResultManagerServices { get; }
        public IResultSkillService ResultSkillServices { get; }
        public IEmployeeService EmployeeServices { get; }
        
    }
}
