using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.terminal_result
{
    [Authorize(Policy = "Result.View")]
    public class nurseryModel : TerminalResultPageBaseModel
    {
        public nurseryModel(ApplicationDbContext db, IResultSkillService resultSkillService, IOptions<SchoolConfigurationSetup> schoolConfig) : base(db, resultSkillService, schoolConfig)
        {
        }

        public override ResultType ResultType => ResultType.Nursery;
    }
}
