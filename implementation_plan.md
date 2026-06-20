# Fee Payment Module Enhancement — Implementation Plan

## Summary

Redesign the **Make.cshtml** payment entry page and the underlying service/model layer
to support:
- User-selected **Session + Term + Payment Item** (with category shown) instead of the
  current class + category picker
- Full **part-payment** tracking per item
- Complete **payment history** per item
- Automatic **Fully Paid / Partially Paid / Unpaid** status calculation
- Safe, **idempotent** payment editing
- Audit trail (who recorded/edited + when)

---

## Gap Analysis (current vs. required)

| Requirement | Current State | Gap |
|---|---|---|
| Session + Term on lookup form | Session hardcoded to active session, term hardcoded to First | Must be user-selected dropdowns |
| Payment Item picker (showing category) | Category picker → all items in category | Single **Payment Item** picker with category text |
| Student reg number search | Text input | ✅ already present |
| Part-payment per transaction | ✅ AmountPaid per item | ✅ already supported at DB level |
| Payment history per item | Not displayed | New: show all previous transactions per item |
| Paid/Partial/Fully Paid status badge | Not shown | New: auto-calculated from totals |
| Running balance after each transaction | Not tracked | New: computed from cumulative paid amounts |
| Who recorded payment | Not stored on `StudentPayment` | Add `RecordedBy` (ApplicationUser FK) |
| Idempotent edit | No edit exists | New: `OnPostUpdateAsync` in controller / service |
| Audit log | Partially via AuditLog table | Wire `RecordedBy` + `UpdatedAt` to AuditLog |

---

## User Review Required

