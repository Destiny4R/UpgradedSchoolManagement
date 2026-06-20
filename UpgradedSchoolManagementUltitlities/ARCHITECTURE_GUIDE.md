# Parent/Guardian Linking Guide for UpgradedSchoolManagement

## Overview
This guide documents the complete architecture for the Parent/Guardian management module. It ensures proper separation of concerns, prevents data duplication, and maintains consistency across the entire system.

---

## Goal
- Add parent/guardian information for an enrolled student
- Prevent duplicate parent records by using **phone numbers as unique identifiers**
- If phone matches an existing parent: warn the user and offer two options:
  - **Yes** → Link student to existing parent
  - **No** → Edit phone and create new parent

---

## Architecture Constraints (STRICTLY FOLLOW)

### Project Structure
- **UpgradedSchoolManagementModels** (Data models and transfer objects)
  - `Models` folder → EF Core entities and DbContext classes
  - `DTOs` folder → Data Transfer Objects (service-to-controller communication)
  - `ViewModels` folder → Razor Page model objects (UI-specific data)

- **UpgradedSchoolManagementDataAccess** (Database and services)
  - `IServices` folder → Service interfaces
  - `Services` folder → Service implementations
  - `Data` folder → DbContext

- **UpgradedSchoolManagementUltitlities** (Shared helpers)
  - `SD.cs` → All static utility functions

- **UpgradedSchoolManagementWeb** (Presentation layer)
  - `Controllers` → API endpoints
  - `Pages` → Razor Pages

### Controller Responsibilities
- **HomeController.cs**
  - Only handles DataTables server-side endpoints
  - Used exclusively for table listing/read operations
  - Example: `POST /home/get-parents-datatable`

- **v1Controller.cs**
  - Handles all CRUD operations (Create, Read, Update, Delete)
  - Handles get by ID, toggle/status actions
  - Handles linking, unlinking, and relationship management
  - Example endpoints:
    - `POST /v1/parents/add-or-link`
    - `POST /v1/parents/link`
    - `DELETE /v1/parents/{id}`
    - `POST /v1/parents/{parentId}/set-primary`

### Database Logic Layer
- All database operations MUST use service interfaces and implementations
- No direct DbContext access in controllers or Razor Pages
- Services MUST be injected via dependency injection

### Static/Helper Functions
- Place all reusable static functions in `UpgradedSchoolManagementUltitlities.SD`
- Current implementations:
  - `NormalizePhone(string phone)` → Removes non-digits, standardizes format
  - `IsValidPhone(string phone, int minLength = 10)` → Validates phone length
  - `FormatPhoneForDisplay(string normalizedPhone)` → Formats for UI display

---

## Data Model

### Entities

#### ParentGuardian
```
- Id (int, PK)
- FullName (string[100], required)
- Relationship (string[50], required) → Father, Mother, Guardian, Aunt, Uncle, etc.
- Occupation (string[100], nullable)
- Address (string[300], nullable)
- Phone1 (string[20], required, unique) → PRIMARY PHONE (stored normalized)
- Phone2 (string[20], nullable, unique) → SECONDARY PHONE (stored normalized)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- Navigation: StudentLinks (ICollection<StudentParentLink>)
```

#### StudentsTable
Already exists. Links to ParentGuardian via StudentParentLink junction table.

#### StudentParentLink (Junction Table)
```
- Id (int, PK)
- StudentId (int, FK, required)
- ParentGuardianId (int, FK, required)
- LinkedAt (DateTime)
- IsPrimaryContact (bool) → True for primary guardian, false for additional guardians
- Unique Index: (StudentId, ParentGuardianId) → Prevents duplicate links
```

---

## Phone Number Strategy

### Normalization
1. All phone numbers are normalized (digits only) before storage
2. Normalization removes: spaces, dashes, parentheses, country codes, etc.
3. Example: "+234-801-234-5678" → "2348012345678"

### Uniqueness Enforcement
1. Database-level: Unique indexes on ParentGuardian.Phone1 and ParentGuardian.Phone2
2. Service-level: Check before creating/updating parents
3. Conflict detection: Returns existing parent data if phone already exists

