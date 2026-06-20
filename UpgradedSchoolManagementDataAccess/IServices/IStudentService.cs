using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IStudentService
    {
        Task<DataTablesResponse<StudentDto>> GetStudents(DataTablesRequest request);
        Task<StudentsTable?> GetStudentById(int id);
        Task<ApiResponse<StudentsTable>> CreateStudent(CreateStudentInput input);
        Task<ApiResponse<StudentsTable>> UpdateStudent(UpdateStudentInput input);
        Task<ApiResponse<bool>> DeleteStudent(int id, string webRootPath);
        Task<ApiResponse<object>> ResetStudentPassword(int studentId, string newPassword);
        Task<ApiResponse<object>> ToggleStudentStatus(int studentId);
        Task<string> GenerateAdmissionNumber();
        Task<StudentDto?> FindStudentByAdmissionNumberAsync(string admissionNumber);
        Task<StudentsTable?> GetStudentByUserId(string userId);
    }

    public class CreateStudentInput
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
        /// <summary>Default password applied to all new student accounts.</summary>
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateStudentInput
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

    public class StudentDto
    {
        public int Id { get; set; }
        public string? AdmissionNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? State { get; set; }
        public string? LocalGov { get; set; }
        public string? PicturePath { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
