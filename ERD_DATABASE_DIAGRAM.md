# BFAS Database - Entity Relationship Diagram (ERD)

## Database: BFASdatabase

---

## TABLES AND THEIR COLUMNS

### 1. Users
**Primary Key:** UserId (INT, IDENTITY)
- UserId
- Username (UNIQUE)
- PasswordHash
- FullName
- Email
- PhoneNumber
- Role
- IsActive
- CreatedAt
- LastLogin
- ProfilePicture (VARBINARY)
- ProfilePictureContentType
- ValidIdImage (VARBINARY)
- ValidIdContentType

### 2. EmployeeAccounts
**Primary Key:** EmployeeID (INT, IDENTITY)
**Foreign Key:** UserID → Users(UserId) (UNIQUE)
- EmployeeID
- UserID (UNIQUE, FK)
- EmployeeNumber (UNIQUE)
- FirstName
- MiddleName
- LastName
- Position
- Department
- HireDate
- Salary
- ContactNumber
- Email
- Address
- EmploymentStatus
- DateCreated
- DateModified

### 3. SuperAdminEmployees
**Primary Key:** EmployeeID (INT, IDENTITY)
- EmployeeID
- UserID
- EmployeeNumber
- FirstName
- MiddleName
- LastName
- Position
- Department
- HireDate
- Salary
- ContactNumber
- Email
- Address
- DateCreated
- DateModified

### 4. CustomerAccounts
**Primary Key:** AccountId (INT, IDENTITY)
**Foreign Key:** CustomerId → Users(UserId)
- AccountId
- CustomerId (FK)
- AccountNumber (UNIQUE)
- AccountType
- Balance
- InterestRate
- OpenedDate
- Status

### 5. SavingsAccounts
**Primary Key:** SavingsAccountId (INT, IDENTITY)
**Foreign Keys:** 
- CustomerId → Users(UserId)
- AccountTypeId → SavingsAccountTypes(TypeId)
- CreatedByEmployeeId → EmployeeAccounts(EmployeeID)
- ApprovedByEmployeeId → EmployeeAccounts(EmployeeID)
- ProcessedByEmployeeId → EmployeeAccounts(EmployeeID)
- ClosedByEmployeeId → EmployeeAccounts(EmployeeID)
- ModifiedByEmployeeId → EmployeeAccounts(EmployeeID)
- DormantByEmployeeId → EmployeeAccounts(EmployeeID)
- FrozenByEmployeeId → EmployeeAccounts(EmployeeID)
- ReactivatedByEmployeeId → EmployeeAccounts(EmployeeID)
- MaturedByEmployeeId → EmployeeAccounts(EmployeeID)
- UpdatedByEmployeeId → EmployeeAccounts(EmployeeID)
- AccountNumber (UNIQUE)
- Balance
- InterestRate
- Status
- CreatedAt
- UpdatedAt

### 6. SavingsAccountTypes
**Primary Key:** TypeId (INT, IDENTITY)
- TypeId
- TypeName (UNIQUE)
- Description
- MinimumBalance
- InterestRate
- WithdrawalLimit
- IsActive
- CreatedAt

### 7. SavingsTransactions
**Primary Key:** TransactionId (INT, IDENTITY)
**Foreign Keys:**
- SavingsAccountId → SavingsAccounts(SavingsAccountId)
- ProcessedByEmployeeId → EmployeeAccounts(EmployeeID)
- TransactionNumber (UNIQUE)
- TransactionType
- Amount
- BalanceBefore
- BalanceAfter
- Description
- Status
- ProcessedByEmployeeName
- CreatedAt

### 8. SavingsInterest
**Primary Key:** InterestId (INT, IDENTITY)
**Foreign Key:** SavingsAccountId → SavingsAccounts(SavingsAccountId)
- InterestId
- SavingsAccountId (FK)
- InterestAmount
- InterestRate
- CalculationDate
- PostedDate
- Status
- CreatedAt

### 9. SavingsInterestPostings
**Primary Key:** PostingId (INT, IDENTITY)
**Foreign Keys:**
- SavingsAccountId → SavingsAccounts(SavingsAccountId)
- PostedByEmployeeId → EmployeeAccounts(EmployeeID)
- PostingDate
- InterestAmount
- InterestRate
- BalanceBefore
- BalanceAfter
- PeriodStart
- PeriodEnd
- Status
- CreatedAt

