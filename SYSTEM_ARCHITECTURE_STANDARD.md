# System Architecture & Standards (Master Guide)

This document establishes the definitive architecture and development standards for the UpgradedSchoolManagement system. All new features and modules must strictly follow these patterns.

---

## Table of Contents
1. Project Structure
2. Design Patterns
3. Naming Conventions
4. File Organization
5. Data Flow Architecture
6. Development Workflow
7. Common Patterns
8. Anti-Patterns to Avoid

---

## 1. Project Structure

### Overview
```
UpgradedSchoolManagement/
├── UpgradedSchoolManagementModels/          (Data models and DTOs)
│   ├── Models/                              (EF Core entities)
│   ├── DTOs/                                (Data Transfer Objects)
│   ├── ViewModels/                          (Razor Page models)
│   ├── ConstraintModels/                    (Enums, constants)
│   └── *.csproj
├── UpgradedSchoolManagementDataAccess/      (Database & services)
│   ├── Data/                                (DbContext)
│   ├── IServices/                           (Service interfaces)
│   ├── Services/                            (Service implementations)
│   ├── Migrations/                          (EF migrations)
│   └── *.csproj
├── UpgradedSchoolManagementUltitlities/     (Shared utilities)
│   ├── SD.cs                                (Static helpers)
│   ├── Constants.cs                         (Global constants)
│   └── *.csproj
└── UpgradedSchoolManagementWeb/             (Presentation layer)
    ├── Controllers/
    │   ├── HomeController.cs                (DataTable endpoints)
    │   └── v1Controller.cs                  (CRUD/action endpoints)
    ├── Pages/                               (Razor Pages)
    │   ├── Students/
    │   ├── Parents/
    │   ├── Classes/
    │   └── ...
    └── *.csproj
```

### Project Responsibility Matrix

| Project | Responsibility | Contains |
|---------|-----------------|----------|
| **Models** | Data shapes and contracts | Entities, DTOs, ViewModels, Enums |
| **DataAccess** | Database & business logic | DbContext, Services, Interfaces, Migrations |
| **Utilities** | Reusable helpers | Static utility functions, constants |
| **Web** | HTTP/UI layer | Controllers, Razor Pages, API routes |

---

## 2. Design Patterns

### 2.1 Dependency Injection (DI)
- **Pattern**: Constructor injection for all services
- **Scope**: `AddScoped<IService, Implementation>()`
- **Location**: `Program.cs` in Web project

**Example**:
```csharp
public class ParentController
{
    private readonly IParentService _parentService;

    public ParentController(IParentService parentService)
    {
        _parentService = parentService;
    }
}
```

### 2.2 Service Layer Pattern
- **Interface**: Defines contract in `IServices` folder
- **Implementation**: Concrete class in `Services` folder
- **Injection**: Always inject interface, not implementation
- **Database Access**: Only through DbContext in service

**Structure**:
```
IServices/IFeatureService.cs (interface)
Services/FeatureService.cs (implementation)
```

### 2.3 Controller Responsibility Segregation
- **HomeController**: DataTables server-side endpoints only
- **v1Controller**: CRUD, get by ID, actions, links, toggles

**Example**:
```csharp
// HomeController - DataTable endpoint
[HttpPost("features-datatable")]
public async Task<IActionResult> GetFeaturesDatatable([FromBody] DataTableRequest request)

// v1Controller - CRUD operations
[HttpPost("features/create")]
public async Task<IActionResult> CreateFeature([FromBody] CreateFeatureDto dto)

[HttpDelete("features/{id}")]
public async Task<IActionResult> DeleteFeature(int id)
```

### 2.4 DTO and ViewModel Separation
- **DTOs** (Models/DTOs): Service↔Controller communication
- **ViewModels** (Models/ViewModels): Controller↔Razor Page communication
- **Entities** (Models/Models): Database representation only

**Flow**:
```
Database → Entity → DTO → ViewModel → Razor Page
```

### 2.5 Generic Result Pattern
Use consistent result DTOs for all operations:

```csharp
public class OperationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

### 2.6 Soft Delete / Audit Trail
- Always include `CreatedAt` and `UpdatedAt` timestamps
- Consider `IsDeleted` flag for soft deletes
- Log significant operations for audit trails

---

## 3. Naming Conventions

### 3.1 Files and Classes
- **Entities**: Singular name + "Table" (e.g., `StudentTable.cs`, `ParentGuardian.cs`)
- **Services**: Feature name + "Service" (e.g., `ParentGuardianService.cs`)
- **Interfaces**: "I" + Feature name + "Service" (e.g., `IParentGuardianService.cs`)
- **DTOs**: Feature name + "Dto" (e.g., `ParentGuardianCreateDto.cs`)
- **ViewModels**: Feature name + "ViewModel" (e.g., `StudentListViewModel.cs`)
- **Controllers**: Feature name + "Controller" (e.g., `v1Controller.cs`)

### 3.2 Properties
- **PascalCase** for all properties
- **Required** properties marked with `[Required]`
- **Foreign keys**: EntityName + "Id" (e.g., `StudentId`, `ParentGuardianId`)
- **Navigation properties**: Plural for collections (e.g., `StudentLinks`)

### 3.3 Methods
- **Async methods**: Suffix with "Async" (e.g., `CreateAsync`, `GetByIdAsync`)
- **Query methods**: Start with "Get" (e.g., `GetById`, `GetByEmail`)
- **Action methods**: Verb + noun (e.g., `CreateStudent`, `UpdateParent`)
- **Boolean methods**: Start with "Is" or "Can" (e.g., `IsValid`, `CanDelete`)

### 3.4 Endpoints
- **POST** for create: `/v1/features/create` or `/v1/features`
- **GET** for retrieve: `/v1/features/{id}`
- **PUT/PATCH** for update: `/v1/features/{id}`
- **DELETE** for remove: `/v1/features/{id}`
- **Custom action**: `/v1/features/{id}/action-name`
- **DataTable**: `POST /home/features-datatable`

---

## 4. File Organization

### 4.1 Entity File Structure
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpgradedSchoolManagementModels.Models
{
    public class FeatureEntity
    {
        // Properties (organized: key, required, optional)
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        public string? Description { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<RelatedEntity> Related { get; set; }
    }
}
```

### 4.2 Service File Structure
```csharp
namespace UpgradedSchoolManagementDataAccess.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly ApplicationDbContext _context;

        public FeatureService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Implement interface methods
        public async Task<FeatureDto?> GetByIdAsync(int id) { }
        public async Task<IList<FeatureDto>> GetAllAsync() { }
        // ...
    }
}
```

### 4.3 Controller File Structure
```csharp
[ApiController]
[Route("api/[controller]")]
public class FeatureController : ControllerBase
{
    private readonly IFeatureService _service;

    public FeatureController(IFeatureService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureDto dto) { }
}
```

---

## 5. Data Flow Architecture

### 5.1 Request Flow (Create Operation)
```
1. Client → Razor Page form
2. Razor Page → POST /v1/features/create (JSON)
3. v1Controller → Service.CreateAsync(dto)
4. Service → Validate + normalize data
5. Service → DbContext.Add() + SaveChanges()
6. DbContext → Database INSERT
7. Service → Return OperationResultDto
8. Controller → HTTP 201 Created
9. Client → Show success message
```

### 5.2 Validation Flow
```
1. DTO validation (DataAnnotations)
2. Controller ModelState validation
3. Service business logic validation
4. Database constraint validation
5. Return appropriate HTTP status code
```

### 5.3 Error Handling
```csharp
try
{
    var result = await _service.CreateAsync(dto);
    if (!result.Success)
        return BadRequest(result); // 400
    return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result); // 201
}
catch (DbUpdateException ex)
{
    return Conflict(new { error = "Duplicate key or constraint violation" }); // 409
}
catch (Exception ex)
{
    return StatusCode(500, new { error = "Internal server error" }); // 500
}
```

---

## 6. Development Workflow

### 6.1 Creating a New Module (Step-by-Step)

#### Step 1: Create Entity
- File: `Models/FeatureName.cs`
- Include: Id, required fields, timestamps, navigation properties
- Constraints: [Required], [StringLength], [EmailAddress], etc.

#### Step 2: Create DTOs
- Files: `DTOs/FeatureNameCreateDto.cs`, `FeatureNameDto.cs`, `FeatureNameUpdateDto.cs`
- Exclude: Navigation properties, timestamps (or include as reference only)
- Format: Input DTOs for create/update, output DTOs for retrieval

