# Terminal Result Psychomotor/Affective Configuration — Implementation Approach

## 1. Requirement Summary

The current result system can configure academic assessments per class, but Psychomotor and Affective Domain/Skills are not configurable per class type. The required behaviour is:

1. Admin can define Psychomotor and Affective skills from the UI.
2. Admin can assign only the relevant skills to a class.
3. Primary, JSS, and SSS can have different skill options.
4. When a student's result is generated/saved for the first time, the system must use the student's academic average to generate a random score between `1` and `5` for each assigned skill.
5. Score generation rule:
   - Average `< 45` → random score `1–4`
   - Average `>= 45` and `< 70` → random score `2–5`
   - Average `>= 70` → random score `3–5`
6. After the first generation, later save/generate actions must not update existing ratings or create new rating entries for that same term registration.

---

## 2. Current System Findings

### 2.1 Existing class/result type structure

- `SchoolClasses` already has `Resulttype` using `ResultType` enum: `Nursery`, `Primary`, `Jss`, `SSS`.
  - File: `UpgradedSchoolManagementModels/Models/SchoolClasses.cs:15`
- `ResultType` is defined in:
  - File: `UpgradedSchoolManagementModels/Models/ConstantEnums.cs:29`
- Class management currently supports class name, display order, and result type:
  - File: `UpgradedSchoolManagementDataAccess/Services/ClassService.cs:24`

### 2.2 Existing academic assessment configuration

- Academic assessment configurations are already class-specific through `AssessmentConfiguration.SchoolClassId`.
  - File: `UpgradedSchoolManagementModels/Models/AssessmentConfiguration.cs:30`
- The Result Configuration page currently only manages academic assessment rows.
  - File: `UpgradedSchoolManagementWeb/Pages/Admin/Academic/result-config/index.cshtml:38`
- The page saves assessment configurations through `ClassService.SaveAssessmentConfigs()`.
  - File: `UpgradedSchoolManagementDataAccess/Services/ClassService.cs:330`

### 2.3 Existing term registration and result save flow

- `TermRegistration` links a student to session, term, class, subclass, academic results, and one fixed `StudentRatings` navigation.
  - File: `UpgradedSchoolManagementModels/Models/TermRegistration.cs:6`
  - File: `UpgradedSchoolManagementModels/Models/TermRegistration.cs:27`
- `ResultManagerService.SaveResultsAsync()` creates or updates `ResultTable` rows only.
  - File: `UpgradedSchoolManagementDataAccess/Services/ResultManagerService.cs:217`
- `ResultManagerService.ImportAssessmentScoresAsync()` imports subject assessment scores and creates/updates `ResultTable` rows only.
  - File: `UpgradedSchoolManagementDataAccess/Services/ResultManagerService.cs:314`
- There is currently no logic that generates Psychomotor/Affective ratings from academic average.

### 2.4 Existing fixed rating model

- `StudentRating` has fixed properties such as `Attentiveness`, `Punctuality`, `Handwriting`, etc.
  - File: `UpgradedSchoolManagementModels/Models/StudentRating.cs:10`
- This model is not suitable for configurable class-based skills because:
  - It has a fixed column structure.
  - It cannot easily support different skill lists for Primary, JSS, and SSS.
  - It cannot support adding/removing skills without schema changes.
  - It does not directly store which configured skill item generated the rating.

### 2.5 Existing terminal result pages

- Terminal result pages are static Razor pages for Nursery, Primary, Junior, and Senior.
  - File: `UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/nursery.cshtml:1`
  - File: `UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/primary.cshtml:1`
  - File: `UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/junior-result.cshtml:1`
  - File: `UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/senior-result.cshtml:1`
- These pages currently hard-code Psychomotor/Affective rows.
  - Example: `primary.cshtml:273`
  - Example: `primary.cshtml:286`
- They are not currently reading terminal skill ratings from the database.

---

## 3. Recommended Domain Design