### 10. SavingsWithdrawalRequests
**Primary Key:** RequestId (INT, IDENTITY)
**Foreign Keys:**
- SavingsAccountId → SavingsAccounts(SavingsAccountId)
- RequestedByEmployeeId → EmployeeAccounts(EmployeeID)
- ApprovedByEmployeeId → EmployeeAccounts(EmployeeID)
- ProcessedByEmployeeId → EmployeeAccounts(EmployeeID)
- RejectedByEmployeeId → EmployeeAccounts(EmployeeID)
- RequestDate
- Amount
- Status
- ApprovalDate
- ProcessedDate
- RejectionDate
- RejectionReason
- CreatedAt

### 11. CustomerTransactions
**Primary Key:** TransactionId (INT, IDENTITY)
**Foreign Key:** AccountId → CustomerAccounts(AccountId)
- TransactionId
- AccountId (FK)
- TransactionNumber (UNIQUE)
- TransactionType
- Amount
- BalanceBefore
- BalanceAfter
- Description
- TransactionDate
- Status
- ProcessedBy
- ApprovedBy
- ReferenceNumber
- Channel
- Location
- DeviceInfo
- RecipientAccount

### 12. CustomerLoans
**Primary Key:** LoanId (INT, IDENTITY)
**Foreign Key:** CustomerId → Users(UserId)
- LoanId
- CustomerId (FK)
- LoanNumber (UNIQUE)
- LoanType
- PrincipalAmount
- InterestRate
- TermMonths
- MonthlyPayment
- TotalAmountDue
- OutstandingBalance
- Status
- ApplicationDate
- ApprovalDate
- DisbursementDate
- MaturityDate
- CollateralDescription
- Guarantor
- CreditScore
- ApprovedBy
- ProcessedBy
- LastPaymentDate
- NextPaymentDate
- IsBlacklisted
- BlacklistedDate
- BlacklistedReason

### 13. LoanApplications
**Primary Key:** ApplicationId (INT, IDENTITY)
**Foreign Key:** CustomerId → Users(UserId)
- ApplicationId
- CustomerId (FK)
- ApplicationNumber (UNIQUE)
- LoanType
- RequestedAmount
- Purpose
- EmploymentInfo
- MonthlyIncome
- Status
- ApplicationDate
- ReviewedDate
- ApprovedDate
- RejectedDate
- ReviewedBy
- ApprovedBy
- RejectedBy

### 14. LoanAssessments
**Primary Key:** AssessmentId (INT, IDENTITY)
**Foreign Key:** ApplicationId → LoanApplications(ApplicationId)
- AssessmentId
- ApplicationId (FK)
- CreditScore
- DebtToIncomeRatio
- CollateralValue
- RecommendedAmount
- RecommendedTermMonths
- AssessmentNotes
- AssessedBy
- AssessedDate
- Status
- ApprovedDate
- RejectedDate
- ApprovedBy
- RejectedBy

### 15. LoanApprovals
**Primary Key:** ApprovalId (INT, IDENTITY)
**Foreign Key:** ApplicationId → LoanApplications(ApplicationId)
- ApprovalId
- ApplicationId (FK)
- ApprovedAmount
- ApprovedTermMonths
- InterestRate
- MonthlyPayment
- ApprovalLevel
- ApprovedBy
- ApprovalDate
- ApprovalNotes
- Status
- DeclinationReason

### 16. LoanDisbursals
**Primary Key:** DisbursalId (INT, IDENTITY)
**Foreign Key:** LoanId → CustomerLoans(LoanId)
- DisbursalId
- LoanId (FK)
- DisbursedAmount
- DisbursementDate
- DisbursementMethod
- DisbursedBy
- AccountNumber
- Reference

### 17. LoanPayments
**Primary Key:** PaymentId (INT, IDENTITY)
**Foreign Key:** LoanId → CustomerLoans(LoanId)
- PaymentId
- LoanId (FK)
- PaymentAmount
- PaymentDate
- PaymentMethod
- PrincipalPaid
- InterestPaid
- PenaltyPaid
- ProcessedBy
- IsLatePayment