> [!IMPORTANT]
> **`RecordedBy` column** — Adding a nullable `string? RecordedBy` (stores the logged-in
> user's UserName) to `StudentPayment` requires a **new EF Core migration**.
> Please confirm you are happy for a migration to be generated and applied.

> [!IMPORTANT]
> **Session / Term on the lookup** — The new form lets the user pick any session and
> term, not just the "active" one. This means staff can record historical payments.
> Confirm this is the desired behaviour.

> [!NOTE]
> **Existing `LookupPayableItemsAsync`** currently uses the active session and
> first term (hardcoded). The new `LookupByItemAsync` replaces it for the Make page;
> the old method is kept for backward compatibility.

---

## Open Questions

> [!WARNING]
> Should payment editing be allowed after a payment has been **Approved**?
> Currently the plan restricts editing to `Pending` state only. Reply to confirm.

> [!NOTE]
> The new lookup returns a **single payment item**; if a student has multiple items
> to pay in one session, staff will need to record a separate transaction per item,
> OR we batch them. The spec says "record separately per item" — this plan follows that.

---

## Proposed Changes

### 1. Domain Model — `StudentPayment.cs`

#### [MODIFY] [StudentPayment.cs](file:///c:/Users/BDIC-004/Documents/Visual%20Studio%202026/Projects/UpgradedSchoolManagement/UpgradedSchoolManagementModels/Models/StudentPayment.cs)

Add `RecordedBy` (nullable string, stores UserName of the staff member) and
`UpdatedAt` (for edit audit trail):

```csharp
[StringLength(256)]
public string? RecordedBy { get; set; }   // UserName of recording staff

public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
```

---

### 2. ViewModels — `StudentPaymentViewModels.cs`

#### [MODIFY] [StudentPaymentViewModels.cs](file:///c:/Users/BDIC-004/Documents/Visual%20Studio%202026/Projects/UpgradedSchoolManagement/UpgradedSchoolManagementModels/ViewModels/StudentPaymentViewModels.cs)

**New / changed view-models:**

```
PaymentItemLookupRequest     – new: SessionId, Term, PaymentItemId, AdmissionNo
SingleItemLookupResult       – new: student info + item info + history rows + balance summary
PaymentHistoryRow            – new: Reference, Date, AmountPaid, RecordedBy, State, RunningBalance
ItemBalanceSummary           – new: TotalDue, TotalPaid, Outstanding, PaymentStatusLabel
CreateSingleItemPaymentVM    – new: TermRegId, PaymentItemId, AmountPaid, Narration
UpdatePaymentAmountVM        – new: PaymentId, NewAmount, Narration (for idempotent edit)
```

---

### 3. Service Interface — `IStudentPaymentService.cs`

#### [MODIFY] [IStudentPaymentService.cs](file:///c:/Users/BDIC-004/Documents/Visual%20Studio%202026/Projects/UpgradedSchoolManagement/UpgradedSchoolManagementDataAccess/IServices/IStudentPaymentService.cs)

Add three new method signatures:

```csharp
// New lookup: by session + term + payment item + admission no
Task<ApiResponse<SingleItemLookupResult>> LookupByItemAsync(
    int sessionId, Term term, int paymentItemId, string admissionNo);

// New: record a single-item part payment
Task<ApiResponse<int>> CreateSingleItemPaymentAsync(
    CreateSingleItemPaymentVM model, string? recordedBy);

// New: idempotent edit of an existing payment's amount
Task<ApiResponse<bool>> UpdatePaymentAmountAsync(
    UpdatePaymentAmountVM model, string? updatedBy);
```

---

### 4. Service Implementation — `StudentPaymentService.cs`

#### [MODIFY] [StudentPaymentService.cs](file:///c:/Users/BDIC-004/Documents/Visual%20Studio%202026/Projects/UpgradedSchoolManagement/UpgradedSchoolManagementDataAccess/Services/StudentPaymentService.cs)

Implement the three new methods:

**`LookupByItemAsync`**
1. Validate student by admission no
2. Find `TermRegistration` for (student, session, term) — no class filter needed
3. Validate `PaymentSetup` exists for (paymentItemId, session, term, student's class)
4. Aggregate all non-reversed/non-rejected `StudentPaymentItems` for this item in this termReg
5. Build payment history rows with running balance
6. Return `SingleItemLookupResult` with balance summary + history

**`CreateSingleItemPaymentAsync`**
1. Validate termReg exists
2. Validate paymentSetup exists for the item
3. Overpayment prevention: `alreadyPaid + newAmount <= configuredAmount`
4. Create `StudentPayment` + single `StudentPaymentItem`, set `RecordedBy`
5. Return new payment Id

**`UpdatePaymentAmountAsync`** (idempotent)
1. Load the payment — must be in `Pending` state
2. Load the single `StudentPaymentItem` on this payment
3. Calculate: `alreadyPaidByOthers + newAmount <= configuredAmount`
4. Update `AmountPaid` on item, `TotalAmount` on header, `UpdatedAt`, `RecordedBy`
5. Return success — calling this twice with the same amount is safe

---

### 5. API Controller — `v1Controller.cs`

#### [MODIFY] [v1Controller.cs](file:///c:/Users/BDIC-004/Documents/Visual%20Studio%202026/Projects/UpgradedSchoolManagement/UpgradedSchoolManagementWeb/Controllers/v1Controller.cs)

Add three new POST endpoints:

```
POST /V1/LookupByItem          → LookupByItemAsync
POST /V1/CreateSingleItemPayment → CreateSingleItemPaymentAsync
POST /V1/UpdatePaymentAmount   → UpdatePaymentAmountAsync
```

---

### 6. Make.cshtml (Razor Page View)

#### [MODIFY] [Make.cshtml](file:///c:/Users/BDIC-004/Documents/Visual%20Studio%202026/Projects/UpgradedSchoolManagement/UpgradedSchoolManagementWeb/Pages/Admin/Finance/Payments/Make.cshtml)

**Lookup form (new fields):**
- Academic Session (server-side `asp-items`)
- Term (server-side `asp-items`)
- Payment Item — `<select>` populated from server, showing "Item Name (Category)" format
- Admission Number (text input)
- Search button

**Result section (shown after successful lookup):**
- Student info card: Full Name, Reg Number, Current Class
- Payment item summary card: Item, Category, Total Due, Total Paid, Outstanding, status badge
- Payment history table: Reference | Date | Amount Paid | Recorded By | Status | Running Balance
- Payment entry form: Amount input (max = outstanding), Narration, Submit button
- Edit panel (if redirected from Detail): pre-fills existing amount for update

**JS responsibilities (retained):**
- AJAX lookup (`/V1/LookupByItem`) → render result section
- AJAX submit (`/V1/CreateSingleItemPayment` or `/V1/UpdatePaymentAmount`)
- Spinner + SweetAlert feedback
- Live remaining balance counter as user types amount

---

### 7. Make.cshtml.cs (Code-behind)

#### [MODIFY] [Make.cshtml.cs](file:///c:/Users/BDIC-004/Documents/Visual%20Studio%202026/Projects/UpgradedSchoolManagement/UpgradedSchoolManagementWeb/Pages/Admin/Finance/Payments/Make.cshtml.cs)

Load dropdowns properly (async, no `.Result`):
- Sessions via `IViewSelectionService.GetSessionsForDropdownAsync()`
- Terms via `IViewSelectionService.GetTermForDropdown()`
- Payment Items via new `IPaymentItemService.GetActiveItemsWithCategoryAsync()` → returns
  items with category name for `"Item (Category)"` display

---

### 8. New EF Migration

#### [NEW] Migration: `AddRecordedByAndUpdatedAtToStudentPayment`

```
Add-Migration AddRecordedByAndUpdatedAtToStudentPayment
Update-Database
```

Adds:
- `RecordedBy` (nvarchar(256), nullable)
- `UpdatedAt` (datetime2, default UtcNow)

---

## File Change Summary

| File | Action | Change |
|---|---|---|
| `StudentPayment.cs` | MODIFY | Add `RecordedBy`, `UpdatedAt` |
| `StudentPaymentViewModels.cs` | MODIFY | Add 6 new view-model classes |
| `IStudentPaymentService.cs` | MODIFY | Add 3 new method signatures |
| `StudentPaymentService.cs` | MODIFY | Implement 3 new methods |
| `v1Controller.cs` | MODIFY | Add 3 new POST endpoints |
| `Make.cshtml` | MODIFY | Complete UI redesign |
| `Make.cshtml.cs` | MODIFY | Fix async loading, add payment-item dropdown |
| `IPaymentItemService.cs` | MODIFY | Add `GetActiveItemsWithCategoryAsync` |
| `PaymentItemService.cs` | MODIFY | Implement `GetActiveItemsWithCategoryAsync` |
| Migration | NEW | `AddRecordedByAndUpdatedAtToStudentPayment` |

---

## Verification Plan

### Automated Tests
- Build the solution: `dotnet build`
- Run migrations: `dotnet ef database update`

### Manual Verification
1. Navigate to **Make Payment** page — confirm Session, Term, Payment Item dropdowns populate.
2. Enter a valid student admission number and click Search — confirm student info + history table appears.
3. Enter a partial payment amount — confirm balance updates correctly.
4. Submit — confirm payment is recorded and history table shows new row.
5. Submit again with same amount — confirm no duplicate (idempotency on edit).
6. Enter amount > remaining — confirm overpayment is blocked.
7. After full payment — confirm "Fully Paid" badge shown and input is disabled.
