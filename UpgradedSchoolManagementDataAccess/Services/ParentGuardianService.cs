using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementUltitlities;

namespace UpgradedSchoolManagementDataAccess.Services
{
    /// <summary>
    /// Service for managing parent/guardian records and student-parent links.
    /// Handles phone deduplication, linking logic, and related operations.
    /// </summary>
    public class ParentGuardianService : IParentGuardianService
    {
        private readonly ApplicationDbContext _context;

        public ParentGuardianService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataTablesResponse<ParentGuardianDto>> GetParents(DataTablesRequest request)
        {
            try
            {
                var query = _context.StudentParentLinks
                    .Include(l => l.ParentGuardian)
                        .ThenInclude(p => p.StudentLinks)
                            .ThenInclude(l => l.Student)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var sv = request.Search.Value.ToLower();
                    query = query.Where(l =>
                        l.ParentGuardian.FullName.ToLower().Contains(sv) ||
                        l.ParentGuardian.Relationship.ToLower().Contains(sv) ||
                        l.ParentGuardian.Phone1.Contains(sv) ||
                        (l.ParentGuardian.Phone2 != null && l.ParentGuardian.Phone2.Contains(sv)) ||
                        (l.ParentGuardian.Occupation != null && l.ParentGuardian.Occupation.ToLower().Contains(sv)));
                }

                var recordsFiltered = await query.CountAsync();

                if (request.Order != null && request.Order.Any())
                {
                    var order = request.Order.First();
                    var raw = request.Columns != null && order.Column < request.Columns.Count
                        ? request.Columns[order.Column].Data ?? string.Empty
                        : string.Empty;
                    var col = raw.Length > 0 ? char.ToUpper(raw[0]) + raw.Substring(1) : string.Empty;

                    query = col switch
                    {
                        "FullName" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.ParentGuardian.FullName)
                            : query.OrderByDescending(l => l.ParentGuardian.FullName),
                        "Relationship" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.ParentGuardian.Relationship)
                            : query.OrderByDescending(l => l.ParentGuardian.Relationship),
                        "Phone1" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.ParentGuardian.Phone1)
                            : query.OrderByDescending(l => l.ParentGuardian.Phone1),
                        "CreatedAt" => order.Dir.ToLower() == "asc"
                            ? query.OrderBy(l => l.ParentGuardian.CreatedAt)
                            : query.OrderByDescending(l => l.ParentGuardian.CreatedAt),
                        _ => query.OrderBy(l => l.ParentGuardian.FullName)
                    };
                }
                else
                {
                    query = query.OrderBy(l => l.ParentGuardian.FullName);
                }

                var length = request.Length > 0 ? request.Length : 10;

                var links = await query
                    .Skip(request.Start)
                    .Take(length)
                    .ToListAsync();

                var data = links.Select(l => new ParentGuardianDto
                {
                    Id = l.ParentGuardian.Id,
                    FullName = l.ParentGuardian.FullName,
                    Relationship = l.ParentGuardian.Relationship,
                    Occupation = l.ParentGuardian.Occupation,
                    Address = l.ParentGuardian.Address,
                    Phone1 = l.ParentGuardian.Phone1,
                    Phone2 = l.ParentGuardian.Phone2,
                    CreatedAt = l.ParentGuardian.CreatedAt,
                    UpdatedAt = l.ParentGuardian.UpdatedAt,
                    Children = l.ParentGuardian.StudentLinks.Select(sl => new StudentChildDto
                    {
                        Id = sl.Student.Id,
                        FullName = sl.Student.Surname + ", " + sl.Student.FirstName + (sl.Student.OtherName != null ? " " + sl.Student.OtherName : ""),
                        AdmissionNumber = sl.Student.AdmissionNumber
                    }).ToList()
                }).ToList();

                return new DataTablesResponse<ParentGuardianDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<ParentGuardianDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<ParentGuardianDto>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<ParentOperationResultDto> CreateParentAsync(ParentGuardianCreateDto dto)
        {
            string normalizedPhone1 = SD.NormalizePhone(dto.Phone1);
            string normalizedPhone2 = SD.NormalizePhone(dto.Phone2);

            if (!SD.IsValidPhone(normalizedPhone1))
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Phone 1 must be at least 10 digits."
                };
            }

            if (!string.IsNullOrEmpty(normalizedPhone2) && !SD.IsValidPhone(normalizedPhone2))
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Phone 2 must be at least 10 digits if provided."
                };
            }

            var existingByPhone1 = await _context.ParentGuardians
                .FirstOrDefaultAsync(p => p.Phone1 == normalizedPhone1);

            if (existingByPhone1 != null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    IsConflict = true,
                    Message = $"Parent with phone {SD.FormatPhoneForDisplay(normalizedPhone1)} already exists.",
                    Parent = MapToDto(existingByPhone1)
                };
            }

            if (!string.IsNullOrEmpty(normalizedPhone2))
            {
                var existingByPhone2 = await _context.ParentGuardians
                    .FirstOrDefaultAsync(p => p.Phone2 == normalizedPhone2);

                if (existingByPhone2 != null)
                {
                    return new ParentOperationResultDto
                    {
                        Success = false,
                        IsConflict = true,
                        Message = $"Parent with phone {SD.FormatPhoneForDisplay(normalizedPhone2)} already exists.",
                        Parent = MapToDto(existingByPhone2)
                    };
                }
            }

            var newParent = new ParentGuardian
            {
                FullName = dto.FullName,
                Relationship = dto.Relationship,
                Occupation = dto.Occupation,
                Address = dto.Address,
                Phone1 = normalizedPhone1,
                Phone2 = string.IsNullOrEmpty(normalizedPhone2) ? null : normalizedPhone2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ParentGuardians.Add(newParent);
            await _context.SaveChangesAsync();

            return new ParentOperationResultDto
            {
                Success = true,
                Message = "Parent/Guardian created successfully.",
                Parent = MapToDto(newParent)
            };
        }

        public async Task<ParentOperationResultDto> AddOrLinkParentAsync(ParentGuardianCreateDto dto, int studentId)
        {
            // Validate student exists
            var studentExists = await _context.StudentsTables.AnyAsync(s => s.Id == studentId);
            if (!studentExists)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Student not found."
                };
            }

            // Check if student already has a parent
            var hasParent = await _context.StudentParentLinks.AnyAsync(l => l.StudentId == studentId);
            if (hasParent)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "This student already has a parent/guardian assigned."
                };
            }

            // Normalize phone numbers
            string normalizedPhone1 = SD.NormalizePhone(dto.Phone1);
            string normalizedPhone2 = SD.NormalizePhone(dto.Phone2);

            // Validate phone numbers
            if (!SD.IsValidPhone(normalizedPhone1))
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Phone 1 must be at least 10 digits."
                };
            }

            if (!string.IsNullOrEmpty(normalizedPhone2) && !SD.IsValidPhone(normalizedPhone2))
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Phone 2 must be at least 10 digits if provided."
                };
            }

            // Check for existing parent by Phone1
            var existingByPhone1 = await _context.ParentGuardians
                .FirstOrDefaultAsync(p => p.Phone1 == normalizedPhone1);

            if (existingByPhone1 != null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    IsConflict = true,
                    Message = $"Parent with phone {SD.FormatPhoneForDisplay(normalizedPhone1)} already exists.",
                    Parent = new ParentGuardianDto
                    {
                        Id = existingByPhone1.Id,
                        FullName = existingByPhone1.FullName,
                        Relationship = existingByPhone1.Relationship,
                        Occupation = existingByPhone1.Occupation,
                        Address = existingByPhone1.Address,
                        Phone1 = SD.FormatPhoneForDisplay(existingByPhone1.Phone1),
                        Phone2 = !string.IsNullOrEmpty(existingByPhone1.Phone2) ? SD.FormatPhoneForDisplay(existingByPhone1.Phone2) : null,
                        CreatedAt = existingByPhone1.CreatedAt,
                        UpdatedAt = existingByPhone1.UpdatedAt
                    }
                };
            }

            // Check for existing parent by Phone2 if provided
            if (!string.IsNullOrEmpty(normalizedPhone2))
            {
                var existingByPhone2 = await _context.ParentGuardians
                    .FirstOrDefaultAsync(p => p.Phone2 == normalizedPhone2);

                if (existingByPhone2 != null)
                {
                    return new ParentOperationResultDto
                    {
                        Success = false,
                        IsConflict = true,
                        Message = $"Parent with phone {SD.FormatPhoneForDisplay(normalizedPhone2)} already exists.",
                        Parent = new ParentGuardianDto
                        {
                            Id = existingByPhone2.Id,
                            FullName = existingByPhone2.FullName,
                            Relationship = existingByPhone2.Relationship,
                            Occupation = existingByPhone2.Occupation,
                            Address = existingByPhone2.Address,
                            Phone1 = SD.FormatPhoneForDisplay(existingByPhone2.Phone1),
                            Phone2 = !string.IsNullOrEmpty(existingByPhone2.Phone2) ? SD.FormatPhoneForDisplay(existingByPhone2.Phone2) : null,
                            CreatedAt = existingByPhone2.CreatedAt,
                            UpdatedAt = existingByPhone2.UpdatedAt
                        }
                    };
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create new parent
                var newParent = new ParentGuardian
                {
                    FullName = dto.FullName,
                    Relationship = dto.Relationship,
                    Occupation = dto.Occupation,
                    Address = dto.Address,
                    Phone1 = normalizedPhone1,
                    Phone2 = string.IsNullOrEmpty(normalizedPhone2) ? null : normalizedPhone2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ParentGuardians.Add(newParent);
                await _context.SaveChangesAsync();

                // Create link to student
                var studentParentLink = new StudentParentLink
                {
                    StudentId = studentId,
                    ParentGuardianId = newParent.Id,
                    LinkedAt = DateTime.UtcNow,
                    IsPrimaryContact = true
                };

                _context.StudentParentLinks.Add(studentParentLink);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ParentOperationResultDto
                {
                    Success = true,
                    Message = "Parent/Guardian created and linked successfully.",
                    Parent = new ParentGuardianDto
                    {
                        Id = newParent.Id,
                        FullName = newParent.FullName,
                        Relationship = newParent.Relationship,
                        Occupation = newParent.Occupation,
                        Address = newParent.Address,
                        Phone1 = SD.FormatPhoneForDisplay(newParent.Phone1),
                        Phone2 = !string.IsNullOrEmpty(newParent.Phone2) ? SD.FormatPhoneForDisplay(newParent.Phone2) : null,
                        CreatedAt = newParent.CreatedAt,
                        UpdatedAt = newParent.UpdatedAt
                    }
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ParentOperationResultDto> LinkExistingParentToStudentAsync(int parentId, int studentId, bool isPrimaryContact = false)
        {
            // Validate both entities exist
            var parent = await _context.ParentGuardians.FindAsync(parentId);
            if (parent == null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Parent/Guardian not found."
                };
            }

            var student = await _context.StudentsTables.FindAsync(studentId);
            if (student == null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Student not found."
                };
            }

            // Check if student already has a parent
            var existingLink = await _context.StudentParentLinks
                .FirstOrDefaultAsync(l => l.StudentId == studentId);

            if (existingLink != null)
            {
                // If it's the same parent, return success (idempotent)
                if (existingLink.ParentGuardianId == parentId)
                {
                    return new ParentOperationResultDto
                    {
                        Success = true,
                        Message = "Parent/Guardian is already linked to this student."
                    };
                }

                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "This student already has a parent/guardian assigned. Please unlink the current parent first."
                };
            }

            // Create new link
            var newLink = new StudentParentLink
            {
                StudentId = studentId,
                ParentGuardianId = parentId,
                LinkedAt = DateTime.UtcNow,
                IsPrimaryContact = isPrimaryContact
            };

            _context.StudentParentLinks.Add(newLink);
            await _context.SaveChangesAsync();

            return new ParentOperationResultDto
            {
                Success = true,
                Message = "Parent/Guardian linked to student successfully."
            };
        }

        public async Task<ParentGuardianDto?> GetParentByIdAsync(int parentId)
        {
            var parent = await _context.ParentGuardians
                .Include(p => p.StudentLinks)
                    .ThenInclude(l => l.Student)
                .FirstOrDefaultAsync(p => p.Id == parentId);

            if (parent == null) return null;

            return MapToDto(parent);
        }

        public async Task<List<ParentGuardianDto>> GetParentsByStudentAsync(int studentId)
        {
            var parents = await _context.StudentParentLinks
                .Where(l => l.StudentId == studentId)
                .Include(l => l.ParentGuardian)
                .Select(l => l.ParentGuardian)
                .ToListAsync();

            return parents.Select(p => new ParentGuardianDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Relationship = p.Relationship,
                Occupation = p.Occupation,
                Address = p.Address,
                Phone1 = SD.FormatPhoneForDisplay(p.Phone1),
                Phone2 = !string.IsNullOrEmpty(p.Phone2) ? SD.FormatPhoneForDisplay(p.Phone2) : null,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<ParentGuardianDto?> GetParentByPhoneAsync(string normalizedPhone)
        {
            var parent = await _context.ParentGuardians
                .FirstOrDefaultAsync(p => p.Phone1 == normalizedPhone || p.Phone2 == normalizedPhone);

            if (parent == null) return null;

            return new ParentGuardianDto
            {
                Id = parent.Id,
                FullName = parent.FullName,
                Relationship = parent.Relationship,
                Occupation = parent.Occupation,
                Address = parent.Address,
                Phone1 = SD.FormatPhoneForDisplay(parent.Phone1),
                Phone2 = !string.IsNullOrEmpty(parent.Phone2) ? SD.FormatPhoneForDisplay(parent.Phone2) : null,
                CreatedAt = parent.CreatedAt,
                UpdatedAt = parent.UpdatedAt
            };
        }

        public async Task<ParentOperationResultDto> UpdateParentAsync(int parentId, ParentGuardianCreateDto dto)
        {
            var parent = await _context.ParentGuardians.FindAsync(parentId);
            if (parent == null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Parent/Guardian not found."
                };
            }

            // Normalize new phone numbers
            string normalizedPhone1 = SD.NormalizePhone(dto.Phone1);
            string normalizedPhone2 = SD.NormalizePhone(dto.Phone2);

            // Validate phone numbers
            if (!SD.IsValidPhone(normalizedPhone1))
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Phone 1 must be at least 10 digits."
                };
            }

            // Check for duplicates (excluding self)
            if (normalizedPhone1 != parent.Phone1)
            {
                var existingPhone1 = await _context.ParentGuardians
                    .AnyAsync(p => p.Id != parentId && p.Phone1 == normalizedPhone1);

                if (existingPhone1)
                {
                    return new ParentOperationResultDto
                    {
                        Success = false,
                        Message = "Phone 1 is already in use by another parent."
                    };
                }
            }

            if (!string.IsNullOrEmpty(normalizedPhone2) && normalizedPhone2 != parent.Phone2)
            {
                var existingPhone2 = await _context.ParentGuardians
                    .AnyAsync(p => p.Id != parentId && p.Phone2 == normalizedPhone2);

                if (existingPhone2)
                {
                    return new ParentOperationResultDto
                    {
                        Success = false,
                        Message = "Phone 2 is already in use by another parent."
                    };
                }
            }

            // Update fields
            parent.FullName = dto.FullName;
            parent.Relationship = dto.Relationship;
            parent.Occupation = dto.Occupation;
            parent.Address = dto.Address;
            parent.Phone1 = normalizedPhone1;
            parent.Phone2 = string.IsNullOrEmpty(normalizedPhone2) ? null : normalizedPhone2;
            parent.UpdatedAt = DateTime.UtcNow;

            _context.ParentGuardians.Update(parent);
            await _context.SaveChangesAsync();

            return new ParentOperationResultDto
            {
                Success = true,
                Message = "Parent/Guardian updated successfully.",
                Parent = new ParentGuardianDto
                {
                    Id = parent.Id,
                    FullName = parent.FullName,
                    Relationship = parent.Relationship,
                    Occupation = parent.Occupation,
                    Address = parent.Address,
                    Phone1 = SD.FormatPhoneForDisplay(parent.Phone1),
                    Phone2 = !string.IsNullOrEmpty(parent.Phone2) ? SD.FormatPhoneForDisplay(parent.Phone2) : null,
                    CreatedAt = parent.CreatedAt,
                    UpdatedAt = parent.UpdatedAt
                }
            };
        }

        public async Task<ParentOperationResultDto> DeleteParentAsync(int parentId)
        {
            var parent = await _context.ParentGuardians
                .Include(p => p.StudentLinks)
                .FirstOrDefaultAsync(p => p.Id == parentId);

            if (parent == null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Parent/Guardian not found."
                };
            }

            // Remove all links
            _context.StudentParentLinks.RemoveRange(parent.StudentLinks);

            // Remove parent
            _context.ParentGuardians.Remove(parent);
            await _context.SaveChangesAsync();

            return new ParentOperationResultDto
            {
                Success = true,
                Message = "Parent/Guardian and all student links deleted successfully."
            };
        }

        public async Task<ParentOperationResultDto> UnlinkParentFromStudentAsync(int studentId, int parentId)
        {
            var link = await _context.StudentParentLinks
                .FirstOrDefaultAsync(l => l.StudentId == studentId && l.ParentGuardianId == parentId);

            if (link == null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Link between student and parent not found."
                };
            }

            _context.StudentParentLinks.Remove(link);
            await _context.SaveChangesAsync();

            return new ParentOperationResultDto
            {
                Success = true,
                Message = "Parent/Guardian unlinked from student successfully."
            };
        }

        public async Task<List<int>> GetStudentsByParentAsync(int parentId)
        {
            return await _context.StudentParentLinks
                .Where(l => l.ParentGuardianId == parentId)
                .Select(l => l.StudentId)
                .ToListAsync();
        }

        public async Task<ParentOperationResultDto> SetPrimaryContactAsync(int studentId, int parentId)
        {
            // Get current primary for this student
            var currentPrimary = await _context.StudentParentLinks
                .FirstOrDefaultAsync(l => l.StudentId == studentId && l.IsPrimaryContact);

            if (currentPrimary != null)
            {
                currentPrimary.IsPrimaryContact = false;
            }

            // Set new primary
            var newPrimary = await _context.StudentParentLinks
                .FirstOrDefaultAsync(l => l.StudentId == studentId && l.ParentGuardianId == parentId);

            if (newPrimary == null)
            {
                return new ParentOperationResultDto
                {
                    Success = false,
                    Message = "Link between student and parent not found."
                };
            }

            newPrimary.IsPrimaryContact = true;
            await _context.SaveChangesAsync();

            return new ParentOperationResultDto
            {
                Success = true,
                Message = "Primary contact updated successfully."
            };
        }

        private static ParentGuardianDto MapToDto(ParentGuardian p)
        {
            return new ParentGuardianDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Relationship = p.Relationship,
                Occupation = p.Occupation,
                Address = p.Address,
                Phone1 = SD.FormatPhoneForDisplay(p.Phone1),
                Phone2 = !string.IsNullOrEmpty(p.Phone2) ? SD.FormatPhoneForDisplay(p.Phone2) : null,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Children = p.StudentLinks?.Select(l => new StudentChildDto
                {
                    Id = l.Student.Id,
                    FullName = l.Student.Surname + ", " + l.Student.FirstName + (l.Student.OtherName != null ? " " + l.Student.OtherName : ""),
                    AdmissionNumber = l.Student.AdmissionNumber
                }).ToList() ?? new List<StudentChildDto>()
            };
        }
    }
}