### 18. LoanPaymentSchedules
**Primary Key:** ScheduleId (INT, IDENTITY)
**Foreign Key:** LoanId → CustomerLoans(LoanId)
- ScheduleId
- LoanId (FK)
- InstallmentNumber
- DueDate
- PrincipalAmount
- InterestAmount
- TotalAmount
- Status
- PaidDate
- Penalty

### 19. LoanInvoices
**Primary Key:** InvoiceId (INT, IDENTITY)
**Foreign Key:** LoanId → CustomerLoans(LoanId)
- InvoiceId
- LoanId (FK)
- InvoiceNumber
- BillingPeriodStart
- BillingPeriodEnd
- PrincipalDue
- InterestDue
- PenaltyDue
- TotalDue
- AmountPaid
- Balance
- Status
- GeneratedDate
- DueDate
- PaidDate
- GeneratedBy
- PaymentReference
- Notes

### 20. LoanTransactionHistory
**Primary Key:** HistoryId (INT, IDENTITY)
**Foreign Key:** LoanId → CustomerLoans(LoanId)
- HistoryId
- LoanId (FK)
- TransactionType
- TransactionDate
- Amount
- PrincipalAmount
- InterestAmount
- PenaltyAmount
- BalanceBefore
- BalanceAfter
- ProcessedBy
- ReferenceNumber
- Notes
- Status

### 21. LoanViolations
**Primary Key:** ViolationId (INT, IDENTITY)
**Foreign Key:** LoanId → CustomerLoans(LoanId)
- ViolationId
- LoanId (FK)
- ViolationType
- ViolationDate
- Description
- PenaltyAmount
- Status
- ResolvedDate
- ResolvedBy
- Resolution
- Remarks

### 22. LoanManagement
**Primary Key:** LoanMgmtId (INT, IDENTITY)
**Foreign Key:** LoanId → CustomerLoans(LoanId)
- LoanMgmtId
- LoanId (FK)
- ActionType
- ActionDate
- ActionBy
- PreviousStatus
- NewStatus
- Notes
- DueDate
- AmountDue
- AmountPaid
- PaymentStatus

### 23. CustomerCards
**Primary Key:** CardId (INT, IDENTITY)
**Foreign Keys:**
- CustomerId → Users(UserId)
- AccountId → CustomerAccounts(AccountId)
- CardId
- CustomerId (FK)
- AccountId (FK)
- CardNumber (UNIQUE)
- CardType
- ExpiryDate
- CVV
- Status
- IssuedDate
- ActivatedDate
- BlockedDate
- CreditLimit
- AvailableCredit
- CreatedAt

### 24. CardManagement
**Primary Key:** CardMgmtId (INT, IDENTITY)
**Foreign Key:** CardId → CustomerCards(CardId)
- CardMgmtId
- CardId (FK)
- ActionType
- ActionBy
- ActionReason
- ActionAt

### 25. CustomerSavingsGoals
**Primary Key:** GoalId (INT, IDENTITY)
**Foreign Key:** CustomerId → Users(UserId)
- GoalId
- CustomerId (FK)
- GoalName
- TargetAmount
- CurrentAmount
- Deadline
- Status
- CreatedBy
- CreatedAt

### 26. CustomerRewardPoints
**Primary Key:** RewardId (INT, IDENTITY)
**Foreign Key:** CustomerId → Users(UserId)
- RewardId
- CustomerId (FK)
- Points
- EarnedFrom
- EarnedDate
- ExpiryDate
- Status
- RedeemedPoints
- RedeemedFor
- RedeemedAt

### 27. BankAccounts
**Primary Key:** BankAccountId (INT, IDENTITY)
- BankAccountId
- AccountNumber (UNIQUE)
- AccountName
- BankName
- AccountType
- Balance
- Currency
- Status
- CreatedAt
- LastModifiedAt
- LastModifiedBy

### 28. FundTransfers
**Primary Key:** TransferId (INT, IDENTITY)
**Foreign Keys:**
- FromAccountId → BankAccounts(BankAccountId)
- ToAccountId → BankAccounts(BankAccountId)
- TransferId
- TransferNumber (UNIQUE)
- FromAccountId (FK)
- ToAccountId (FK)
- Amount
- TransferDate
- Status
- InitiatedBy
- ApprovedBy
- Description
- ReferenceNumber
- ApprovedAt