#### Step 3: Create Service Interface
- File: `IServices/IFeatureService.cs`
- Methods: Get, Create, Update, Delete, Search, Filter
- Pattern: All methods async returning OperationResultDto or specific Dto

#### Step 4: Create Service Implementation
- File: `Services/FeatureService.cs`
- Implement: All interface methods
- Validation: Business logic validation (uniqueness, dependencies, etc.)

#### Step 5: Update DbContext
- Location: `Data/ApplicationDbContext.cs`
- Add: `public DbSet<FeatureName> FeatureNames { get; set; }`
- Configure: Foreign keys, indexes, constraints in OnModelCreating

#### Step 6: Register Service
- File: `Program.cs`
- Add: `builder.Services.AddScoped<IFeatureService, FeatureService>();`

#### Step 7: Create Migration
- Command: `dotnet ef migrations add AddFeatureModule`
- Verify: Correct table structure, constraints, indexes
- Apply: `dotnet ef database update`

#### Step 8: Create Controllers
- HomeController: DataTable endpoint
- v1Controller: CRUD endpoints

#### Step 9: Create Razor Pages
- Index.cshtml: List with DataTable
- Create.cshtml: Form for creating
- Edit.cshtml: Form for editing
- Details.cshtml: View details

#### Step 10: Unit Tests
- Test all service methods
- Test validation logic
- Test error cases

#### Step 11: Integration Tests
- Test all endpoints
- Test with real data

---

## 7. Common Patterns

### 7.1 Phone Number (Like Parent Module)
```csharp
// Normalize
string normalized = SD.NormalizePhone(input); // Remove non-digits

// Validate
if (!SD.IsValidPhone(normalized))
    return BadRequest("Invalid phone");

// Format for display
string display = SD.FormatPhoneForDisplay(normalized);
```

### 7.2 Unique Constraint with Conflict Handling
```csharp
// Check for existing
var existing = await _context.Features
    .FirstOrDefaultAsync(f => f.Email == normalizedEmail);

if (existing != null)
    return Conflict(new { error = "Email already exists", existingId = existing.Id });

// Create new
var feature = new Feature { Email = normalizedEmail };
```

### 7.3 Soft Delete
```csharp
// Entity
public bool IsDeleted { get; set; } = false;

// Service query
var features = await _context.Features
    .Where(f => !f.IsDeleted)
    .ToListAsync();

// Delete (soft)
feature.IsDeleted = true;
feature.UpdatedAt = DateTime.UtcNow;
_context.Update(feature);
```

### 7.4 Primary/Default Selection
```csharp
// Entity
public bool IsPrimary { get; set; } = false;

// Service update
var currentPrimary = await _context.Features
    .FirstOrDefaultAsync(f => f.ParentId == parentId && f.IsPrimary);

if (currentPrimary != null)
    currentPrimary.IsPrimary = false;

newFeature.IsPrimary = true;
```

### 7.5 Bulk Operations
```csharp
// Create multiple
var items = dtoList.Select(dto => new Feature { ... }).ToList();
_context.Features.AddRange(items);
await _context.SaveChangesAsync();

// Update multiple
foreach (var item in itemsToUpdate)
{
    item.UpdatedAt = DateTime.UtcNow;
    _context.Update(item);
}
```

---

## 8. Anti-Patterns to Avoid

### ❌ Direct Database Access in Controllers
```csharp
// WRONG
public class StudentController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    [HttpGet("{id}")]
    public IActionResult GetStudent(int id)
    {
        var student = _context.StudentsTables.FirstOrDefault(s => s.Id == id);
        return Ok(student);
    }
}
```

### ✅ Use Service Layer
```csharp
public class StudentController : ControllerBase
{
    private readonly IStudentService _service;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var student = await _service.GetByIdAsync(id);
        return Ok(student);
    }
}
```

### ❌ Multiple Responsibility Controllers
```csharp
// WRONG - DataTable AND CRUD in HomeController
public class HomeController : ControllerBase
{
    [HttpPost("students-datatable")]
    public async Task<IActionResult> GetStudentsDatatable([FromBody] DataTableRequest request) { }

    [HttpPost("students")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto) { }
}
```

### ✅ Segregated Responsibilities
```csharp
// HomeController - DataTables only
[HttpPost("students-datatable")]
public async Task<IActionResult> GetStudentsDatatable([FromBody] DataTableRequest request) { }

// v1Controller - CRUD operations
[HttpPost("students")]
public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto) { }
```