Keep the existing fixed `StudentRating` table for backward compatibility, but stop using it for new configurable terminal skill ratings. Add new configurable tables.

### 3.1 New entity: `ResultSkill`

Represents the reusable skill catalog.

```csharp
public class ResultSkill
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ResultSkillDomain Domain { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
```

Suggested enum:

```csharp
public enum ResultSkillDomain
{
    Affective = 1,
    Psychomotor = 2
}
```

Purpose:

- Stores all possible skills once.
- Allows Admin to create, edit, deactivate, and order skills.
- Supports different skill names for Primary, JSS, and SSS.

### 3.2 New entity: `ClassResultSkill`

Represents the assignment of skills to a class.

```csharp
public class ClassResultSkill
{
    public int Id { get; set; }
    public int SchoolClassId { get; set; }
    public int ResultSkillId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    public SchoolClasses SchoolClass { get; set; }
    public ResultSkill ResultSkill { get; set; }
}
```

Purpose:

- Defines which skills are available for each class.
- Allows Primary, JSS, and SSS to have different skill lists.
- Should have a unique index on `(SchoolClassId, ResultSkillId)`.

### 3.3 New entity: `StudentResultSkillRating`

Stores generated ratings for a student in a specific term registration.

```csharp
public class StudentResultSkillRating
{
    public int Id { get; set; }
    public long TermRegId { get; set; }
    public int ResultSkillId { get; set; }
    public byte Score { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    public TermRegistration TermRegistration { get; set; }
    public ResultSkill ResultSkill { get; set; }
}
```

Purpose:

- Stores one generated score per assigned skill.
- Prevents later updates or duplicate creation after first generation.
- Should have a unique index on `(TermRegId, ResultSkillId)`.

### 3.4 DbContext changes

Add DbSets in `ApplicationDbContext`:

```csharp
public DbSet<ResultSkill> ResultSkills { get; set; }
public DbSet<ClassResultSkill> ClassResultSkills { get; set; }
public DbSet<StudentResultSkillRating> StudentResultSkillRatings { get; set; }
```

Add relationships and indexes in `OnModelCreating()`:

- `ClassResultSkill` → `SchoolClasses` with `Restrict` delete.
- `ClassResultSkill` → `ResultSkill` with `Restrict` delete.
- `StudentResultSkillRating` → `TermRegistration` with `Cascade` delete.
- `StudentResultSkillRating` → `ResultSkill` with `Restrict` delete.
- Unique index on `ClassResultSkill(SchoolClassId, ResultSkillId)`.
- Unique index on `StudentResultSkillRating(TermRegId, ResultSkillId)`.

### 3.5 Migration

Create and apply a new EF Core migration, for example:

```powershell
dotnet ef migrations add AddTerminalResultSkillConfiguration
dotnet ef database update
```

Important: `Program.cs` currently calls `context.Database.EnsureCreatedAsync()` at line 101. For migration-based environments, use `context.Database.Migrate()` instead of `EnsureCreatedAsync()` to ensure new tables are created in deployed databases.

---

## 4. UI Implementation Approach

### 4.1 Extend Result Configuration page

The existing page is the best place to add terminal skill configuration because it already manages class-based result configuration.

File:

- `UpgradedSchoolManagementWeb/Pages/Admin/Academic/result-config/index.cshtml`

Add tabs:

1. **Academic Assessment Config**
   - Keep current behaviour.
2. **Terminal Skills Catalog**
   - Add/edit/deactivate Psychomotor and Affective skills.
3. **Assign Skills to Class**
   - Select a class and choose available skills for that class.

### 4.2 Terminal Skills Catalog UI

Fields:

- Skill Name
- Domain
  - Affective
  - Psychomotor
- Display Order
- Active status

Example skills:

#### Affective

- Punctuality
- Attendance
- Reliability
- Neatness
- Politeness
- Honesty
- Responsibility

#### Psychomotor

- Handwriting
- Drawing / Painting
- Sports / Games
- Music
- Tool Handling
- Construction