### 29. ChartOfAccounts
**Primary Key:** AccountId (INT, IDENTITY)
- AccountId
- AccountCode (UNIQUE)
- AccountName
- AccountType
- ParentAccountId
- Balance
- IsActive
- CreatedAt
- CreatedBy

### 30. GeneralLedger
**Primary Key:** LedgerId (INT, IDENTITY)
**Foreign Key:** AccountId → ChartOfAccounts(AccountId)
- LedgerId
- AccountId (FK)
- TransactionDate
- DebitAmount
- CreditAmount
- Balance
- Description
- CreatedAt

### 31. GeneralLedgerTransactions
**Primary Key:** GLTransactionId (INT, IDENTITY)
**Foreign Key:** AccountId → ChartOfAccounts(AccountId)
- GLTransactionId
- AccountId (FK)
- TransactionDate
- TransactionType
- DebitAmount
- CreditAmount
- RunningBalance

### 32. JournalEntries
**Primary Key:** JournalId (INT, IDENTITY)
- JournalId
- JournalNumber (UNIQUE)
- EntryDate
- Description
- TotalDebit
- TotalCredit
- Status
- CreatedBy
- CreatedAt
- PostedDate
- PostedBy

### 33. JournalEntryLines
**Primary Key:** LineId (INT, IDENTITY)
**Foreign Keys:**
- JournalId → JournalEntries(JournalId)
- AccountId → ChartOfAccounts(AccountId)
- LineId
- JournalId (FK)
- AccountId (FK)
- Description
- DebitAmount
- CreditAmount

### 34. AccountingEntries
**Primary Key:** EntryId (INT, IDENTITY)
**Foreign Key:** AccountId → ChartOfAccounts(AccountId)
- EntryId
- AccountId (FK)
- EntryDate
- TransactionType
- DebitAmount
- CreditAmount
- Balance
- Description
- ReferenceNumber
- CreatedBy
- CreatedAt

### 35. AccountsPayable
**Primary Key:** PayableId (INT, IDENTITY)
- PayableId
- VendorName
- InvoiceNumber
- InvoiceDate
- DueDate
- Amount
- AmountPaid
- Balance
- Status
- Description
- CreatedAt
- CreatedBy

### 36. AccountsReceivable
**Primary Key:** ReceivableId (INT, IDENTITY)
- ReceivableId
- CustomerName
- InvoiceNumber
- InvoiceDate
- DueDate
- Amount
- AmountReceived
- Balance
- Status
- Description
- CreatedAt
- CreatedBy
- DiscountAmount
- TaxAmount

### 37. Invoices
**Primary Key:** InvoiceId (INT, IDENTITY)
**Foreign Key:** CustomerId → Users(UserId)
- InvoiceId
- CustomerId (FK)
- InvoiceNumber (UNIQUE)
- InvoiceDate
- DueDate
- TotalAmount
- PaidAmount
- Balance
- Status
- Items
- TaxAmount
- DiscountAmount
- CreatedBy
- CreatedAt
- PaidDate
- PaymentMethod
- DownloadedAt

### 38. BudgetManagement
**Primary Key:** BudgetId (INT, IDENTITY)
- BudgetId
- BudgetName
- Category
- AllocatedAmount
- SpentAmount
- RemainingAmount
- StartDate
- EndDate
- Status
- Description
- CreatedAt
- CreatedBy

### 39. CashflowAnalysis
**Primary Key:** CashflowId (INT, IDENTITY)
- CashflowId
- AnalysisDate
- CashInflow
- CashOutflow
- NetCashflow
- Notes
- CreatedAt
- CreatedBy

### 40. FinancialStatements
**Primary Key:** StatementId (INT, IDENTITY)
- StatementId
- StatementType
- PeriodStart
- PeriodEnd
- GeneratedDate
- Content
- GeneratedBy

