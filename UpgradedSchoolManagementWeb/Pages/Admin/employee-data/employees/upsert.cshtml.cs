using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.Admin.employee_data.employees
{
    public class upsertModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public upsertModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        [BindProperty]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        public int Gender { get; set; }

        [BindProperty]
        public string? EmployeeType { get; set; }

        [BindProperty]
        public string? Address { get; set; }

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public int Id { get; set; }
        public bool IsEdit => Id > 0;
        public string? EmployeeCode { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                var employee = await _unitOfWork.EmployeeServices.GetEmployeeById(id.Value);
                if (employee == null)
                    return RedirectToPage("index");

                Id = employee.Id;
                FullName = employee.FullName;
                Gender = (int)employee.Gender;
                EmployeeType = employee.EmployeeType;
                Address = employee.Address;
                EmployeeCode = employee.EmployeeCode;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required");
                return Page();
            }
            if (Gender != 1 && Gender != 2)
            {
                ModelState.AddModelError("Gender", "Please select a gender");
                return Page();
            }

            if (IsEdit)
            {
                var input = new UpdateEmployeeInput
                {
                    Id = Id,
                    FullName = FullName,
                    Gender = Gender,
                    EmployeeType = EmployeeType,
                    Address = Address
                };
                var result = await _unitOfWork.EmployeeServices.UpdateEmployee(input);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }
                TempData["Error"] = result.Message;
                return RedirectToPage(new {id = Id});
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ModelState.AddModelError("Password", "Password is required");
                    return Page();
                }

                var input = new CreateEmployeeInput
                {
                    FullName = FullName,
                    Gender = Gender,
                    EmployeeType = EmployeeType,
                    Address = Address,
                    Password = Password
                };
                var result = await _unitOfWork.EmployeeServices.CreateEmployee(input);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }
                TempData["Error"] = result.Message;
                return Page();
            }
        }
    }
}
