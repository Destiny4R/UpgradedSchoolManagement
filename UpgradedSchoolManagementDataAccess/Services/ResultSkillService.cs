using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class ResultSkillService : IResultSkillService
    {
        private readonly ApplicationDbContext _context;

        public ResultSkillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ResultSkillDto>> GetActiveSkillsAsync()
        {
            return await _context.ResultSkills
                .Where(s => s.IsActive)
                .OrderBy(s => s.Domain)
                .ThenBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .Select(s => new ResultSkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Domain = s.Domain,
                    DomainName = s.Domain.ToString(),
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<List<ResultSkillDto>> GetAssignedSkillsByClassIdAsync(int schoolClassId)
        {
            return await _context.ClassResultSkills
                .Where(crs => crs.SchoolClassId == schoolClassId && crs.IsActive && crs.ResultSkill.IsActive)
                .OrderBy(crs => crs.ResultSkill.Domain)
                .ThenBy(crs => crs.ResultSkill.DisplayOrder)
                .Select(crs => new ResultSkillDto
                {
                    Id = crs.ResultSkill.Id,
                    Name = crs.ResultSkill.Name,
                    ClassName = crs.SchoolClass.Name,
                    Domain = crs.ResultSkill.Domain,
                    DomainName = crs.ResultSkill.Domain.ToString(),
                    DisplayOrder = crs.ResultSkill.DisplayOrder,
                    IsActive = crs.ResultSkill.IsActive
                })
                .ToListAsync();
        }

        public async Task<DataTablesResponse<ResultSkillDto>> GetSkillsForDataTableAsync(DataTablesRequest request)
        {
            try
            {
                var query = _context.ResultSkills
                    .AsNoTracking()
                    .Select(s => new ResultSkillDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Domain = s.Domain,
                        DomainName = s.Domain.ToString(),
                        DisplayOrder = s.DisplayOrder,
                        IsActive = s.IsActive,
                        CreatedDate = s.CreatedDate,
                        UpdatedDate = s.UpdatedDate
                    })
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(request.Search?.Value))
                {
                    var search = request.Search.Value.ToLower();
                    query = query.Where(s =>
                        s.Name.ToLower().Contains(search) ||
                        s.DomainName.ToLower().Contains(search));
                }

                var recordsFiltered = await query.CountAsync();
                var length = request.Length > 0 ? request.Length : 10;

                query = request.Order != null && request.Order.Any()
                    ? ApplySkillOrder(query, request)
                    : query.OrderBy(s => s.Domain).ThenBy(s => s.DisplayOrder).ThenBy(s => s.Name);

                var data = await query.Skip(request.Start).Take(length).ToListAsync();

                return new DataTablesResponse<ResultSkillDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<ResultSkillDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<ResultSkillDto>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<DataTablesResponse<ResultSkillDto>> GetAssignedSkillsForDataTableAsync(DataTablesRequest request, int schoolClassId)
        {
            try
            {
                var query = _context.ClassResultSkills
                    .AsNoTracking()
                    .Where(crs => schoolClassId <= 0 || crs.SchoolClassId == schoolClassId)
                    .Select(crs => new ResultSkillDto
                    {
                        Id = crs.ResultSkill.Id,
                        Name = crs.ResultSkill.Name,
                        ClassName = crs.SchoolClass.Name,
                        Domain = crs.ResultSkill.Domain,
                        DomainName = crs.ResultSkill.Domain.ToString(),
                        DisplayOrder = crs.ResultSkill.DisplayOrder,
                        IsActive = crs.ResultSkill.IsActive
                    })
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(request.Search?.Value))
                {
                    var search = request.Search.Value.ToLower();
                    query = query.Where(s =>
                        s.Name.ToLower().Contains(search) ||
                        s.DomainName.ToLower().Contains(search));
                }

                var recordsFiltered = await query.CountAsync();
                var length = request.Length > 0 ? request.Length : 10;

                query = request.Order != null && request.Order.Any()
                    ? ApplySkillOrder(query, request)
                    : query.OrderBy(s => s.Domain).ThenBy(s => s.DisplayOrder).ThenBy(s => s.Name);

                var data = await query.Skip(request.Start).Take(length).ToListAsync();

                return new DataTablesResponse<ResultSkillDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<ResultSkillDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<ResultSkillDto>(),
                    Error = ex.Message
                };
            }
        }

        public async Task<ApiResponse<ResultSkillDto>> CreateSkillAsync(CreateResultSkillDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return new ApiResponse<ResultSkillDto> { Success = false, Message = "Skill name is required.", Data = null };

                if (dto.DisplayOrder <= 0)
                    return new ApiResponse<ResultSkillDto> { Success = false, Message = "Display order must be greater than 0.", Data = null };

                var duplicate = await _context.ResultSkills
                    .AnyAsync(s => s.Name.Trim().ToLower() == dto.Name.Trim().ToLower() && s.Domain == dto.Domain);

                if (duplicate)
                    return new ApiResponse<ResultSkillDto> { Success = false, Message = "A skill with this name already exists in the selected domain.", Data = null };

                var skill = new ResultSkill
                {
                    Name = dto.Name.Trim(),
                    Domain = dto.Domain,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.ResultSkills.Add(skill);
                await _context.SaveChangesAsync();

                return new ApiResponse<ResultSkillDto>
                {
                    Success = true,
                    Message = "Terminal skill created successfully.",
                    Data = ToDto(skill)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ResultSkillDto> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<ApiResponse<bool>> UpdateSkillAsync(UpdateResultSkillDto dto)
        {
            try
            {
                var skill = await _context.ResultSkills.FindAsync(dto.Id);
                if (skill == null)
                    return new ApiResponse<bool> { Success = false, Message = "Terminal skill not found.", Data = false };

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return new ApiResponse<bool> { Success = false, Message = "Skill name is required.", Data = false };

                if (dto.DisplayOrder <= 0)
                    return new ApiResponse<bool> { Success = false, Message = "Display order must be greater than 0.", Data = false };

                var duplicate = await _context.ResultSkills
                    .AnyAsync(s => s.Id != dto.Id && s.Name.Trim().ToLower() == dto.Name.Trim().ToLower() && s.Domain == dto.Domain);

                if (duplicate)
                    return new ApiResponse<bool> { Success = false, Message = "A skill with this name already exists in the selected domain.", Data = false };

                skill.Name = dto.Name.Trim();
                skill.Domain = dto.Domain;
                skill.DisplayOrder = dto.DisplayOrder;
                skill.IsActive = dto.IsActive;
                skill.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = "Terminal skill updated successfully.", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message, Data = false };
            }
        }

        public async Task<ApiResponse<bool>> ToggleSkillStatusAsync(int id)
        {
            try
            {
                var skill = await _context.ResultSkills.FindAsync(id);
                if (skill == null)
                    return new ApiResponse<bool> { Success = false, Message = "Terminal skill not found.", Data = false };

                skill.IsActive = !skill.IsActive;
                skill.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = skill.IsActive ? "Terminal skill activated." : "Terminal skill deactivated.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message, Data = false };
            }
        }

        public async Task<ApiResponse<bool>> AssignSkillsToClassAsync(int schoolClassId, List<int> resultSkillIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (schoolClassId <= 0)
                    return new ApiResponse<bool> { Success = false, Message = "Class is required.", Data = false };

                if (resultSkillIds == null || resultSkillIds.Count == 0)
                    return new ApiResponse<bool> { Success = false, Message = "Select at least one terminal skill.", Data = false };

                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == schoolClassId);
                if (!classExists)
                    return new ApiResponse<bool> { Success = false, Message = "Class not found.", Data = false };

                var activeSkillIds = await _context.ResultSkills
                    .Where(s => s.IsActive && resultSkillIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                if (activeSkillIds.Count != resultSkillIds.Distinct().Count())
                    return new ApiResponse<bool> { Success = false, Message = "One or more selected skills are inactive or not found.", Data = false };

                var existing = await _context.ClassResultSkills
                    .Where(crs => crs.SchoolClassId == schoolClassId)
                    .ToListAsync();

                _context.ClassResultSkills.RemoveRange(existing);

                var assignments = resultSkillIds
                    .Distinct()
                    .Select(id => new ClassResultSkill
                    {
                        SchoolClassId = schoolClassId,
                        ResultSkillId = id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    })
                    .ToList();

                await _context.ClassResultSkills.AddRangeAsync(assignments);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Terminal skills assigned successfully to class ID {schoolClassId}.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<bool> { Success = false, Message = ex.Message, Data = false };
            }
        }

        public async Task<ApiResponse<bool>> EnsureTerminalSkillRatingsForTermRegistrationAsync(long termRegId)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .FirstOrDefaultAsync(tr => tr.Id == termRegId);

                if (termReg == null)
                    return new ApiResponse<bool> { Success = false, Message = "Term registration not found.", Data = false };

                var assignedSkillIds = await _context.ClassResultSkills
                    .Where(crs => crs.SchoolClassId == termReg.SchoolClassId && crs.IsActive && crs.ResultSkill.IsActive)
                    .OrderBy(crs => crs.ResultSkill.DisplayOrder)
                    .Select(crs => crs.ResultSkillId)
                    .ToListAsync();

                if (!assignedSkillIds.Any())
                    return new ApiResponse<bool> { Success = true, Message = "No terminal skills are assigned to this class.", Data = true };

                var existingRatingIds = await _context.StudentResultSkillRatings
                    .Where(r => r.TermRegId == termRegId)
                    .Select(r => r.ResultSkillId)
                    .ToListAsync();

                if (existingRatingIds.Any())
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Terminal skill ratings already exist for this term registration. No rating changes were made.",
                        Data = true
                    };

                var averageResult = await CalculateAcademicAverageAsync(termRegId, termReg.SchoolClassId);
                if (!averageResult.Success)
                    return new ApiResponse<bool> { Success = false, Message = averageResult.Message, Data = false };

                var (min, max) = GetRatingRange(averageResult.Data);
                var ratings = assignedSkillIds.Select(skillId => new StudentResultSkillRating
                {
                    TermRegId = termRegId,
                    ResultSkillId = skillId,
                    Score = (byte)Random.Shared.Next(min, max + 1),
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                await _context.StudentResultSkillRatings.AddRangeAsync(ratings);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Terminal skill ratings generated successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message, Data = false };
            }
        }

        public async Task<TerminalResultDto?> GetTerminalResultAsync(long termRegId)
        {
            var termReg = await _context.TermRegistrations
                .Include(tr => tr.StudentsTable)
                .Include(tr => tr.SchoolClasses)
                .Include(tr => tr.SubClassTable)
                .Include(tr => tr.SesseionTable)
                .FirstOrDefaultAsync(tr => tr.Id == termRegId);

            if (termReg == null)
                return null;

            var hasSubmittedResults = await _context.ResultTables
                .AnyAsync(rt => rt.TermRegId == termRegId && rt.Status);

            if (hasSubmittedResults)
            {
                await EnsureTerminalSkillRatingsForTermRegistrationAsync(termRegId);
            }

            var configs = await _context.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == termReg.SchoolClassId)
                .OrderBy(ac => ac.DisplayOrder)
                .ToListAsync();

            var results = await _context.ResultTables
                .Include(rt => rt.Subject)
                .Where(rt => rt.TermRegId == termRegId)
                .OrderBy(rt => rt.Subject != null ? rt.Subject.DisplayOrder : 0)
                .ThenBy(rt => rt.Subject != null ? rt.Subject.Name : string.Empty)
                .ToListAsync();

            var ratings = await _context.StudentResultSkillRatings
                .Include(r => r.ResultSkill)
                .Where(r => r.TermRegId == termRegId)
                .ToListAsync();

            var ratingMap = ratings.ToDictionary(r => r.ResultSkillId);
            var academicRows = new List<TerminalResultAcademicRowDto>();
            decimal academicTotal = 0;

            foreach (var result in results)
            {
                var scores = new[] { result.ScoreOne, result.ScoreTwo, result.ScoreThree, result.ScoreFour, result.ScoreFive, result.ScoreSix };
                var total = scores.Sum(score => score ?? 0);
                academicTotal += (decimal)total;

                var (grade, remark) = SD.GetGradeAndRemark((decimal)total);

                academicRows.Add(new TerminalResultAcademicRowDto
                {
                    SubjectName = result.Subject?.Name ?? "Unknown Subject",
                    Assessments = configs.Select((config, index) => new ResultTableAssessmentDto
                    {
                        AssessmentName = config.AssessmentName,
                        AssessmentScore = config.AssessmentScore,
                        StudentScore = index < scores.Length ? scores[index] : null
                    }).ToList(),
                    TotalScore = (decimal)total,
                    Grade = grade,
                    Remark = remark
                });
            }

            var maxPerSubject = configs.Sum(c => c.AssessmentScore);
            var submittedCount = results.Count(rt => rt.Status);
            var academicAverage = submittedCount > 0 && maxPerSubject > 0
                ? Math.Round((decimal)((double)academicTotal / (submittedCount * maxPerSubject) * 100), 2)
                : 0;

            var skillRatings = ratings
                .Where(r => r.ResultSkill != null)
                .Select(r => new StudentResultSkillRatingDto
                {
                    ResultSkillId = r.ResultSkillId,
                    SkillName = r.ResultSkill!.Name,
                    Domain = r.ResultSkill.Domain,
                    DomainName = r.ResultSkill.Domain.ToString(),
                    Score = r.Score,
                    ScoreLabel = GetScoreLabel(r.Score)
                })
                .ToList();

            return new TerminalResultDto
            {
                TermRegId = termReg.Id,
                StudentName = termReg.StudentsTable != null
                    ? $"{termReg.StudentsTable.Surname} {termReg.StudentsTable.FirstName} {termReg.StudentsTable.OtherName}".Trim()
                    : string.Empty,
                AdmissionNumber = termReg.StudentsTable?.AdmissionNumber ?? string.Empty,
                ClassName = termReg.SubClassTable != null
                    ? $"{termReg.SchoolClasses?.Name} - {termReg.SubClassTable.Name}"
                    : termReg.SchoolClasses?.Name ?? string.Empty,
                Session = termReg.SesseionTable?.Name ?? string.Empty,
                Term = termReg.Term.ToString(),
                ResultType = termReg.SchoolClasses?.Resulttype.ToString() ?? string.Empty,
                Attendance = termReg.Attendance,
                AcademicTotal = academicTotal,
                AcademicAverage = academicAverage,
                AcademicRows = academicRows,
                AffectiveSkills = skillRatings
                    .Where(r => r.Domain == ResultSkillDomain.Affective)
                    .OrderBy(r => r.DomainName)
                    .ToList(),
                PsychomotorSkills = skillRatings
                    .Where(r => r.Domain == ResultSkillDomain.Psychomotor)
                    .OrderBy(r => r.DomainName)
                    .ToList(),
                RatingsGenerated = ratings.Any(),
                RatingMessage = ratings.Any()
                    ? "Terminal skill ratings have already been generated."
                    : hasSubmittedResults
                        ? "Terminal skill ratings are pending generation."
                        : "Academic result has not been saved yet."
            };
        }

        private static IQueryable<ResultSkillDto> ApplySkillOrder(IQueryable<ResultSkillDto> query, DataTablesRequest request)
        {
            var order = request.Order.First();
            var column = request.Columns != null && order.Column < request.Columns.Count
                ? request.Columns[order.Column].Data ?? string.Empty
                : string.Empty;
            var dir = order.Dir.ToLower() == "desc" ? -1 : 1;

            return dir > 0 ? column switch
            {
                "name" => query.OrderBy(s => s.Name),
                "domain" => query.OrderBy(s => s.Domain),
                "displayOrder" => query.OrderBy(s => s.DisplayOrder),
                "isActive" => query.OrderBy(s => s.IsActive),
                "createdDate" => query.OrderBy(s => s.CreatedDate),
                _ => query.OrderBy(s => s.Domain).ThenBy(s => s.DisplayOrder).ThenBy(s => s.Name)
            } : column switch
            {
                "name" => query.OrderByDescending(s => s.Name),
                "domain" => query.OrderByDescending(s => s.Domain),
                "displayOrder" => query.OrderByDescending(s => s.DisplayOrder),
                "isActive" => query.OrderByDescending(s => s.IsActive),
                "createdDate" => query.OrderByDescending(s => s.CreatedDate),
                _ => query.OrderByDescending(s => s.Domain).ThenByDescending(s => s.DisplayOrder).ThenByDescending(s => s.Name)
            };
        }

        private async Task<ApiResponse<decimal>> CalculateAcademicAverageAsync(long termRegId, int schoolClassId)
        {
            var configs = await _context.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == schoolClassId)
                .ToListAsync();

            if (!configs.Any())
                return new ApiResponse<decimal> { Success = false, Message = "No assessment configuration found for this class.", Data = 0 };

            var maxPerSubject = configs.Sum(c => c.AssessmentScore);
            if (maxPerSubject <= 0)
                return new ApiResponse<decimal> { Success = false, Message = "Assessment configuration maximum score is invalid.", Data = 0 };

            var results = await _context.ResultTables
                .Where(rt => rt.TermRegId == termRegId && rt.Status)
                .ToListAsync();

            if (!results.Any())
                return new ApiResponse<decimal> { Success = false, Message = "No submitted academic results found for this student.", Data = 0 };

            var totalObtained = results.Sum(rt =>
                (rt.ScoreOne ?? 0) +
                (rt.ScoreTwo ?? 0) +
                (rt.ScoreThree ?? 0) +
                (rt.ScoreFour ?? 0) +
                (rt.ScoreFive ?? 0) +
                (rt.ScoreSix ?? 0));

            var average = totalObtained / (results.Count * maxPerSubject) * 100;
            return new ApiResponse<decimal> { Success = true, Message = "Academic average calculated.", Data = (decimal)Math.Round(average, 2) };
        }

        private static (int Min, int Max) GetRatingRange(decimal average)
        {
            if (average < 45)
                return (1, 4);

            if (average < 70)
                return (2, 5);

            return (3, 5);
        }

        private static string GetScoreLabel(byte score)
        {
            return score switch
            {
                1 => "Poor",
                2 => "Fail",
                3 => "Good",
                4 => "Very Good",
                5 => "Excellent",
                _ => string.Empty
            };
        }

        private static ResultSkillDto ToDto(ResultSkill skill)
        {
            return new ResultSkillDto
            {
                Id = skill.Id,
                Name = skill.Name,
                ClassName = string.Empty,
                Domain = skill.Domain,
                DomainName = skill.Domain.ToString(),
                DisplayOrder = skill.DisplayOrder,
                IsActive = skill.IsActive,
                CreatedDate = skill.CreatedDate,
                UpdatedDate = skill.UpdatedDate
            };
        }
    }
}
