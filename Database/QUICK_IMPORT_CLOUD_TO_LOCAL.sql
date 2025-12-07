-- =============================================
-- QUICK DATA IMPORT: CLOUD TO LOCAL
-- =============================================
-- 
-- Instructions:
-- 1. Connect SSMS to CLOUD: db34283.public.databaseasp.net,1433 -> db34283
-- 2. Run this script to get INSERT statements
-- 3. Copy the results
-- 4. Connect SSMS to LOCAL: localhost\SQLEXPRESS -> BFASdatabase
-- 5. Paste and run the INSERT statements
--
-- =============================================

USE db34283;
GO

-- =============================================
-- TABLE 1: USERS (Run first - parent table)
-- =============================================
PRINT '-- ========== USERS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N''' + REPLACE(Username, '''', '''''') + ''') ' +
    'INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, PhoneNumber, IsActive, CreatedAt) VALUES (' +
    'N''' + REPLACE(Username, '''', '''''') + ''', ' +
    'N''' + REPLACE(PasswordHash, '''', '''''') + ''', ' +
    'N''' + Role + ''', ' +
    ISNULL('N''' + REPLACE(FullName, '''', '''''') + '''', 'NULL') + ', ' +
    ISNULL('N''' + REPLACE(Email, '''', '''''') + '''', 'NULL') + ', ' +
    ISNULL('N''' + PhoneNumber + '''', 'NULL') + ', ' +
    CAST(IsActive AS VARCHAR(1)) + ', ' +
    '''' + CONVERT(VARCHAR, CreatedAt, 120) + ''');'
FROM Users
ORDER BY UserId;
GO

-- =============================================
-- TABLE 2: EMPLOYEE ACCOUNTS
-- =============================================
PRINT '-- ========== EMPLOYEE ACCOUNTS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM EmployeeAccounts WHERE EmployeeNumber = N''' + EmployeeNumber + ''') ' +
    'INSERT INTO EmployeeAccounts (UserID, FirstName, LastName, EmployeeNumber, Department, Position, HireDate, EmploymentStatus, DateModified) VALUES (' +
    CAST(UserID AS VARCHAR) + ', ' +
    'N''' + REPLACE(FirstName, '''', '''''') + ''', ' +
    'N''' + REPLACE(LastName, '''', '''''') + ''', ' +
    'N''' + EmployeeNumber + ''', ' +
    'N''' + Department + ''', ' +
    'N''' + Position + ''', ' +
    '''' + CONVERT(VARCHAR, HireDate, 120) + ''', ' +
    'N''' + EmploymentStatus + ''', ' +
    '''' + CONVERT(VARCHAR, DateModified, 120) + ''');'
FROM EmployeeAccounts
ORDER BY EmployeeID;
GO

-- =============================================
-- TABLE 3: CUSTOMER ACCOUNTS
-- =============================================
PRINT '-- ========== CUSTOMER ACCOUNTS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM CustomerAccounts WHERE AccountNumber = N''' + AccountNumber + ''') ' +
    'INSERT INTO CustomerAccounts (CustomerId, AccountNumber, Balance, AvailableBalance, Currency, IsActive, CreatedAt, Status) VALUES (' +
    CAST(CustomerId AS VARCHAR) + ', ' +
    'N''' + AccountNumber + ''', ' +
    CAST(Balance AS VARCHAR) + ', ' +
    CAST(AvailableBalance AS VARCHAR) + ', ' +
    'N''' + Currency + ''', ' +
    CAST(IsActive AS VARCHAR(1)) + ', ' +
    '''' + CONVERT(VARCHAR, CreatedAt, 120) + ''', ' +
    'N''' + Status + ''');'
FROM CustomerAccounts
ORDER BY AccountId;
GO

-- =============================================
-- TABLE 4: CUSTOMER TRANSACTIONS (Recent 30 days)
-- =============================================
PRINT '-- ========== CUSTOMER TRANSACTIONS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM CustomerTransactions WHERE TransactionNumber = N''' + TransactionNumber + ''') ' +
    'INSERT INTO CustomerTransactions (TransactionNumber, AccountId, TransactionType, Amount, Fee, Status, Description, CreatedAt) VALUES (' +
    'N''' + TransactionNumber + ''', ' +
    CAST(AccountId AS VARCHAR) + ', ' +
    'N''' + TransactionType + ''', ' +
    CAST(Amount AS VARCHAR) + ', ' +
    CAST(Fee AS VARCHAR) + ', ' +
    'N''' + Status + ''', ' +
    ISNULL('N''' + REPLACE(Description, '''', '''''') + '''', 'NULL') + ', ' +
    '''' + CONVERT(VARCHAR, CreatedAt, 120) + ''');'
FROM CustomerTransactions
WHERE CreatedAt >= DATEADD(DAY, -30, GETDATE())
ORDER BY TransactionId;
GO

-- =============================================
-- TABLE 5: LOAN APPLICATIONS
-- =============================================
PRINT '-- ========== LOAN APPLICATIONS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM LoanApplications WHERE ApplicationNumber = N''' + ApplicationNumber + ''') ' +
    'INSERT INTO LoanApplications (ApplicationNumber, AccountId, LoanType, RequestedAmount, Purpose, Status, ApplicationDate, SubmittedBy) VALUES (' +
    'N''' + ApplicationNumber + ''', ' +
    CAST(AccountId AS VARCHAR) + ', ' +
    'N''' + LoanType + ''', ' +
    CAST(RequestedAmount AS VARCHAR) + ', ' +
    ISNULL('N''' + REPLACE(Purpose, '''', '''''') + '''', 'NULL') + ', ' +
    'N''' + Status + ''', ' +
    '''' + CONVERT(VARCHAR, ApplicationDate, 120) + ''', ' +
    'N''' + SubmittedBy + ''');'
FROM LoanApplications
ORDER BY ApplicationId;
GO

-- =============================================
-- TABLE 6: LOAN ASSESSMENTS
-- =============================================
PRINT '-- ========== LOAN ASSESSMENTS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM LoanAssessments WHERE ApplicationId = ' + CAST(ApplicationId AS VARCHAR) + ') ' +
    'INSERT INTO LoanAssessments (ApplicationId, AccountId, LoanAmount, InterestRate, TermMonths, ComputedMonthlyPayment, TotalPayable, AssessmentDate, AssessmentStatus, AssessedBy) VALUES (' +
    CAST(ApplicationId AS VARCHAR) + ', ' +
    CAST(AccountId AS VARCHAR) + ', ' +
    CAST(LoanAmount AS VARCHAR) + ', ' +
    CAST(InterestRate AS VARCHAR) + ', ' +
    CAST(TermMonths AS VARCHAR) + ', ' +
    CAST(ComputedMonthlyPayment AS VARCHAR) + ', ' +
    CAST(TotalPayable AS VARCHAR) + ', ' +
    '''' + CONVERT(VARCHAR, AssessmentDate, 120) + ''', ' +
    'N''' + AssessmentStatus + ''', ' +
    'N''' + AssessedBy + ''');'
FROM LoanAssessments
ORDER BY AssessmentId;
GO

-- =============================================
-- TABLE 7: LOAN APPROVALS
-- =============================================
PRINT '-- ========== LOAN APPROVALS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM LoanApprovals WHERE ApplicationId = ' + CAST(ApplicationId AS VARCHAR) + ') ' +
    'INSERT INTO LoanApprovals (AssessmentId, ApplicationId, AccountId, ApprovalStatus, ApprovalDate, ApprovedBy, ApprovedAmount, ApprovedInterestRate, ApprovedTermMonths) VALUES (' +
    CAST(AssessmentId AS VARCHAR) + ', ' +
    CAST(ApplicationId AS VARCHAR) + ', ' +
    CAST(AccountId AS VARCHAR) + ', ' +
    'N''' + ApprovalStatus + ''', ' +
    '''' + CONVERT(VARCHAR, ApprovalDate, 120) + ''', ' +
    'N''' + ApprovedBy + ''', ' +
    ISNULL(CAST(ApprovedAmount AS VARCHAR), 'NULL') + ', ' +
    ISNULL(CAST(ApprovedInterestRate AS VARCHAR), 'NULL') + ', ' +
    ISNULL(CAST(ApprovedTermMonths AS VARCHAR), 'NULL') + ');'
FROM LoanApprovals
ORDER BY ApprovalId;
GO

-- =============================================
-- TABLE 8: CUSTOMER LOANS
-- =============================================
PRINT '-- ========== CUSTOMER LOANS =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM CustomerLoans WHERE LoanNumber = N''' + LoanNumber + ''') ' +
    'INSERT INTO CustomerLoans (AccountId, LoanNumber, LoanType, LoanAmount, OutstandingBalance, InterestRate, TermMonths, MonthlyPayment, StartDate, EndDate, Status, CreatedAt) VALUES (' +
    CAST(AccountId AS VARCHAR) + ', ' +
    'N''' + LoanNumber + ''', ' +
    'N''' + LoanType + ''', ' +
    CAST(LoanAmount AS VARCHAR) + ', ' +
    CAST(OutstandingBalance AS VARCHAR) + ', ' +
    CAST(InterestRate AS VARCHAR) + ', ' +
    CAST(TermMonths AS VARCHAR) + ', ' +
    CAST(MonthlyPayment AS VARCHAR) + ', ' +
    '''' + CONVERT(VARCHAR, StartDate, 120) + ''', ' +
    '''' + CONVERT(VARCHAR, EndDate, 120) + ''', ' +
    'N''' + Status + ''', ' +
    '''' + CONVERT(VARCHAR, CreatedAt, 120) + ''');'
FROM CustomerLoans
ORDER BY LoanId;
GO

-- =============================================
-- TABLE 9: LOAN PAYMENT SCHEDULES
-- =============================================
PRINT '-- ========== LOAN PAYMENT SCHEDULES =========='
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM LoanPaymentSchedules WHERE LoanId = ' + CAST(LoanId AS VARCHAR) + ' AND PaymentNumber = ' + CAST(PaymentNumber AS VARCHAR) + ') ' +
    'INSERT INTO LoanPaymentSchedules (LoanId, PaymentNumber, DueDate, MinimumPayment, PrincipalAmount, InterestAmount, PaymentStatus) VALUES (' +
    CAST(LoanId AS VARCHAR) + ', ' +
    CAST(PaymentNumber AS VARCHAR) + ', ' +
    '''' + CONVERT(VARCHAR, DueDate, 120) + ''', ' +
    CAST(MinimumPayment AS VARCHAR) + ', ' +
    CAST(PrincipalAmount AS VARCHAR) + ', ' +
    CAST(InterestAmount AS VARCHAR) + ', ' +
    'N''' + PaymentStatus + ''');'
FROM LoanPaymentSchedules
ORDER BY ScheduleId;
GO

-- =============================================
-- TABLE 10: LOAN PAYMENTS
-- =============================================
PRINT '-- ========== LOAN PAYMENTS =========='
SELECT 
    'INSERT INTO LoanPayments (ScheduleId, LoanId, PaymentAmount, PaymentDate, PaymentMethod, ProcessedBy) VALUES (' +
    CAST(ScheduleId AS VARCHAR) + ', ' +
    CAST(LoanId AS VARCHAR) + ', ' +
    CAST(PaymentAmount AS VARCHAR) + ', ' +
    '''' + CONVERT(VARCHAR, PaymentDate, 120) + ''', ' +
    'N''' + PaymentMethod + ''', ' +
    'N''' + ProcessedBy + ''');'
FROM LoanPayments
ORDER BY PaymentId;
GO

PRINT ''
PRINT '-- =========================================='
PRINT '-- SYNC COMPLETE'
PRINT '-- Copy all statements above and run on LOCAL'
PRINT '-- =========================================='