### 4.3 Assign Skills to Class UI

Fields:

- Class dropdown
- Checkboxes grouped by domain

Behaviour:

- Selecting Primary should show only Primary-relevant options.
- Selecting JSS should show only JSS-relevant options.
- Selecting SSS should show only SSS-relevant options.
- Saving should replace the previous assignment for that class.

### 4.4 Terminal result page UI

Current terminal result pages are static. Replace or refactor them into a dynamic result renderer.

Recommended options:

#### Option A — Single generic terminal result page

Create one page:

```text
/result-manager/terminal-result/{termRegId}
```

The page loads:

- Student information
- Class/session/term
- Academic result table
- Configured Affective skills for that student's class
- Configured Psychomotor skills for that student's class
- Generated ratings for that term registration

Use the class `Resulttype` to choose a layout partial:

- Nursery layout
- Primary layout
- Junior layout
- Senior layout

#### Option B — Keep separate pages but make them dynamic

Keep:

- `nursery.cshtml`
- `primary.cshtml`
- `junior-result.cshtml`
- `senior-result.cshtml`

But each page model should load data from the database instead of hard-coding names and scores.

Recommended: Option A, because it avoids duplicated logic and makes class-based configuration easier to maintain.

---

## 5. Rating Generation Logic

### 5.1 Where to generate ratings

The generation should be triggered from the same places where academic results are saved:

1. Manual result save:
   - `ResultManagerService.SaveResultsAsync()`
   - File: `UpgradedSchoolManagementDataAccess/Services/ResultManagerService.cs:217`
2. Excel result import:
   - `ResultManagerService.ImportAssessmentScoresAsync()`
   - File: `UpgradedSchoolManagementDataAccess/Services/ResultManagerService.cs:314`

This satisfies the requirement that ratings are generated when the result is generated/saved for the first time.

### 5.2 Idempotent generation rule

The generation service must be idempotent.

Pseudo-flow:

```text
EnsureTerminalSkillRatingsForTermRegistrationAsync(termRegId)
1. Load TermRegistration.
2. Load active skills assigned to TermRegistration.SchoolClassId.
3. If no assigned skills exist, return.
4. If StudentResultSkillRating already exists for this TermRegId, return without creating/updating anything.
5. Calculate student academic average.
6. If average cannot be calculated, return without creating ratings.
7. Determine random score range from average.
8. Create one StudentResultSkillRating row for each assigned skill.
9. Save once.
```

This directly enforces:

> Subsequent save/generate actions must not update or create new entries.

### 5.3 Academic average calculation

Use the class assessment configuration to calculate the student's academic average percentage.

Recommended formula:

```text
Maximum score per subject = sum of active AssessmentConfiguration.AssessmentScore for the class

Total obtained = sum of all saved scores across submitted subjects

Total possible = submitted subject count × maximum score per subject

Average = Total obtained / Total possible × 100
```

Implementation details:

- Use only `ResultTable` rows with `Status == true`.
- Use only non-null scores.
- If no submitted subject exists, do not generate ratings.
- If class assessment configuration is missing, return an error/warning.
- If maximum score per subject is `0`, return an error/warning.

This is better than simple subject average because the system already supports configurable assessment components and max scores.

### 5.4 Random score range

Use this exact range mapping:

```csharp
if (average < 45)
    range = (1, 4);
else if (average < 70)
    range = (2, 5);
else
    range = (3, 5);
```

Recommended boundary handling:

- `< 45` → `1–4`
- `45 <= average < 70` → `2–5`
- `average >= 70` → `3–5`

This avoids a gap at exactly `70`.

### 5.5 Random number generation

Use inclusive random generation:

```csharp
var score = (byte)Random.Shared.Next(min, max + 1);
```

Each assigned skill receives its own generated score.

### 5.6 Rating label mapping

Use the existing terminal result wording:

| Score | Label |
|---:|---|
| 1 | Poor |
| 2 | Fail |
| 3 | Good |
| 4 | Very Good |
| 5 | Excellent |