### 41. FinancialForecasting
**Primary Key:** ForecastId (INT, IDENTITY)
- ForecastId
- ForecastType
- ForecastPeriod
- StartDate
- EndDate
- PredictedRevenue
- PredictedExpenses
- PredictedProfit
- Notes
- CreatedAt
- CreatedBy

### 42. BillersManagement
**Primary Key:** BillerId (INT, IDENTITY)
- BillerId
- BillerCode (UNIQUE)
- BillerName
- Category
- ServiceFee
- IsActive
- CreatedAt
- CreatedBy

### 43. BankingReports
**Primary Key:** ReportId (INT, IDENTITY)
- ReportId
- ReportType
- ReportDate
- StartDate
- EndDate
- ReportData
- GeneratedAt
- GeneratedBy

### 44. AuditLogs
**Primary Key:** AuditId (INT, IDENTITY)
- AuditId
- UserId
- Action
- TableName
- RecordId
- Timestamp
- Details
- IpAddress
- UserAgent
- OldValues
- NewValues
- Amount
- BalanceBefore
- BalanceAfter

### 45. LoginHistory
**Primary Key:** LoginId (INT, IDENTITY)
**Foreign Key:** UserId → Users(UserId)
- LoginId
- UserId (FK)
- LoginTime
- LogoutTime
- IpAddress
- DeviceInfo
- Status
- FailureReason

### 46. ApprovalQueue
**Primary Key:** ApprovalId (INT, IDENTITY)
- ApprovalId
- RequestType
- RequestId
- RequestedBy
- RequestedAt
- Status
- ApprovedBy
- ApprovedAt
- RejectedBy
- RejectedAt
- ApprovalNotes

### 47. RolePermissions
**Primary Key:** PermissionId (INT, IDENTITY)
- PermissionId
- RoleName
- PermissionName
- IsActive

### 48. Employees
**Primary Key:** EmployeeId (INT, IDENTITY)
- EmployeeId
- FullName
- Email
- PhoneNumber
- Department
- Position
- HireDate
- Salary
- Status
- CreatedAt
- UpdatedAt

### 49. Seeders
**Primary Key:** SeedID (INT, IDENTITY)
- SeedID
- SeederName
- ExecutedAt
- CreatedAt

### 50. vw_ActiveEmployees (VIEW)
- EmployeeID
- UserID
- EmployeeNumber
- FirstName
- MiddleName
- LastName
- FullName
- Position
- Department
- Role
- IsActive

---

## RELATIONSHIPS

### Core User & Authentication
1. **Users → EmployeeAccounts** (1:1)
   - Users.UserId → EmployeeAccounts.UserID (UNIQUE)

2. **Users → LoginHistory** (1:Many)
   - Users.UserId → LoginHistory.UserId

3. **Users → CustomerAccounts** (1:Many)
   - Users.UserId → CustomerAccounts.CustomerId

### Savings Module
4. **Users → SavingsAccounts** (1:Many)
   - Users.UserId → SavingsAccounts.CustomerId

5. **SavingsAccountTypes → SavingsAccounts** (1:Many)
   - SavingsAccountTypes.TypeId → SavingsAccounts.AccountTypeId

6. **SavingsAccounts → SavingsTransactions** (1:Many)
   - SavingsAccounts.SavingsAccountId → SavingsTransactions.SavingsAccountId

7. **SavingsAccounts → SavingsInterest** (1:Many)
   - SavingsAccounts.SavingsAccountId → SavingsInterest.SavingsAccountId

8. **SavingsAccounts → SavingsInterestPostings** (1:Many)
   - SavingsAccounts.SavingsAccountId → SavingsInterestPostings.SavingsAccountId

9. **SavingsAccounts → SavingsWithdrawalRequests** (1:Many)
   - SavingsAccounts.SavingsAccountId → SavingsWithdrawalRequests.SavingsAccountId

10. **EmployeeAccounts → SavingsAccounts** (1:Many) - CreatedBy
    - EmployeeAccounts.EmployeeID → SavingsAccounts.CreatedByEmployeeId

11. **EmployeeAccounts → SavingsAccounts** (1:Many) - ApprovedBy
    - EmployeeAccounts.EmployeeID → SavingsAccounts.ApprovedByEmployeeId

12. **EmployeeAccounts → SavingsAccounts** (1:Many) - ProcessedBy
    - EmployeeAccounts.EmployeeID → SavingsAccounts.ProcessedByEmployeeId

