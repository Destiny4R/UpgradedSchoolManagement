using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    /// <summary>
    /// Service interface for managing parent/guardian records and their links to students.
    /// </summary>
    public interface IParentGuardianService
    {
        /// <summary>
        /// DataTables listing of all parents/guardians.
        /// </summary>
        Task<DataTablesResponse<ParentGuardianDto>> GetParents(DataTablesRequest request);

        /// <summary>
        /// Creates a new parent/guardian record (standalone, no student link).
        /// </summary>
        Task<ParentOperationResultDto> CreateParentAsync(ParentGuardianCreateDto dto);

        /// <summary>
        /// Attempts to create a new parent or link to an existing one.
        /// If phone number matches an existing parent, returns conflict response.
        /// </summary>
        /// <param name="dto">Parent information to create</param>
        /// <param name="studentId">Student ID to link to</param>
        /// <returns>Result containing parent data or conflict information</returns>
        Task<ParentOperationResultDto> AddOrLinkParentAsync(ParentGuardianCreateDto dto, int studentId);

        /// <summary>
        /// Links an existing parent to a student.
        /// Checks for duplicate links (idempotent).
        /// </summary>
        /// <param name="parentId">Parent ID</param>
        /// <param name="studentId">Student ID</param>
        /// <param name="isPrimaryContact">Whether this is the primary contact</param>
        /// <returns>Result of the link operation</returns>
        Task<ParentOperationResultDto> LinkExistingParentToStudentAsync(int parentId, int studentId, bool isPrimaryContact = false);

        /// <summary>
        /// Gets parent by ID with related student links.
        /// </summary>
        /// <param name="parentId">Parent ID</param>
        /// <returns>Parent DTO or null if not found</returns>
        Task<ParentGuardianDto?> GetParentByIdAsync(int parentId);

        /// <summary>
        /// Gets all parents linked to a specific student.
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>List of parent DTOs</returns>
        Task<List<ParentGuardianDto>> GetParentsByStudentAsync(int studentId);

        /// <summary>
        /// Checks if a phone number already exists in the system.
        /// </summary>
        /// <param name="normalizedPhone">Normalized phone number (digits only)</param>
        /// <returns>Parent DTO if found, null otherwise</returns>
        Task<ParentGuardianDto?> GetParentByPhoneAsync(string normalizedPhone);

        /// <summary>
        /// Updates an existing parent's information.
        /// </summary>
        /// <param name="parentId">Parent ID to update</param>
        /// <param name="dto">Updated parent information</param>
        /// <returns>Result of the update operation</returns>
        Task<ParentOperationResultDto> UpdateParentAsync(int parentId, ParentGuardianCreateDto dto);

        /// <summary>
        /// Deletes a parent/guardian record.
        /// Also removes all links to students.
        /// </summary>
        /// <param name="parentId">Parent ID to delete</param>
        /// <returns>Result of the delete operation</returns>
        Task<ParentOperationResultDto> DeleteParentAsync(int parentId);

        /// <summary>
        /// Unlinks a parent from a student.
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="parentId">Parent ID to unlink</param>
        /// <returns>Result of the unlink operation</returns>
        Task<ParentOperationResultDto> UnlinkParentFromStudentAsync(int studentId, int parentId);

        /// <summary>
        /// Gets all students linked to a specific parent.
        /// </summary>
        /// <param name="parentId">Parent ID</param>
        /// <returns>List of student IDs linked to this parent</returns>
        Task<List<int>> GetStudentsByParentAsync(int parentId);

        /// <summary>
        /// Marks a parent as the primary contact for a student.
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="parentId">Parent ID to set as primary</param>
        /// <returns>Result of the operation</returns>
        Task<ParentOperationResultDto> SetPrimaryContactAsync(int studentId, int parentId);
    }
}
