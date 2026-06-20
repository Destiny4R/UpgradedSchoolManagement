# Graham School Admin System — Fees Payment System

## Overview

The fees payment system is a multi-layer, end-to-end solution that lets administrators
configure what a student owes, process and verify each payment, generate receipts,
and analyse collection performance. It follows a strict hierarchical flow:

```
Category → Item → Setup (amount + class) → Student Payment → Receipt → Reports
```

---

## Architecture

The solution is divided into three projects:

| Project | Role |
|---|---|
| `GrahamSchoolAdminSystemModels` | Domain models, ViewModels, DTOs, Enums |
| `GrahamSchoolAdminSystemAccess` | EF Core DbContext, service interfaces & implementations |
| `GrahamSchoolAdminSystemWeb` | Razor Pages UI, REST API controller (`v1Controller`) |

---

## Domain Models

### `PaymentCategory`
**File:** `GrahamSchoolAdminSystemModels/Models/PaymentCategory.cs`

The top-level grouping for fees (e.g. *School Fees*, *PTA Levy*, *Exam Fees*).

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `Name` | `string(100)` | Unique category name |
| `Description` | `string?(300)` | Optional description |
| `IsActive` | `bool` | Soft-enable/disable |
| `CreatedAt` / `UpdatedAt` | `DateTime` | Audit timestamps |
| → `PaymentItems` | nav | One-to-many |

---

### `PaymentItem`
**File:** `GrahamSchoolAdminSystemModels/Models/PaymentItem.cs`

A named fee line that belongs to a category (e.g. *Tuition*, *Development Levy*).

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `CategoryId` | `int` | FK → `PaymentCategory` |
| `Name` | `string(100)` | Unique within category |
| `Description` | `string?(300)` | Optional |
| `IsActive` | `bool` | Soft-enable/disable |
| `CreatedAt` / `UpdatedAt` | `DateTime` | Audit timestamps |
| → `PaymentCategory` | nav | Parent category |
| → `PaymentSetups` | nav | Amount configs for each class |

---

### `PaymentSetup`
**File:** `GrahamSchoolAdminSystemModels/Models/PaymentSetup.cs`

Assigns a specific monetary amount to a payment item for a given **Session + Term + Class** combination.

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `PaymentItemId` | `int` | FK → `PaymentItem` |
| `SessionId` | `int` | FK → `SessionYear` |
| `Term` | `enum Term` | First / Second / Third |
| `ClassId` | `int` | FK → `SchoolClasses` |
| `Amount` | `decimal(18,2)` | Fee amount for that class |
| `IsCompulsory` | `bool` | **NEW** — Must be paid before results can be viewed |
| `IsActive` | `bool` | Active/inactive toggle |
| `CreatedAt` / `UpdatedAt` | `DateTime` | Audit timestamps |

> **Unique constraint:** `(PaymentItemId, SessionId, Term, ClassId)` — only one setup per combination.

---

### `StudentPayment`
**File:** `GrahamSchoolAdminSystemModels/Models/StudentPayment.cs`

A single payment transaction made by or on behalf of a student.

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `TermRegId` | `int` | FK → `TermRegistration` |
| `TotalAmount` | `decimal` | Sum of all line items |
| `PaymentDate` | `DateTime` | UTC timestamp |
| `Reference` | `string` | Unique auto-generated ref |
| `Status` | `enum PaymentStatus` | Pending / Completed / Failed / Reversed |
| `State` | `enum PaymentState` | Pending / Approved / Rejected / Cancelled |
| `Narration` | `string?(120)` | Optional notes |
| `RejectMessage` | `string?(120)` | Populated when rejected |
| `EvidenceFilePath` | `string?(420)` | Uploaded proof of payment |
| → `PaymentItems` | nav | Breakdown of items paid |

---

### `StudentPaymentItem`
**File:** `GrahamSchoolAdminSystemModels/Models/StudentPaymentItem.cs`

A single line within a payment transaction (which item was paid and how much).

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `StudentPaymentId` | `int` | FK → `StudentPayment` |
| `PaymentItemId` | `int` | FK → `PaymentItem` |
| `AmountPaid` | `decimal` | Partial or full payment |

---

### Enumerations
**File:** `GrahamSchoolAdminSystemModels/Models/GetEnums.cs`

```csharp
Term       { First=1, Second=2, Third=3 }
PaymentStatus { Pending=1, Completed=2, Failed=3, Reversed=4 }
PaymentState  { Pending=1, Approved=2, Rejected=3, Cancelled=4 }
```

---

## Database — Entity Relationships