The terminal result page should display the saved numeric score and optionally the label.

---

## 6. Service Layer Design

### 6.1 New service interface

Create:

```text
UpgradedSchoolManagementDataAccess/IServices/IResultSkillService.cs
```

Suggested methods:

```csharp
Task<List<ResultSkillDto>> GetActiveSkillsAsync();

Task<List<ResultSkillDto>> GetAssignedSkillsByClassIdAsync(int schoolClassId);

Task<ApiResponse<ResultSkillDto>> CreateSkillAsync(CreateResultSkillDto dto);

Task<ApiResponse<bool>> UpdateSkillAsync(UpdateResultSkillDto dto);

Task<ApiResponse<bool>> ToggleSkillStatusAsync(int id);

Task<ApiResponse<bool>> AssignSkillsToClassAsync(int schoolClassId, List<int> resultSkillIds);

Task<ApiResponse<bool>> EnsureTerminalSkillRatingsForTermRegistrationAsync(long termRegId);
```

### 6.2 Service implementation

Create:

```text
UpgradedSchoolManagementDataAccess/Services/ResultSkillService.cs
```

Responsibilities:

- Skill catalog CRUD.
- Class-skill assignment.
- Idempotent rating generation.
- Average calculation.
- Random score generation.

### 6.3 DTOs

Create in `UpgradedSchoolManagementModels/DTOs`:

```text
ResultSkillDto.cs
CreateResultSkillDto.cs
UpdateResultSkillDto.cs
AssignSkillsToClassDto.cs
StudentResultSkillRatingDto.cs
TerminalResultDto.cs
```

Suggested `StudentResultSkillRatingDto`:

```csharp
public class StudentResultSkillRatingDto
{
    public int ResultSkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public ResultSkillDomain Domain { get; set; }
    public byte Score { get; set; }
    public string ScoreLabel { get; set; } = string.Empty;
}
```

### 6.4 Register DI

Update `Program.cs`:

```csharp
builder.Services.AddScoped<IResultSkillService, ResultSkillService>();
```

Update `IUnitOfWork` and `UnitOfWork` if the UI uses the unit-of-work pattern consistently.

---

## 7. Save Flow Changes

### 7.1 Manual save

Modify `ResultManagerService.SaveResultsAsync()`:

Current behaviour:

- Validates scores.
- Creates/updates `ResultTable`.
- Saves changes.

New behaviour:

```text
1. Validate scores.
2. Create/update ResultTable rows.
3. SaveChangesAsync().
4. Call EnsureTerminalSkillRatingsForTermRegistrationAsync(model.TermRegId).
5. Return success message.
```

Important: call the rating generation after subject scores are saved, because the average depends on saved academic scores.

### 7.2 Excel import

Modify `ImportAssessmentScoresAsync()`:

Current behaviour:

- Reads Excel rows.
- Creates/updates `ResultTable` rows.
- Saves changes once.

New behaviour:

```text
1. Process Excel rows.
2. Create/update ResultTable rows.
3. SaveChangesAsync().
4. For every touched TermRegId, call EnsureTerminalSkillRatingsForTermRegistrationAsync(termRegId).
5. Return import result.
```

Because the generation service is idempotent, calling it for each touched registration is safe.

### 7.3 Subsequent saves

On later manual saves or imports:

- Subject scores may still be updated.
- Terminal skill ratings must not be updated.
- New ratings must not be created.
- The service should return a message such as:
  - `Terminal skill ratings already exist for this term registration. No rating changes were made.`

---

## 8. Terminal Result Rendering Flow

### 8.1 Build terminal result view model

Create a service method or extend result manager:

```csharp
Task<TerminalResultDto?> GetTerminalResultAsync(long termRegId);
```

It should load:

- Term registration
- Student
- Class
- Session
- Term
- Result tables
- Assessment configurations
- Assigned skills for the class
- Existing `StudentResultSkillRating` rows
- Optional attendance and class position if already available

