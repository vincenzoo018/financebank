-- ============================================
-- POPULATE GENERAL LEDGER FROM EXISTING TRANSACTIONS
-- This script creates GL entries from all existing transaction data
-- Run AFTER running GL_ACCOUNTS_MIGRATION.sql
-- ============================================

-- First, ensure required accounts exist in GeneralLedger table
INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '1010', 'Cash on Hand', 'Asset', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '1010');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '1110', 'Loans Receivable', 'Asset', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '1110');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '1200', 'Accounts Receivable', 'Asset', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '1200');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '2000', 'Accounts Payable', 'Liability', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '2000');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '2010', 'Customer Deposits', 'Liability', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '2010');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '2101', 'Savings Deposits', 'Liability', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '2101');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '4010', 'Interest Income', 'Revenue', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '4010');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '4020', 'Service Fee Income', 'Revenue', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '4020');

INSERT INTO GeneralLedger (AccountCode, AccountName, AccountType, Balance, IsActive)
SELECT '4030', 'Penalty Income', 'Revenue', 0, 1
WHERE NOT EXISTS (SELECT 1 FROM GeneralLedger WHERE AccountCode = '4030');

PRINT 'GL Accounts created/verified';

-- ============================================
-- CREATE GL TRANSACTIONS FROM CUSTOMER TRANSACTIONS
-- ============================================

-- DEPOSITS: Debit Cash (1010), Credit Customer Deposits (2010)
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand
    ISNULL(ProcessedAt, CreatedAt),
    Amount, -- Debit
    0, -- Credit
    'Deposit - ' + ISNULL(TransactionNumber, 'TXN' + CAST(TransactionId AS VARCHAR)),
    0
FROM CustomerTransactions 
WHERE TransactionType = 'Deposit' AND Status = 'Completed';

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '2010', -- Customer Deposits
    ISNULL(ProcessedAt, CreatedAt),
    0, -- Debit
    Amount, -- Credit
    'Deposit liability - ' + ISNULL(TransactionNumber, 'TXN' + CAST(TransactionId AS VARCHAR)),
    0
FROM CustomerTransactions 
WHERE TransactionType = 'Deposit' AND Status = 'Completed';

PRINT 'Deposit GL entries created';

-- WITHDRAWALS: Debit Customer Deposits (2010), Credit Cash (1010)
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '2010', -- Customer Deposits
    ISNULL(ProcessedAt, CreatedAt),
    Amount, -- Debit
    0, -- Credit
    'Withdrawal - ' + ISNULL(TransactionNumber, 'TXN' + CAST(TransactionId AS VARCHAR)),
    0
FROM CustomerTransactions 
WHERE TransactionType = 'Withdrawal' AND Status = 'Completed';

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand
    ISNULL(ProcessedAt, CreatedAt),
    0, -- Debit
    Amount, -- Credit
    'Cash disbursed - ' + ISNULL(TransactionNumber, 'TXN' + CAST(TransactionId AS VARCHAR)),
    0
FROM CustomerTransactions 
WHERE TransactionType = 'Withdrawal' AND Status = 'Completed';

PRINT 'Withdrawal GL entries created';

-- BILL PAYMENTS: Debit Customer Deposits (2010), Credit Cash (1010)
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '2010', -- Customer Deposits
    ISNULL(ProcessedAt, CreatedAt),
    Amount, -- Debit
    0, -- Credit
    'Bills Payment - ' + ISNULL(BillerName, 'Biller'),
    0
FROM CustomerTransactions 
WHERE TransactionType IN ('Bills', 'BillsPayment') AND Status = 'Completed';

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand
    ISNULL(ProcessedAt, CreatedAt),
    0, -- Debit
    Amount, -- Credit
    'Cash paid to biller - ' + ISNULL(BillerName, 'Biller'),
    0
FROM CustomerTransactions 
WHERE TransactionType IN ('Bills', 'BillsPayment') AND Status = 'Completed';

PRINT 'Bills Payment GL entries created';

-- ============================================
-- CREATE GL TRANSACTIONS FROM LOAN PAYMENTS
-- ============================================
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand (Debit)
    PaymentDate,
    PaymentAmount,
    0,
    'Loan Payment received - LP-' + CAST(PaymentId AS VARCHAR),
    0
FROM LoanPayments;

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1110', -- Loans Receivable (Credit)
    PaymentDate,
    0,
    PaymentAmount - ISNULL(PenaltyPaid, 0),
    'Loan principal + interest collected - LP-' + CAST(PaymentId AS VARCHAR),
    0
FROM LoanPayments
WHERE PaymentAmount > ISNULL(PenaltyPaid, 0);

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '4030', -- Penalty Income (Credit)
    PaymentDate,
    0,
    PenaltyPaid,
    'Penalty collected - LP-' + CAST(PaymentId AS VARCHAR),
    0
FROM LoanPayments
WHERE ISNULL(PenaltyPaid, 0) > 0;

PRINT 'Loan Payment GL entries created';

-- ============================================
-- CREATE GL TRANSACTIONS FROM LOAN DISBURSALS
-- ============================================
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1110', -- Loans Receivable (Debit)
    DisbursalDate,
    DisbursalAmount,
    0,
    'Loan disbursed - LD-' + CAST(DisbursalId AS VARCHAR) + ' to ' + ISNULL(DisbursedTo, 'Customer'),
    0