```
PaymentCategory
    └── PaymentItem (CategoryId FK, Restrict delete)
            └── PaymentSetup (PaymentItemId FK, Restrict delete)
                    │   unique(PaymentItemId, SessionId, Term, ClassId)
                    │
                    └── linked to SessionYear (FK)
                    └── linked to SchoolClasses (FK)

TermRegistration (Student + Session + Term + Class + SubClass)
    └── StudentPayment (TermRegId FK, Restrict delete)
            └── StudentPaymentItem (StudentPaymentId FK, Cascade delete)
                    └── PaymentItem (PaymentItemId FK, Restrict delete)
```

---

## Full Feature Flow

### STEP 1 — Payment Category Setup
**Page:** `/admin/payment-categories`
**API:** `POST /api/v1/paymentcategories/create`, `PUT .../update`, `DELETE .../id`

An administrator creates top-level fee buckets.  
**Validation:** duplicate name check; cannot delete if child items exist.

---

### STEP 2 — Payment Item Setup
**Page:** `/admin/payment-items`
**API:** `POST /api/v1/paymentitems/create`, `PUT .../update`, `DELETE .../id`

Named fee lines are created and assigned to a category.  
**Validation:** duplicate name within same category; cannot delete if setups exist.

---

### STEP 3 — Payment Amount Setup
**Page:** `/admin/payment-setup`
**API:** `POST /api/v1/paymentsetups/create-batch` (multi-class), `PUT .../update`, `DELETE .../id`

Maps a payment item to one or more classes with a fixed amount per session/term.  
**Supports batch creation** — selecting multiple classes at once.

> **NEW FEATURE — `IsCompulsory` flag**  
> When setting up a payment, the user can now check the **"Compulsory"** toggle.  
> A `IsCompulsory = true` payment must be **fully paid and approved** before the
> student's academic result is released/viewable.

---

### STEP 4 — Student Term Registration
**Page:** `/admin/termly-registeration`
**Service:** `TermRegistrationServices`

A student must be registered for a specific session + term + class before a payment
can be created for them. This creates the `TermRegistration` record that serves as
the anchor for all their payments in that term.

---

### STEP 5 — Making a Payment
**Page:** `/admin/student-payments/new-payment` or `/admin/student-payments/make-payment`
**Service:** `StudentPaymentService.LookupPayableItemsAsync()` → `CreatePaymentAsync()`

**Flow:**
1. Staff looks up student by **Admission Number + Class + Category**.
2. System resolves the active `TermRegistration` for the current session/term.
3. All active `PaymentSetups` for that class/session/term/category are loaded.
4. Already-paid amounts (from non-reversed/non-rejected `StudentPaymentItems`) are subtracted per item.
5. Staff selects which items to pay and enters amounts (cannot exceed remaining balance — overpayment prevention).
6. On submit, a `StudentPayment` header + `StudentPaymentItem` lines are saved.
7. Payment starts with `State = Pending`, `Status = Completed`.
8. If `AppSettings.PaymentEvidence = true`, an evidence file upload is required.

---

### STEP 6 — Payment Approval / Rejection
**Page:** `/admin/student-payments` (detail view)
**API:** `PUT /api/v1/studentpayments/{id}/state`

An authorised admin reviews the payment and sets `State`:
- **Approved** → payment counts toward balances and reports.
- **Rejected** → `Status` flips to `Reversed`; amount is excluded from all calculations.
- **Cancelled** → payment excluded from active totals.

---

### STEP 7 — Receipt Download
**Pages:**
- `/admin/student-payments/receipt?paymentId=X` — single payment receipt  
- `/admin/student-payments/full-receipt?termRegId=X` — consolidated all-payments receipt

**Service:** `GetReceiptAsync()` / `GetConsolidatedReceiptAsync()`

The consolidated receipt groups all approved payments by category and lists each line item with the amount paid, all payment references, and a grand total.

---

### STEP 8 — Compulsory Fee Check (Result Gating)
**Service:** `StudentPaymentService.HasPaidAllCompulsoryFeesAsync(int termRegId)`

> **NEW FEATURE**

Before a student's result is displayed, the system calls this method. It:
1. Fetches all `PaymentSetups` for the student's class + session + term where `IsCompulsory = true`.
2. Computes total paid (from approved, non-reversed `StudentPaymentItems`) per item.
3. Returns `true` only if **every** compulsory item is **fully paid**.

If any compulsory fee is outstanding, the result view is blocked and a message
lists the unpaid items.

---

### STEP 9 — Reports & Analytics
**Pages:** `/admin/payment-reports/`

Three report views are available:

#### Class Report (`class-report`)
**Service:** `GetClassReportAsync()`

Shows each student's **expected vs. paid** amount per category (or per item).
Filters: Session, Term, Class, Sub-class, Category, Payment Item.

#### School-wide Report (`school-report`)
**Service:** `GetSchoolReportAsync()`

Aggregates across all classes — rows are grouped by Category × Item.
Shows: total students, students who paid, total expected, total collected, outstanding.