### Display Format
Phone numbers are formatted for display using `SD.FormatPhoneForDisplay()`
- Example: "2348012345678" → "+234-801-234-5678"

---

## DTOs (Data Transfer Objects)

### ParentGuardianCreateDto
Used for creating/updating parent records. Receives raw phone input (client may include formatting).

### ParentGuardianDto
Used for returning parent data to client/UI. Phones are formatted for display.

### ParentPhoneConflictDto
Returned when a phone conflict is detected. Contains existing parent details.

### LinkParentToStudentDto
Used for linking an existing parent to a student (after conflict resolution).

### ParentOperationResultDto
Generic result DTO for all parent operations:
- `Success` → Operation succeeded
- `IsConflict` → Phone conflict detected (return 409 Conflict)
- `Message` → Result message
- `Parent` → Parent DTO (if operation succeeded or conflict)

---

## Service Layer

### IParentGuardianService Interface
Core methods:

```csharp
// Attempts to create or link parent; detects phone conflicts
Task<ParentOperationResultDto> AddOrLinkParentAsync(ParentGuardianCreateDto dto, int studentId)

// Links existing parent to student (idempotent)
Task<ParentOperationResultDto> LinkExistingParentToStudentAsync(int parentId, int studentId, bool isPrimaryContact = false)

// Get parent by ID
Task<ParentGuardianDto?> GetParentByIdAsync(int parentId)

// Get all parents for a student
Task<List<ParentGuardianDto>> GetParentsByStudentAsync(int studentId)

// Get parent by normalized phone
Task<ParentGuardianDto?> GetParentByPhoneAsync(string normalizedPhone)

// Update parent information
Task<ParentOperationResultDto> UpdateParentAsync(int parentId, ParentGuardianCreateDto dto)

// Delete parent and all links
Task<ParentOperationResultDto> DeleteParentAsync(int parentId)

// Unlink parent from student
Task<ParentOperationResultDto> UnlinkParentFromStudentAsync(int studentId, int parentId)

// Get all students linked to a parent
Task<List<int>> GetStudentsByParentAsync(int parentId)

// Set parent as primary contact for a student
Task<ParentOperationResultDto> SetPrimaryContactAsync(int studentId, int parentId)
```

### Service Behavior

#### AddOrLinkParentAsync
1. Validates student exists
2. Normalizes phone numbers
3. Checks if phone1 or phone2 already exist
4. If conflict detected: Return IsConflict=true with existing parent data
5. If no conflict: Create parent, link to student, return success
6. First parent linked is automatically primary contact

#### LinkExistingParentToStudentAsync
1. Validates parent and student exist
2. Checks if link already exists (idempotent behavior)
3. If link exists and isPrimaryContact=true: Updates primary contact status
4. If link doesn't exist: Creates link and saves

---

## Controller Flow

### POST /v1/parents/add-or-link
**Request Body:**
```json
{
  "studentId": 5,
  "parentData": {
    "fullName": "John Doe",
    "relationship": "Father",
    "occupation": "Engineer",
    "address": "123 Main St",
    "phone1": "+234-801-234-5678",
    "phone2": "+234-802-987-6543"
  }
}
```

**Response (No Conflict):**
```json
{
  "statusCode": 201,
  "isSuccess": true,
  "message": "Parent created and linked successfully",
  "data": {
    "success": true,
    "message": "Parent/Guardian created and linked successfully.",
    "parent": { ... }
  }
}
```

**Response (Phone Conflict):**
```json
{
  "statusCode": 409,
  "isSuccess": false,
  "message": "Phone conflict detected",
  "data": {
    "success": false,
    "isConflict": true,
    "message": "Parent with phone +234-801-234-5678 already exists.",
    "parent": { ... existing parent data ... }
  }
}
```

### POST /v1/parents/link
**Request Body (After User Confirms):**
```json
{
  "studentId": 5,
  "parentGuardianId": 3,
  "isPrimaryContact": false
}
```

**Response:**
```json
{
  "statusCode": 200,
  "isSuccess": true,
  "message": "Parent linked successfully"
}
```

---

## Razor Page & Client Behavior

### Step 1: User Fills Form
- User enters parent information in form
- Submits to `/v1/parents/add-or-link`