13. **EmployeeAccounts → SavingsTransactions** (1:Many)
    - EmployeeAccounts.EmployeeID → SavingsTransactions.ProcessedByEmployeeId

14. **EmployeeAccounts → SavingsInterestPostings** (1:Many)
    - EmployeeAccounts.EmployeeID → SavingsInterestPostings.PostedByEmployeeId

15. **EmployeeAccounts → SavingsWithdrawalRequests** (1:Many) - Requested
    - EmployeeAccounts.EmployeeID → SavingsWithdrawalRequests.RequestedByEmployeeId

16. **EmployeeAccounts → SavingsWithdrawalRequests** (1:Many) - Approved
    - EmployeeAccounts.EmployeeID → SavingsWithdrawalRequests.ApprovedByEmployeeId

17. **EmployeeAccounts → SavingsWithdrawalRequests** (1:Many) - Processed
    - EmployeeAccounts.EmployeeID → SavingsWithdrawalRequests.ProcessedByEmployeeId

### Customer Transactions & Accounts
18. **CustomerAccounts → CustomerTransactions** (1:Many)
    - CustomerAccounts.AccountId → CustomerTransactions.AccountId

19. **Users → CustomerCards** (1:Many)
    - Users.UserId → CustomerCards.CustomerId

20. **CustomerAccounts → CustomerCards** (1:Many)
    - CustomerAccounts.AccountId → CustomerCards.AccountId

21. **CustomerCards → CardManagement** (1:Many)
    - CustomerCards.CardId → CardManagement.CardId

22. **Users → CustomerSavingsGoals** (1:Many)
    - Users.UserId → CustomerSavingsGoals.CustomerId

23. **Users → CustomerRewardPoints** (1:Many)
    - Users.UserId → CustomerRewardPoints.CustomerId

### Loan Module
24. **Users → CustomerLoans** (1:Many)
    - Users.UserId → CustomerLoans.CustomerId

25. **Users → LoanApplications** (1:Many)
    - Users.UserId → LoanApplications.CustomerId

26. **LoanApplications → LoanAssessments** (1:Many)
    - LoanApplications.ApplicationId → LoanAssessments.ApplicationId

27. **LoanApplications → LoanApprovals** (1:Many)
    - LoanApplications.ApplicationId → LoanApprovals.ApplicationId

28. **CustomerLoans → LoanDisbursals** (1:Many)
    - CustomerLoans.LoanId → LoanDisbursals.LoanId

29. **CustomerLoans → LoanPayments** (1:Many)
    - CustomerLoans.LoanId → LoanPayments.LoanId

30. **CustomerLoans → LoanPaymentSchedules** (1:Many)
    - CustomerLoans.LoanId → LoanPaymentSchedules.LoanId

31. **CustomerLoans → LoanInvoices** (1:Many)
    - CustomerLoans.LoanId → LoanInvoices.LoanId

32. **CustomerLoans → LoanTransactionHistory** (1:Many)
    - CustomerLoans.LoanId → LoanTransactionHistory.LoanId

33. **CustomerLoans → LoanViolations** (1:Many)
    - CustomerLoans.LoanId → LoanViolations.LoanId

34. **CustomerLoans → LoanManagement** (1:Many)
    - CustomerLoans.LoanId → LoanManagement.LoanId

### Accounting & Financial
35. **ChartOfAccounts → GeneralLedger** (1:Many)
    - ChartOfAccounts.AccountId → GeneralLedger.AccountId

36. **ChartOfAccounts → GeneralLedgerTransactions** (1:Many)
    - ChartOfAccounts.AccountId → GeneralLedgerTransactions.AccountId

37. **ChartOfAccounts → AccountingEntries** (1:Many)
    - ChartOfAccounts.AccountId → AccountingEntries.AccountId

38. **JournalEntries → JournalEntryLines** (1:Many)
    - JournalEntries.JournalId → JournalEntryLines.JournalId

39. **ChartOfAccounts → JournalEntryLines** (1:Many)
    - ChartOfAccounts.AccountId → JournalEntryLines.AccountId