#### Category/Item Report (`category-report`)
**Service:** `GetCategoryItemReportAsync()`

Breaks down collection per fee item per class.
Shows: expected amount, amount collected, students paid, outstanding.

#### Dashboard Analytics
**Service:** `GetDashboardCategorySummaryAsync()`, `GetDashboardItemSummaryAsync()`, `GetDashboardCategoryTrendAsync()`

- Category summary: expected vs. collected per category.
- Item summary: per-item expected vs. collected.
- Trend chart: historical collection per category across sessions.
- Item bar chart: side-by-side expected/collected per item.
- Term registration chart: registrations per term in a session.

---

## Service Layer

| Interface | Implementation | Responsibility |
|---|---|---|
| `IPaymentCategoryService` | `PaymentCategoryService` | CRUD + toggle for categories |
| `IPaymentItemService` | `PaymentItemService` | CRUD + toggle for items |
| `IPaymentSetupService` | `PaymentSetupService` | CRUD + batch create + toggle for setups |
| `IStudentPaymentService` | `StudentPaymentService` | Lookup payable items, create payment, receipts, state management, compulsory check |
| `IPaymentReportService` | `PaymentReportService` | Class, school, category reports + dashboard charts |

All services return `ServiceResponse<T>` — a generic wrapper with `Succeeded`, `Message`, and `Data`.

---

## REST API Endpoints (v1Controller)

All endpoints are under `/api/v1/` and require `[Authorize]`.

### Payment Categories
| Method | Route | Action |
|---|---|---|
| POST | `paymentcategories/create` | Create |
| GET | `paymentcategories/{id}` | Get by ID |
| GET | `paymentcategories/active` | List active |
| PUT | `paymentcategories/update` | Update |
| DELETE | `paymentcategories/{id}` | Delete |
| POST | `paymentcategories/{id}/toggle` | Toggle active |

### Payment Items
| Method | Route | Action |
|---|---|---|
| POST | `paymentitems/create` | Create |
| GET | `paymentitems/{id}` | Get by ID |
| GET | `paymentitems/active` | List active (optional `?categoryId=`) |
| PUT | `paymentitems/update` | Update |
| DELETE | `paymentitems/{id}` | Delete |
| POST | `paymentitems/{id}/toggle` | Toggle active |

### Payment Setups
| Method | Route | Action |
|---|---|---|
| POST | `paymentsetups/create` | Create single |
| POST | `paymentsetups/create-batch` | Create for multiple classes |
| GET | `paymentsetups/{id}` | Get by ID |
| PUT | `paymentsetups/update` | Update (includes `IsCompulsory`) |
| DELETE | `paymentsetups/{id}` | Delete |
| POST | `paymentsetups/{id}/toggle` | Toggle active |

### Student Payments
| Method | Route | Action |
|---|---|---|
| POST | `studentpayments/lookup` | Find student payable items |
| POST | `studentpayments/create` | Record a payment |
| GET | `studentpayments/{id}` | Payment detail / receipt |
| PUT | `studentpayments/{id}/state` | Approve / Reject / Cancel |
| GET | `studentpayments/{id}/compulsory-check` | **NEW** — Check compulsory fees paid |

### Reports
| Method | Route | Action |
|---|---|---|
| GET | `reports/class` | Class-level fee report |
| GET | `reports/school` | School-wide report |
| GET | `reports/category` | Category/item report |
| GET | `reports/dashboard/categories` | Dashboard category summary |
| GET | `reports/dashboard/items` | Dashboard item summary |
| GET | `reports/dashboard/trend` | Historical trend |

---

## NEW FEATURE — `IsCompulsory` on Payment Setup

### Goal
When a fee is marked as **Compulsory**, a student **cannot view their academic result** 
until that fee is fully paid and approved.

### Changes Required

#### 1. Model: `PaymentSetup.cs`
Add the `IsCompulsory` boolean property:

```csharp
public bool IsCompulsory { get; set; } = false;
```

#### 2. ViewModel: `PaymentSetupViewModel.cs`
Add the property to the ViewModel:

```csharp
[Display(Name = "Compulsory")]
public bool IsCompulsory { get; set; } = false;
```

#### 3. Service: `PaymentSetupService.cs`
- Map `IsCompulsory` in `CreatePaymentSetupAsync`, `CreateBatchPaymentSetupAsync`, and `UpdatePaymentSetupAsync`.
- Include `IsCompulsory` in the data returned by `GetPaymentSetupsAsync` and `GetPaymentSetupByIdAsync`.

#### 4. Service: `StudentPaymentService.cs`
Add a new method `HasPaidAllCompulsoryFeesAsync(int termRegId)`:

```csharp
public async Task<(bool hasPaid, List<string> unpaidItems)>
    HasPaidAllCompulsoryFeesAsync(int termRegId)
```