### 8.2 Display Affective and Psychomotor sections

The terminal result page should not hard-code rows. It should render from the assigned skills:

```text
Affective Domain:
- For each assigned skill where Domain == Affective:
  - Show skill name
  - Show saved score
  - Show score label

Psychomotor Skills:
- For each assigned skill where Domain == Psychomotor:
  - Show skill name
  - Show saved score
  - Show score label
```

### 8.3 Missing rating handling

If terminal skill ratings are missing:

- If academic results are not saved yet:
  - Show message: `Academic result has not been saved yet.`
- If academic results are saved but ratings were not generated:
  - Trigger `EnsureTerminalSkillRatingsForTermRegistrationAsync(termRegId)` before rendering.
- If class has no assigned skills:
  - Show message: `No terminal skills have been assigned to this class.`

---

## 9. Validation and Business Rules

### 9.1 Skill validation

- Skill name is required.
- Domain is required.
- Display order must be greater than `0`.
- Skill name should be unique per domain.
- Inactive skills should not be assigned to new classes.

### 9.2 Assignment validation

- Class must exist.
- At least one skill must be selected.
- Duplicate assignment for the same class and skill should be ignored or rejected.
- Inactive skills should be removed from class assignment or blocked.

### 9.3 Rating validation

- Score must be between `1` and `5`.
- Rating must be linked to an existing term registration.
- Rating must be linked to an existing active result skill.
- Rating must not already exist for the same `(TermRegId, ResultSkillId)`.

### 9.4 Deletion rules

- Do not delete a skill if it is assigned to a class.
- Do not delete a class skill assignment if ratings already exist for students.
- If deleting a term registration, block deletion if terminal skill ratings exist, similar to the existing block for recorded academic results.

---

## 10. Files to Create or Modify

### 10.1 Models

Create:

```text
UpgradedSchoolManagementModels/Models/ResultSkill.cs
UpgradedSchoolManagementModels/Models/ClassResultSkill.cs
UpgradedSchoolManagementModels/Models/StudentResultSkillRating.cs
```

Modify:

```text
UpgradedSchoolManagementModels/Models/ConstantEnums.cs
```

Add enum:

```csharp
public enum ResultSkillDomain
{
    Affective = 1,
    Psychomotor = 2
}
```

### 10.2 DTOs

Create:

```text
UpgradedSchoolManagementModels/DTOs/ResultSkillDto.cs
UpgradedSchoolManagementModels/DTOs/CreateResultSkillDto.cs
UpgradedSchoolManagementModels/DTOs/UpdateResultSkillDto.cs
UpgradedSchoolManagementModels/DTOs/AssignSkillsToClassDto.cs
UpgradedSchoolManagementModels/DTOs/StudentResultSkillRatingDto.cs
UpgradedSchoolManagementModels/DTOs/TerminalResultDto.cs
```

### 10.3 DataAccess

Create:

```text
UpgradedSchoolManagementDataAccess/IServices/IResultSkillService.cs
UpgradedSchoolManagementDataAccess/Services/ResultSkillService.cs
```

Modify:

```text
UpgradedSchoolManagementDataAccess/Data/ApplicationDbContext.cs
UpgradedSchoolManagementDataAccess/Services/ResultManagerService.cs
UpgradedSchoolManagementDataAccess/Services/UnitOfWork.cs
UpgradedSchoolManagementDataAccess/IServices/IUnitOfWork.cs
```

### 10.4 Web

Modify:

```text
UpgradedSchoolManagementWeb/Program.cs
UpgradedSchoolManagementWeb/Pages/Admin/Academic/result-config/index.cshtml
UpgradedSchoolManagementWeb/Pages/Admin/Academic/result-config/index.cshtml.cs
UpgradedSchoolManagementWeb/Pages/result-manager/index.cshtml
UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/*
```

Recommended new page:

```text
UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/detail.cshtml
UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/detail.cshtml.cs
```

Or one generic page:

```text
UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/index.cshtml
UpgradedSchoolManagementWeb/Pages/result-manager/terminal-result/index.cshtml.cs
```

### 10.5 Migration

Create EF migration:

```text
UpgradedSchoolManagementDataAccess/Migrations/*_AddTerminalResultSkillConfiguration.cs
```

---

## 11. Suggested Implementation Order

### Phase 1 — Database/domain

1. Add `ResultSkillDomain` enum.
2. Add new entities.
3. Add DbSets and relationships to `ApplicationDbContext`.
4. Add migration.
5. Apply migration.

### Phase 2 — Services

1. Add `IResultSkillService`.
2. Add `ResultSkillService`.
3. Implement skill catalog CRUD.
4. Implement class assignment.
5. Implement idempotent rating generation.
6. Implement terminal result view-model builder.

### Phase 3 — Wire into result save

1. Modify `SaveResultsAsync()` to call rating generation after saving academic scores.
2. Modify `ImportAssessmentScoresAsync()` to call rating generation for touched term registrations.
3. Ensure no rating updates happen when ratings already exist.

### Phase 4 — Admin UI

1. Extend Result Configuration page with tabs.
2. Add Terminal Skills Catalog modal/table.
3. Add Assign Skills to Class modal/table.
4. Add AJAX endpoints for skill CRUD and assignment.
5. Validate selected skills and class before saving.

### Phase 5 — Terminal result UI

1. Create dynamic terminal result page.
2. Load academic results from database.
3. Load assigned Affective/Psychomotor skills from database.
4. Load generated ratings from database.
5. Replace hard-coded rows in terminal result templates.
6. Use class `Resulttype` to select the correct visual layout.

### Phase 6 — Testing

1. Manual tests for Primary, JSS, and SSS.
2. Excel import tests.
3. Subsequent save tests to confirm no rating update/create.
4. Class assignment tests.
5. Terminal result rendering tests.

---

## 12. Test Cases

### 12.1 Skill catalog

| Test | Expected Result |
|---|---|
| Create Affective skill | Skill appears in catalog |
| Create Psychomotor skill | Skill appears in catalog |
| Deactivate skill | Skill no longer available for assignment |
| Duplicate skill name in same domain | Validation error |

### 12.2 Class assignment

| Test | Expected Result |
|---|---|
| Assign skills to Primary | Only Primary gets those skills |
| Assign different skills to JSS | JSS has different terminal result skills |
| Assign skills to SSS | SSS has different terminal result skills |
| Save assignment again | Existing assignment is replaced safely |

### 12.3 Rating generation

| Average | Expected Random Range |
|---:|---|
| 44.9 | 1–4 |
| 45 | 2–5 |
| 69.9 | 2–5 |
| 70 | 3–5 |
| 100 | 3–5 |

### 12.4 Idempotency

| Scenario | Expected Result |
|---|---|
| Save result first time | Ratings are created |
| Save result second time | Existing ratings are not updated |
| Import result after ratings exist | No new ratings are created |
| Terminal result opened multiple times | Same saved ratings are displayed |

### 12.5 Terminal result rendering

| Scenario | Expected Result |
|---|---|
| Primary student | Shows Primary assigned Affective/Psychomotor skills |
| JSS student | Shows JSS assigned Affective/Psychomotor skills |
| SSS student | Shows SSS assigned Affective/Psychomotor skills |
| No ratings yet | Shows pending/not generated message |
| No skills assigned to class | Shows no assigned skills message |

---

## 13. Important Notes

- Do not reuse the fixed `StudentRating` table for this feature. It cannot support configurable class-based skills.
- Do not generate ratings before academic scores are saved, because the average is required.
- Do not update ratings on subsequent saves. Use the unique `(TermRegId, ResultSkillId)` rule to enforce this.
- Do not hard-code terminal result skill rows in Razor pages. They must come from class assignment.
- If the school later needs admin correction of terminal ratings, add a separate protected override workflow with audit logging instead of changing the default save flow.