FROM LoanDisbursals
WHERE DisbursalStatus = 'Completed';

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand (Credit)
    DisbursalDate,
    0,
    DisbursalAmount,
    'Cash disbursed for loan - LD-' + CAST(DisbursalId AS VARCHAR),
    0
FROM LoanDisbursals
WHERE DisbursalStatus = 'Completed';

PRINT 'Loan Disbursal GL entries created';

-- ============================================
-- CREATE GL TRANSACTIONS FROM SAVINGS TRANSACTIONS
-- ============================================
-- Savings Deposits: Debit Cash (1010), Credit Savings Deposits (2101)
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand (Debit)
    TransactionDate,
    Amount,
    0,
    'Savings Deposit - ' + TransactionReference,
    0
FROM SavingsTransactions
WHERE TransactionType = 'Deposit' AND Status = 'Completed';

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '2101', -- Savings Deposits (Credit)
    TransactionDate,
    0,
    Amount,
    'Savings liability - ' + TransactionReference,
    0
FROM SavingsTransactions
WHERE TransactionType = 'Deposit' AND Status = 'Completed';

-- Savings Withdrawals: Debit Savings Deposits (2101), Credit Cash (1010)
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '2101', -- Savings Deposits (Debit)
    TransactionDate,
    Amount,
    0,
    'Savings Withdrawal - ' + TransactionReference,
    0
FROM SavingsTransactions
WHERE TransactionType = 'Withdrawal' AND Status = 'Completed';

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand (Credit)
    TransactionDate,
    0,
    Amount,
    'Cash paid out - ' + TransactionReference,
    0
FROM SavingsTransactions
WHERE TransactionType = 'Withdrawal' AND Status = 'Completed';

PRINT 'Savings Transaction GL entries created';

-- ============================================
-- CREATE GL TRANSACTIONS FROM ACCOUNTS PAYABLE
-- ============================================
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '2000', -- Accounts Payable (Debit when paid)
    ISNULL(DueDate, CreatedAt),
    PaidAmount,
    0,
    'AP Payment - ' + ISNULL(InvoiceNumber, 'AP-' + CAST(PayableId AS VARCHAR)) + ' to ' + ISNULL(VendorName, 'Vendor'),
    0
FROM AccountsPayable
WHERE PaidAmount > 0;

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand (Credit when paid)
    ISNULL(DueDate, CreatedAt),
    0,
    PaidAmount,
    'Cash paid for AP - ' + ISNULL(InvoiceNumber, 'AP-' + CAST(PayableId AS VARCHAR)),
    0
FROM AccountsPayable
WHERE PaidAmount > 0;

PRINT 'Accounts Payable GL entries created';

-- ============================================
-- CREATE GL TRANSACTIONS FROM ACCOUNTS RECEIVABLE
-- ============================================
INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1010', -- Cash on Hand (Debit when received)
    ISNULL(DueDate, CreatedAt),
    ReceivedAmount,
    0,
    'AR Receipt - ' + ISNULL(InvoiceNumber, 'AR-' + CAST(ReceivableId AS VARCHAR)) + ' from ' + ISNULL(CustomerName, 'Customer'),
    0
FROM AccountsReceivable
WHERE ReceivedAmount > 0;

INSERT INTO GeneralLedgerTransactions (AccountCode, TransactionDate, DebitAmount, CreditAmount, Description, RunningBalance)
SELECT 
    '1200', -- Accounts Receivable (Credit when received)
    ISNULL(DueDate, CreatedAt),
    0,
    ReceivedAmount,
    'AR collected - ' + ISNULL(InvoiceNumber, 'AR-' + CAST(ReceivableId AS VARCHAR)),
    0
FROM AccountsReceivable
WHERE ReceivedAmount > 0;

PRINT 'Accounts Receivable GL entries created';

-- ============================================
-- UPDATE RUNNING BALANCES (simplified - by account)
-- ============================================
PRINT 'Updating account balances...';

-- Update GeneralLedger balances from transactions
UPDATE gl
SET Balance = ISNULL(totals.NetBalance, 0)
FROM GeneralLedger gl
LEFT JOIN (
    SELECT 
        AccountCode,
        SUM(DebitAmount) - SUM(CreditAmount) AS NetBalance
    FROM GeneralLedgerTransactions
    GROUP BY AccountCode
) totals ON gl.AccountCode = totals.AccountCode;

PRINT '===========================================';
PRINT 'GL POPULATION COMPLETE!';
PRINT '===========================================';

-- Show summary
SELECT 'GeneralLedgerTransactions' AS TableName, COUNT(*) AS RecordCount FROM GeneralLedgerTransactions
UNION ALL
SELECT 'GeneralLedger (Accounts)', COUNT(*) FROM GeneralLedger
UNION ALL
SELECT 'CustomerTransactions', COUNT(*) FROM CustomerTransactions WHERE Status = 'Completed'
UNION ALL
SELECT 'LoanPayments', COUNT(*) FROM LoanPayments
UNION ALL
SELECT 'LoanDisbursals', COUNT(*) FROM LoanDisbursals WHERE DisbursalStatus = 'Completed'
UNION ALL
SELECT 'SavingsTransactions', COUNT(*) FROM SavingsTransactions WHERE Status = 'Completed';