**Logic:**
1. Load the `TermRegistration` (to get class, session, term).
2. Query all `PaymentSetups` where `IsCompulsory = true` AND matching class/session/term AND `IsActive = true`.
3. For each compulsory setup, sum all approved `StudentPaymentItems` for that item in this term registration.
4. If any item's paid amount < expected amount → fee is outstanding.
5. Return `(false, listOfUnpaidItems)` if any outstanding; otherwise `(true, [])`.

#### 5. Interface: `IStudentPaymentService.cs`
Add the method signature:

```csharp
Task<(bool hasPaid, List<string> unpaidItems)> HasPaidAllCompulsoryFeesAsync(int termRegId);
```

#### 6. UI: `payment-setup/index.cshtml`
- Add a **"Compulsory"** checkbox column to the table.
- Add a **"Compulsory"** checkbox to the Add modal and Edit modal.
- Show a badge (e.g., 🔒 *Compulsory*) in the table rows where `IsCompulsory = true`.

#### 7. API: `v1Controller.cs`
Add endpoint:
```
GET api/v1/studentpayments/{termRegId}/compulsory-check
```
Returns:
```json
{ "hasPaid": true/false, "unpaidItems": ["Item 1", "Item 2"] }
```

#### 8. Database Migration
Run:
```
Add-Migration AddIsCompulsoryToPaymentSetup
Update-Database
```

The migration adds a single nullable/default-false `IsCompulsory` column to `PaymentSetups`.

---

## Business Rules Summary

| Rule | Where Enforced |
|---|---|
| Category name must be unique | `PaymentCategoryService.CreatePaymentCategoryAsync` |
| Item name must be unique within category | `PaymentItemService.CreatePaymentItemAsync` |
| Setup uniqueness: one per Item+Session+Term+Class | DB unique index + service duplicate check |
| Amount must be > 0 | `PaymentSetupService` + ViewModel `Range` attribute |
| Student must be term-registered before payment | `LookupPayableItemsAsync` step 3 |
| Overpayment is blocked per item | `CreatePaymentAsync` — loops all items |
| Evidence file required if AppSettings.PaymentEvidence = true | `CreatePaymentAsync` |
| Rejected payments flip Status to Reversed | `UpdatePaymentStateAsync` |
| Compulsory fees must be fully paid for results | `HasPaidAllCompulsoryFeesAsync` |
| Reports count only Approved, non-reversed payments | All report service queries |

---

## Key File Reference

| File | Purpose |
|---|---|
| `Models/PaymentCategory.cs` | Category entity |
| `Models/PaymentItem.cs` | Item entity |
| `Models/PaymentSetup.cs` | Amount-per-class setup entity |
| `Models/StudentPayment.cs` | Payment transaction header |
| `Models/StudentPaymentItem.cs` | Payment line item |
| `Models/GetEnums.cs` | Term, PaymentStatus, PaymentState enums |
| `ViewModels/PaymentViewModels.cs` | PaymentCategoryViewModel, PaymentItemViewModel, PaymentSetupViewModel |
| `ViewModels/StudentPaymentViewModels.cs` | MakePaymentPageViewModel, PayableItemViewModel, PaymentReceiptViewModel, ConsolidatedReceiptViewModel |
| `ViewModels/PaymentReportViewModels.cs` | ClassReportRow, SchoolReportRow, CategoryItemReportRow, dashboard models |
| `ServiceRepo/PaymentCategoryService.cs` | Category CRUD |
| `ServiceRepo/PaymentItemService.cs` | Item CRUD |
| `ServiceRepo/PaymentSetupService.cs` | Setup CRUD + batch |
| `ServiceRepo/StudentPaymentService.cs` | Payment creation, receipt, state, compulsory check |
| `ServiceRepo/PaymentReportService.cs` | Class, school, category reports + dashboard |
| `Data/ApplicationDbContext.cs` | EF Core context, relationships, unique index |
| `Controllers/v1Controller.cs` | REST API (all payment routes) |
| `Pages/admin/payment-categories/index.cshtml` | Category management UI |
| `Pages/admin/payment-items/index.cshtml` | Item management UI |
| `Pages/admin/payment-setup/index.cshtml` | Setup management UI |
| `Pages/admin/student-payments/new-payment.cshtml` | New payment lookup page |
| `Pages/admin/student-payments/make-payment.cshtml` | Payment entry form |
| `Pages/admin/student-payments/receipt.cshtml` | Single payment receipt |
| `Pages/admin/student-payments/full-receipt.cshtml` | Consolidated receipt |
| `Pages/admin/payment-reports/class-report.cshtml` | Class fee report |
| `Pages/admin/payment-reports/school-report.cshtml` | School-wide report |
| `Pages/admin/payment-reports/category-report.cshtml` | Category/item report |