### ❌ Passing Entities Instead of DTOs
```csharp
// WRONG
public async Task<StudentTable> CreateStudent(StudentTable entity)
{
    _context.StudentsTables.Add(entity);
    await _context.SaveChangesAsync();
    return entity; // Exposes all properties
}
```

### ✅ Use DTOs
```csharp
// CORRECT
public async Task<StudentDto> CreateStudent(CreateStudentDto dto)
{
    var student = new StudentTable { /* map from dto */ };
    _context.StudentsTables.Add(student);
    await _context.SaveChangesAsync();
    return new StudentDto { /* map from entity */ };
}
```

### ❌ No Error Handling
```csharp
// WRONG
public async Task<IActionResult> DeleteStudent(int id)
{
    var student = await _context.StudentsTables.FindAsync(id);
    _context.StudentsTables.Remove(student);
    await _context.SaveChangesAsync();
    return Ok();
}
```

### ✅ Proper Error Handling
```csharp
// CORRECT
public async Task<IActionResult> DeleteStudent(int id)
{
    try
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Failed to delete" });
    }
}
```

### ❌ No Validation
```csharp
// WRONG
public string NormalizeEmail(string email)
{
    return email.ToLower();
}
```

### ✅ Comprehensive Validation
```csharp
// CORRECT
public string NormalizeEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        throw new ArgumentException("Email cannot be empty");

    var normalized = email.Trim().ToLower();
    if (!normalized.Contains("@"))
        throw new ArgumentException("Invalid email format");

    return normalized;
}
```

### ❌ Hardcoded Values
```csharp
// WRONG
public const int MaxNameLength = 100; // Scattered throughout code
```

### ✅ Centralized Constants
```csharp
// CORRECT - in SD.cs or Constants.cs
public static class ValidationRules
{
    public const int MaxNameLength = 100;
    public const int MinPhoneLength = 10;
}
```

---

## HTTP Status Code Reference

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT, DELETE |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE (no response body) |
| 400 | Bad Request | Invalid input, validation error |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Business rule violation (duplicate, FK violation) |
| 500 | Server Error | Unhandled exception |

---

## Testing Guidelines

### Unit Tests
- Test individual service methods
- Use in-memory database for isolation
- Mock external dependencies
- Test happy path and error cases

### Integration Tests
- Test full request/response cycle
- Use real database (or in-memory)
- Test with valid and invalid data
- Test authorization

### Test Coverage Target
- Services: 90%+
- Controllers: 80%+
- Utilities: 95%+

---

## Documentation

Every new module should include:
1. Architecture guide (like ARCHITECTURE_GUIDE.md)
2. API documentation (endpoints, request/response)
3. Database schema documentation
4. User guide
5. Troubleshooting guide

---

## Deployment Checklist

- [ ] All tests passing
- [ ] No compilation errors
- [ ] Database migration tested
- [ ] API endpoints tested with Postman
- [ ] UI pages tested
- [ ] Error handling complete
- [ ] Logging configured
- [ ] Documentation complete
- [ ] Security review completed
- [ ] Performance benchmarks acceptable

---

## Quick Reference: Module Creation Checklist

Use this checklist when creating any new module:

- [ ] Entity created (Models/FeatureName.cs)
- [ ] DTOs created (Models/DTOs/*)
- [ ] Service interface created (IServices/I*Service.cs)
- [ ] Service implementation created (Services/*Service.cs)
- [ ] DbContext updated (Data/ApplicationDbContext.cs)
- [ ] Service registered in DI (Program.cs)
- [ ] Database migration created and applied
- [ ] v1Controller endpoints implemented
- [ ] HomeController DataTable endpoint implemented
- [ ] Razor Pages created (Create, Edit, List, Details)
- [ ] Unit tests created (90%+ coverage)
- [ ] Integration tests created
- [ ] All tests passing
- [ ] Architecture documentation created
- [ ] API documentation created
- [ ] User documentation created
- [ ] Deployed to development environment
- [ ] Deployed to staging/production

---

## Support & Questions

For questions about architecture or standards:
1. Review this document
2. Review ARCHITECTURE_GUIDE.md (for specific module)
3. Check existing module implementation
4. Ask team lead

---

**Last Updated**: 2025-01-14
**Version**: 1.0
**Status**: Active (All projects must follow this standard)
