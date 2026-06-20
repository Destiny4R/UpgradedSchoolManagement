using Microsoft.Extensions.Options;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.terminal_result
{
    public class junior_resultModel : TerminalResultPageBaseModel
    {
        public junior_resultModel(ApplicationDbContext db, IResultSkillService resultSkillService, IOptions<SchoolConfigurationSetup> schoolConfig) : base(db, resultSkillService, schoolConfig)
        {
        }

        public override ResultType ResultType => ResultType.Jss;
    }
}
