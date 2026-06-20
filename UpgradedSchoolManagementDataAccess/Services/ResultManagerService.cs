using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class ResultManagerService : IResultManagerService
    {
        private readonly ApplicationDbContext _db;
        private readonly IResultSkillService _resultSkillService;

        public ResultManagerService(ApplicationDbContext db, IResultSkillService resultSkillService)
        {
            this._db = db;
            _resultSkillService = resultSkillService;
        }
        public async Task<List<ResultSheetDto>> GetResultSheetAsync(TermObjects model)
        {
            var termRegistrations = await _db.TermRegistrations
                .AsNoTracking()
                .Where(k =>
                    k.Term == model.Term &&
                    k.SessionId == model.SessionId &&
                    k.SchoolClassId == model.schoolClassId &&
                    k.SubClassId == model.SubclassId)
                .Select(k => new ResultSheetDto
                {
                    termRegId = k.Id,
                    name = k.StudentsTable != null ? k.StudentsTable.FullName : string.Empty,
                    regnumber = k.StudentsTable != null ? k.StudentsTable.AdmissionNumber : string.Empty,
                    term = k.Term.ToString(),
                    session = k.SesseionTable != null ? k.SesseionTable.Name : string.Empty,
                    schoolclass = k.SchoolClasses != null ? k.SchoolClasses.Name : string.Empty,
                    subjects = $"{k.ResultTable.Count(r => r.Status)}/{k.ResultTable.Count()}",
                    resultStatus = k.ResultTable.Any() && k.ResultTable.All(r => r.Status),
                    attendance = k.Attendance != null && k.Attendance > 0,
                })
                .ToListAsync();

            return termRegistrations;
        }

        public async Task<EditResultDto?> GetEditResultDataAsync(long termRegId)
        {
            var termReg = await _db.TermRegistrations
                .Include(tr => tr.StudentsTable)
                .Include(tr => tr.SchoolClasses)
                .Include(tr => tr.SubClassTable)
                .Include(tr => tr.SesseionTable)
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.Id == termRegId);

            if (termReg == null) return null;

            var assessmentConfigs = await _db.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == termReg.SchoolClassId)
                .OrderBy(ac => ac.DisplayOrder)
                .ToListAsync();

            var existingResults = await _db.ResultTables
                .Include(rt => rt.Subject)
                .Where(rt => rt.TermRegId == termRegId)
                .AsNoTracking()
                .ToListAsync();

            var existingSubjectIds = existingResults.Select(rt => rt.SubjectId).ToHashSet();

            var allSubjects = await _db.SubjectTables
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();

            var subjects = new List<SubjectResultDto>();

            foreach (var subject in allSubjects)
            {
                if (existingSubjectIds.Contains(subject.Id))
                {
                    var rt = existingResults.First(r => r.SubjectId == subject.Id);
                    var total = (decimal)((rt.ScoreOne ?? 0) + (rt.ScoreTwo ?? 0) + (rt.ScoreThree ?? 0) + (rt.ScoreFour ?? 0) + (rt.ScoreFive ?? 0) + (rt.ScoreSix ?? 0));
                    var (grade, remark) = SD.GetGradeAndRemark(total);
                    subjects.Add(new SubjectResultDto
                    {
                        ResultTableId = rt.Id,
                        SubjectId = rt.SubjectId,
                        SubjectName = rt.Subject?.Name ?? subject.Name,
                        ScoreOne = rt.ScoreOne,
                        ScoreTwo = rt.ScoreTwo,
                        ScoreThree = rt.ScoreThree,
                        ScoreFour = rt.ScoreFour,
                        ScoreFive = rt.ScoreFive,
                        ScoreSix = rt.ScoreSix,
                        TotalScore = total,
                        Grade = grade,
                        Remark = remark
                    });
                }
                else
                {
                    subjects.Add(new SubjectResultDto
                    {
                        ResultTableId = 0,
                        SubjectId = subject.Id,
                        SubjectName = subject.Name,
                        ScoreOne = null,
                        ScoreTwo = null,
                        ScoreThree = null,
                        ScoreFour = null,
                        ScoreFive = null,
                        ScoreSix = null,
                        TotalScore = 0,
                        Grade = "E",
                        Remark = "Poor"
                    });
                }
            }

            return new EditResultDto
            {
                TermRegId = termReg.Id,
                StudentName = termReg.StudentsTable != null
                    ? $"{termReg.StudentsTable.Surname} {termReg.StudentsTable.OtherName} {termReg.StudentsTable.FirstName}".Trim()
                    : string.Empty,
                AdmissionNumber = termReg.StudentsTable?.AdmissionNumber ?? string.Empty,
                ClassName = $"{termReg.SchoolClasses?.Name} - {termReg.SubClassTable?.Name}",
                Term = termReg.Term.ToString(),
                Session = termReg.SesseionTable?.Name ?? "N/A",
                AssessmentConfigs = assessmentConfigs.Select(ac => new AssessmentConfigDto
                {
                    Id = ac.Id,
                    AssessmentName = ac.AssessmentName,
                    AssessmentScore = ac.AssessmentScore,
                    DisplayOrder = ac.DisplayOrder,
                    SchoolClassId = ac.SchoolClassId,
                    CreatedDate = ac.CreatedDate,
                    UpdatedDate = ac.UpdatedDate
                }).ToList(),
                Subjects = subjects
            };
        }

        public async Task<List<AssessmentSheetDto>> GetAssessmentSheetAsync(TermObjects model)
        {
            var termRegistrations = await _db.TermRegistrations
                .Include(tr => tr.StudentsTable)
                .Where(k =>
                    k.Term == model.Term &&
                    k.SessionId == model.SessionId &&
                    k.SchoolClassId == model.schoolClassId &&
                    k.SubClassId == model.SubclassId)
                .AsNoTracking()
                .ToListAsync();

            var allSubjects = await _db.SubjectTables
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();

            var termRegIds = termRegistrations.Select(t => t.Id).ToList();
            var existingResults = await _db.ResultTables
                .Where(rt => termRegIds.Contains(rt.TermRegId))
                .AsNoTracking()
                .ToListAsync();

            var result = new List<AssessmentSheetDto>();
            var sn = 1;

            foreach (var tr in termRegistrations)
            {
                var studentResults = existingResults.Where(r => r.TermRegId == tr.Id).ToList();
                var existingSubjectIds = studentResults.Select(r => r.SubjectId).ToHashSet();

                foreach (var subject in allSubjects)
                {
                    ResultTable? existing = null;
                    if (existingSubjectIds.Contains(subject.Id))
                    {
                        existing = studentResults.First(r => r.SubjectId == subject.Id);
                    }

                    result.Add(new AssessmentSheetDto
                    {
                        Sn = sn++,
                        TermRegId = tr.Id,
                        StudentName = tr.StudentsTable != null
                            ? $"{tr.StudentsTable.Surname} {tr.StudentsTable.OtherName} {tr.StudentsTable.FirstName}".Trim()
                            : "N/A",
                        AdmissionNumber = tr.StudentsTable?.AdmissionNumber ?? "N/A",
                        SubjectId = subject.Id,
                        SubjectName = subject.Name,
                        ScoreOne = existing?.ScoreOne,
                        ScoreTwo = existing?.ScoreTwo,
                        ScoreThree = existing?.ScoreThree,
                        ScoreFour = existing?.ScoreFour,
                        ScoreFive = existing?.ScoreFive,
                        ScoreSix = existing?.ScoreSix
                    });
                }
            }

            return result;
        }

        public async Task<ApiResponse<bool>> SaveResultsAsync(SaveResultDto model)
        {
            try
            {
                var termReg = await _db.TermRegistrations
                    .FirstOrDefaultAsync(tr => tr.Id == model.TermRegId);

                if (termReg == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Term registration not found.",
                        Data = false
                    };
                }

                var configs = await _db.AssessmentConfigurations
                    .Where(ac => ac.SchoolClassId == termReg.SchoolClassId)
                    .OrderBy(ac => ac.DisplayOrder)
                    .ToListAsync();

                foreach (var subject in model.Subjects)
                {
                    var scores = new[] { subject.ScoreOne, subject.ScoreTwo, subject.ScoreThree, subject.ScoreFour, subject.ScoreFive, subject.ScoreSix };

                    for (int i = 0; i < configs.Count && i < scores.Length; i++)
                    {
                        if (scores[i].HasValue && scores[i].Value > configs[i].AssessmentScore)
                        {
                            var subjectName = await _db.SubjectTables
                                .Where(s => s.Id == subject.SubjectId)
                                .Select(s => s.Name)
                                .FirstOrDefaultAsync();

                            return new ApiResponse<bool>
                            {
                                Success = false,
                                Message = $"{configs[i].AssessmentName} score for '{subjectName}' exceeds the maximum of {configs[i].AssessmentScore}.",
                                Data = false
                            };
                        }
                    }

                    var existing = await _db.ResultTables
                        .FirstOrDefaultAsync(rt => rt.TermRegId == model.TermRegId && rt.SubjectId == subject.SubjectId);

                    if (existing != null)
                    {
                        existing.ScoreOne = subject.ScoreOne;
                        existing.ScoreTwo = subject.ScoreTwo;
                        existing.ScoreThree = subject.ScoreThree;
                        existing.ScoreFour = subject.ScoreFour;
                        existing.ScoreFive = subject.ScoreFive;
                        existing.ScoreSix = subject.ScoreSix;
                        existing.Status = true;
                        existing.UpdatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        await _db.ResultTables.AddAsync(new ResultTable
                        {
                            TermRegId = model.TermRegId,
                            SubjectId = subject.SubjectId,
                            ScoreOne = subject.ScoreOne,
                            ScoreTwo = subject.ScoreTwo,
                            ScoreThree = subject.ScoreThree,
                            ScoreFour = subject.ScoreFour,
                            ScoreFive = subject.ScoreFive,
                            ScoreSix = subject.ScoreSix,
                            Status = true,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        });
                    }
                }

                await _db.SaveChangesAsync();

                await _resultSkillService.EnsureTerminalSkillRatingsForTermRegistrationAsync(model.TermRegId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Results saved successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while saving results: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<AssessmentImportResult>> ImportAssessmentScoresAsync(
            int sessionId, Term termValue, int schoolClassId, int subClassId, Stream excelStream)
        {
            var result = new AssessmentImportResult();
            var touchedTermRegIds = new HashSet<long>();

            try
            {
                var configs = await _db.AssessmentConfigurations
                    .Where(ac => ac.SchoolClassId == schoolClassId)
                    .OrderBy(ac => ac.DisplayOrder)
                    .AsNoTracking()
                    .ToListAsync();

                if (configs.Count == 0)
                {
                    return new ApiResponse<AssessmentImportResult>
                    {
                        Success = false,
                        Message = "No assessment configurations found for the selected class.",
                        Data = result
                    };
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var reader = ExcelReaderFactory.CreateReader(excelStream);

                var headers = new List<string>();
                var admissionCol = -1;
                var subjectIdCol = -1;
                var assessmentCols = new List<int>();

                var knownHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "S/N", "SN", "SNO",
                    "Session", "Term", "Class", "Sub Class", "Subclass",
                    "Student Name", "StudentName",
                    "Admission No", "AdmissionNumber",
                    "Subject Id", "SubjectId", "Code",
                    "Subject", "Subjects"
                };

                bool isHeader = true;
                do
                {
                    while (reader.Read())
                    {
                        if (isHeader)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var val = reader.GetValue(i)?.ToString()?.Trim() ?? "";
                                headers.Add(val);

                                if (val.Equals("Admission No", StringComparison.OrdinalIgnoreCase) ||
                                    val.Equals("AdmissionNumber", StringComparison.OrdinalIgnoreCase))
                                    admissionCol = i;
                                else if (val.Equals("Subject Id", StringComparison.OrdinalIgnoreCase) ||
                                         val.Equals("SubjectId", StringComparison.OrdinalIgnoreCase) ||
                                         val.Equals("Code", StringComparison.OrdinalIgnoreCase) ||
                                         val.Equals("Subject ID", StringComparison.OrdinalIgnoreCase))
                                    subjectIdCol = i;
                            }

                            for (int i = 0; i < headers.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(headers[i]) && !knownHeaders.Contains(headers[i]))
                                    assessmentCols.Add(i);
                            }

                            if (admissionCol == -1 || subjectIdCol == -1)
                            {
                                return new ApiResponse<AssessmentImportResult>
                                {
                                    Success = false,
                                    Message = "Required columns 'Admission No' and/or 'Subject Id' not found in the Excel file.",
                                    Data = result
                                };
                            }

                            isHeader = false;
                        }
                        else
                        {
                            var excelRow = result.TotalRows + 2;
                            result.TotalRows++;

                            var admissionNo = reader.GetValue(admissionCol)?.ToString()?.Trim();
                            var subjectIdStr = reader.GetValue(subjectIdCol)?.ToString()?.Trim();

                            if (string.IsNullOrWhiteSpace(admissionNo))
                            {
                                result.FailureCount++;
                                result.Errors.Add($"Row {excelRow}: Admission No is empty.");
                                continue;
                            }

                            if (!int.TryParse(subjectIdStr, out var subjectId) || subjectId <= 0)
                            {
                                result.FailureCount++;
                                result.Errors.Add($"Row {excelRow}: Invalid or missing Subject Id ('{subjectIdStr}').");
                                continue;
                            }

                            var termReg = await _db.TermRegistrations
                                .Include(tr => tr.StudentsTable)
                                .FirstOrDefaultAsync(tr =>
                                    tr.SessionId == sessionId &&
                                    tr.Term == termValue &&
                                    tr.SchoolClassId == schoolClassId &&
                                    tr.SubClassId == subClassId &&
                                    tr.StudentsTable != null &&
                                    tr.StudentsTable.AdmissionNumber == admissionNo);

                            if (termReg == null)
                            {
                                result.FailureCount++;
                                result.Errors.Add($"Row {excelRow}: No term registration found for admission no '{admissionNo}' in the selected session/term/class.");
                                continue;
                            }

                            var scores = new double?[6];
                            var hasAnyScore = false;
                            var validationFailed = false;

                            for (int j = 0; j < assessmentCols.Count && j < 6; j++)
                            {
                                var cellValue = reader.GetValue(assessmentCols[j]);
                                if (cellValue != null)
                                {
                                    var strVal = cellValue.ToString()?.Trim();
                                    if (!string.IsNullOrWhiteSpace(strVal) &&
                                        double.TryParse(strVal, out var parsed))
                                    {
                                        if (j < configs.Count && parsed > configs[j].AssessmentScore)
                                        {
                                            result.FailureCount++;
                                            result.Errors.Add($"Row {excelRow}: '{configs[j].AssessmentName}' score ({parsed}) for admission '{admissionNo}' subject {subjectId} exceeds max ({configs[j].AssessmentScore}).");
                                            validationFailed = true;
                                            break;
                                        }
                                        scores[j] = parsed;
                                        hasAnyScore = true;
                                    }
                                }
                            }

                            if (validationFailed) continue;
                            if (!hasAnyScore) continue;

                            var existing = await _db.ResultTables
                                .FirstOrDefaultAsync(rt => rt.TermRegId == termReg.Id && rt.SubjectId == subjectId);

                            if (existing != null)
                            {
                                existing.ScoreOne = scores[0];
                                existing.ScoreTwo = scores[1];
                                existing.ScoreThree = scores[2];
                                existing.ScoreFour = scores[3];
                                existing.ScoreFive = scores[4];
                                existing.ScoreSix = scores[5];
                                existing.Status = true;
                                existing.UpdatedDate = DateTime.UtcNow;
                            }
                            else
                            {
                                await _db.ResultTables.AddAsync(new ResultTable
                                {
                                    TermRegId = termReg.Id,
                                    SubjectId = subjectId,
                                    ScoreOne = scores[0],
                                    ScoreTwo = scores[1],
                                    ScoreThree = scores[2],
                                    ScoreFour = scores[3],
                                    ScoreFive = scores[4],
                                    ScoreSix = scores[5],
                                    Status = true,
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedDate = DateTime.UtcNow
                                });
                            }

                            result.SuccessCount++;
                            touchedTermRegIds.Add(termReg.Id);
                        }
                    }
                } while (reader.NextResult());

                await _db.SaveChangesAsync();

                if (result.FailureCount == 0)
                {
                    foreach (var termRegId in touchedTermRegIds)
                    {
                        await _resultSkillService.EnsureTerminalSkillRatingsForTermRegistrationAsync(termRegId);
                    }
                }

                return new ApiResponse<AssessmentImportResult>
                {
                    Success = result.FailureCount == 0,
                    Message = $"{result.SuccessCount} row(s) updated successfully." +
                              (result.FailureCount > 0
                                  ? $" {result.FailureCount} row(s) failed. See errors for details."
                                  : ""),
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AssessmentImportResult>
                {
                    Success = false,
                    Message = $"An error occurred while importing: {ex.Message}",
                    Data = result
                };
            }
        }
    }
}