### Step 2: Check Response
- If status 201 (Created): Success! Display confirmation message
- If status 409 (Conflict): Existing parent found!

### Step 3: Handle Conflict
Display modal/dialog:
```
"A parent/guardian with phone +234-801-234-5678 already exists:
Name: John Doe
Relationship: Father
Occupation: Engineer

Do you want to link this parent to the student?
[Yes] [No]"
```

### Step 4: User Action
- **Yes**: Call POST /v1/parents/link with existingParentId and studentId
- **No**: Allow user to edit phone number and resubmit

---

## Database Configuration

### Unique Indexes
- `ParentGuardian.Phone1` → Unique (nullable: false)
- `ParentGuardian.Phone2` → Unique (nullable: true)
- `StudentParentLink (StudentId, ParentGuardianId)` → Unique (prevents duplicate links)

### Foreign Key Constraints
- `StudentParentLink.StudentId` → FK to StudentsTable.Id (Cascade Delete)
- `StudentParentLink.ParentGuardianId` → FK to ParentGuardian.Id (Cascade Delete)

### Migration
```bash
dotnet ef migrations add AddParentGuardianModule
dotnet ef database update
```

---

## Utilities (SD.cs)

### Implemented Functions

#### NormalizePhone(string phone)
- Removes all non-digit characters
- Returns digits-only string
- Example: "+234-801-234-5678" → "2348012345678"

#### IsValidPhone(string phone, int minLength = 10)
- Validates phone after normalization
- Checks minimum length (default: 10 digits)
- Returns true if valid, false otherwise

#### FormatPhoneForDisplay(string normalizedPhone)
- Formats normalized phone for UI display
- Detects Nigerian format (starts with 234)
- Example: "2348012345678" → "+234-801-234-5678"

---

## Testing

### Unit Tests
- **ParentService.Tests**
  - Test phone normalization
  - Test phone conflict detection
  - Test creating parent (no conflict)
  - Test linking existing parent (idempotent)
  - Test unlinking parent
  - Test setting primary contact

### Integration Tests
- **v1Controller.Tests**
  - Test POST /v1/parents/add-or-link (success)
  - Test POST /v1/parents/add-or-link (conflict)
  - Test POST /v1/parents/link
  - Test DELETE /v1/parents/{id}
  - Test GET /v1/parents/{id}

---

## Summary: Implementation Checklist

- [x] Add phone utilities to `SD.cs`
- [x] Create `ParentGuardian` entity in `Models`
- [x] Create `StudentParentLink` entity in `Models`
- [x] Create DTOs in `Models/DTOs`
- [x] Create `IParentGuardianService` interface
- [x] Create `ParentGuardianService` implementation
- [x] Update `ApplicationDbContext` with new DbSets and configurations
- [ ] Create database migration
- [ ] Create v1Controller endpoints
- [ ] Create Razor Pages (Add/Edit/Link)
- [ ] Add unit tests
- [ ] Add integration tests

---

## Key Design Principles

1. **Unique Phone Identification** → Use phone as primary duplicate detector
2. **Normalization** → Always normalize before comparison/storage
3. **Idempotent Operations** → Linking twice should not create duplicates
4. **Separation of Concerns** → Services handle logic; controllers handle HTTP
5. **Relationship Support** → Many-to-many via junction table
6. **Primary Contact** → One parent per student marked as primary

---

## File Locations

- **Entities**: `UpgradedSchoolManagementModels/Models/ParentGuardian.cs`, `StudentParentLink.cs`
- **DTOs**: `UpgradedSchoolManagementModels/DTOs/ParentGuardianDto.cs`
- **Service Interface**: `UpgradedSchoolManagementDataAccess/IServices/IParentGuardianService.cs`
- **Service Implementation**: `UpgradedSchoolManagementDataAccess/Services/ParentGuardianService.cs`
- **DbContext**: `UpgradedSchoolManagementDataAccess/Data/ApplicationDbContext.cs`
- **Utilities**: `UpgradedSchoolManagementUltitlities/SD.cs`
- **Controllers** (to implement): `UpgradedSchoolManagementWeb/Controllers/v1Controller.cs`
- **Razor Pages** (to implement): `UpgradedSchoolManagementWeb/Pages/Parents/...`
