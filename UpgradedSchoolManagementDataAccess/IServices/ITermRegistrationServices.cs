using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface ITermRegistrationServices
    {
        /// <summary>
        /// Server-side DataTables: returns paged, searched, and filtered term registrations.
        /// </summary>
        Task<(List<TermRegDto> data, int recordsTotal, int recordsFiltered)> GetStudentTermRegistrationAsync(
            int    skip           = 0,
            int    pageSize       = 10,
            string searchTerm     = "",
            int    sortColumn     = 0,
            string sortDirection  = "asc",
            int?   termFilter     = null,
            int?   sessionFilter  = null,
            int?   classFilter    = null,
            int?   subclassFilter = null);

        Task<TermRegistrationViewModel> GetStudentTermRegistrationByIdAsync(int id);

        Task<ApiResponse<int>> CreateStudentTermRegistrationAsync(TermRegistrationViewModel model);

        Task<ApiResponse<int>> UpdateStudentTermRegistrationAsync(TermRegistrationViewModel model);

        /// <summary>
        /// Deletes a single term registration.
        /// Blocked if any linked ResultTable row has Status == true (results already recorded).
        /// </summary>
        Task<ApiResponse<bool>> DeleteStudentTermRegistrationAsync(int id);

        /// <summary>
        /// Removes a single subject (ResultTable row) from a registration.
        /// Blocked if the subject has recorded results (Status == true).
        /// </summary>
        Task<ApiResponse<bool>> RemoveSubjectFromResultTableAsync(long resultTableId);

        /// <summary>
        /// Bulk-deletes term registrations.
        /// Blocked for any individual registration where a linked ResultTable row has Status == true.
        /// </summary>
        Task<ApiResponse<bool>> DeleteStudentsTermRegistrationAsync(List<int> ids);

        /// <summary>
        /// Registers multiple students via an Excel upload extracted list.
        /// </summary>
        Task<BatchRegistrationResult> BatchRegisterStudentsAsync(
            List<string> registrationNumbers, 
            int classId, 
            int sessionId, 
            int subClassId, 
            Term term, 
            List<int>? subjectIds);

        /// <summary>
        /// Retrieves term registrations based on specific criteria for batch promotion.
        /// </summary>
        Task<List<TermRegDto>> GetStudentsForBatchPromotionAsync(int term, int sessionId, int classId, int subclassId);

        /// <summary>
        /// Promotes a batch of students to a new term/session/class using their previous registration IDs.
        /// It inherits the subjects from the previous registration.
        /// </summary>
        Task<BatchRegistrationResult> BatchPromoteStudentsAsync(List<long> previousTermRegIds, int classId, int sessionId, int subClassId, Term term);

        Task<List<TermRegDto>> GetStudentTermRegistrationsAsync(int studentId);
        Task<List<AttendanceViewModel>> GetAllStudentAttendanceTermRegistrationsAsync(TermObjects model);
        Task<ApiResponse<bool>> UpdateStudentAttendanceAsync(List<AttendanceViewModel> attendanceUpdates, TermObjects termObjects);
    }
}