40. **Users → Invoices** (1:Many)
    - Users.UserId → Invoices.CustomerId

### Banking Operations
41. **BankAccounts → FundTransfers** (1:Many) - From
    - BankAccounts.BankAccountId → FundTransfers.FromAccountId

42. **BankAccounts → FundTransfers** (1:Many) - To
    - BankAccounts.BankAccountId → FundTransfers.ToAccountId

---

## ENTITY CATEGORIES

### 1. USER MANAGEMENT
- Users
- EmployeeAccounts
- SuperAdminEmployees
- LoginHistory
- RolePermissions
- vw_ActiveEmployees

### 2. CUSTOMER BANKING
- CustomerAccounts
- CustomerTransactions
- CustomerCards
- CardManagement
- CustomerSavingsGoals
- CustomerRewardPoints

### 3. SAVINGS SYSTEM
- SavingsAccounts
- SavingsAccountTypes
- SavingsTransactions
- SavingsInterest
- SavingsInterestPostings
- SavingsWithdrawalRequests

### 4. LOAN MANAGEMENT
- CustomerLoans
- LoanApplications
- LoanAssessments
- LoanApprovals
- LoanDisbursals
- LoanPayments
- LoanPaymentSchedules
- LoanInvoices
- LoanTransactionHistory
- LoanViolations
- LoanManagement

### 5. ACCOUNTING & FINANCE
- ChartOfAccounts
- GeneralLedger
- GeneralLedgerTransactions
- JournalEntries
- JournalEntryLines
- AccountingEntries
- AccountsPayable
- AccountsReceivable
- Invoices

### 6. FINANCIAL PLANNING
- BudgetManagement
- CashflowAnalysis
- FinancialStatements
- FinancialForecasting

### 7. BANKING OPERATIONS
- BankAccounts
- FundTransfers
- BillersManagement
- BankingReports

### 8. SYSTEM & AUDIT
- AuditLogs
- ApprovalQueue
- Employees
- Seeders

---

## DRAW.IO CREATION GUIDE

### Step 1: Create Tables
1. Use **Entity** shapes for each table
2. Add table name as header
3. List all columns with data types
4. Mark Primary Keys with 🔑
5. Mark Foreign Keys with 🔗

### Step 2: Add Relationships
Draw connectors between tables using these notations:
- **1:1** - One solid line with "1" on both ends
- **1:Many** - One solid line with "1" on one end and "∞" or "crow's foot" on the other
- **Many:Many** - Crow's foot on both ends (if any junction tables exist)

### Step 3: Color Coding Suggestion
- **Blue**: User Management tables
- **Green**: Customer Banking tables
- **Orange**: Savings System tables
- **Purple**: Loan Management tables
- **Red**: Accounting & Finance tables
- **Yellow**: Financial Planning tables
- **Cyan**: Banking Operations tables
- **Gray**: System & Audit tables

### Step 4: Layout
Organize tables by category in sections:
- Top Left: User Management
- Top Center: Customer Banking
- Top Right: Savings System
- Middle Left: Loan Management
- Middle Center: Accounting & Finance
- Middle Right: Financial Planning
- Bottom: Banking Operations and System/Audit

---

## KEY NOTES FOR ERD

1. **Complex Foreign Key Relationships**: SavingsAccounts has multiple FK relationships to EmployeeAccounts for different actions (Created, Approved, Processed, Closed, Modified, etc.)

2. **Self-Referencing**: ChartOfAccounts has ParentAccountId which references itself

3. **Unique Constraints**: Many tables have unique constraints on number fields (AccountNumber, TransactionNumber, LoanNumber, etc.)

4. **Status Fields**: Most transactional tables include Status fields for workflow management

5. **Audit Trail**: Most tables include CreatedAt, CreatedBy, UpdatedAt tracking fields

6. **BLOB Fields**: Users table stores images (ProfilePicture, ValidIdImage) as VARBINARY

7. **Financial Precision**: All monetary amounts use DECIMAL(18,2) for precision

---

**Total Tables:** 50 (49 physical tables + 1 view)
**Total Relationships:** 42+ foreign key relationships

This comprehensive ERD structure represents a full-featured banking and financial management system with integrated loan processing, savings accounts, accounting, and customer management capabilities.
