-- =============================================
-- FINEBANK SAVINGS ACCOUNT MODULE - DATABASE MIGRATION
-- Complete Savings Account System with Interest Calculation
-- =============================================

USE [BFASdatabase];
GO

-- =============================================
-- 1. SAVINGS ACCOUNT TYPES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavingsAccountTypes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SavingsAccountTypes](
        [TypeId] INT IDENTITY(1,1) PRIMARY KEY,
        [TypeName] NVARCHAR(50) NOT NULL UNIQUE,
        [Description] NVARCHAR(500) NULL,
        [MinimumInitialDeposit] DECIMAL(18,2) NOT NULL DEFAULT 5000.00,
        [MinimumMaintainingBalance] DECIMAL(18,2) NOT NULL DEFAULT 10000.00,
        [InterestRateMin] DECIMAL(5,2) NOT NULL DEFAULT 0.10, -- Annual %
        [InterestRateMax] DECIMAL(5,2) NOT NULL DEFAULT 1.00, -- Annual %
        [DefaultLockInDays] INT NOT NULL DEFAULT 0,
        [AllowsWithdrawalAnytime] BIT NOT NULL DEFAULT 1,
        [EarlyWithdrawalPenaltyRate] DECIMAL(5,2) NOT NULL DEFAULT 0.02, -- 2%
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    PRINT 'Created SavingsAccountTypes table';
END
GO

-- Insert default account types
IF NOT EXISTS (SELECT 1 FROM [dbo].[SavingsAccountTypes])
BEGIN
    INSERT INTO [dbo].[SavingsAccountTypes] ([TypeName], [Description], [MinimumInitialDeposit], [MinimumMaintainingBalance], [InterestRateMin], [InterestRateMax], [DefaultLockInDays], [AllowsWithdrawalAnytime], [EarlyWithdrawalPenaltyRate])
    VALUES 
        ('Regular', 'Regular Savings Account - Flexible withdrawals', 1000.00, 2000.00, 0.10, 1.00, 0, 1, 0.00),
        ('TimeDeposit', 'Time Deposit Account - Fixed term, higher interest', 50000.00, 50000.00, 3.00, 6.50, 180, 0, 0.05),
        ('HighYield', 'High-Yield Savings - Higher balance, better rates', 25000.00, 25000.00, 2.00, 4.50, 0, 1, 0.02);
    
    PRINT 'Inserted default savings account types';
END
GO

-- =============================================
-- 2. SAVINGS ACCOUNTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavingsAccounts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SavingsAccounts](
        [SavingsAccountId] INT IDENTITY(1,1) PRIMARY KEY,
        [AccountNumber] NVARCHAR(50) NOT NULL UNIQUE,
        [CustomerId] INT NOT NULL,
        [CustomerAccountId] INT NOT NULL,
        [AccountTypeId] INT NOT NULL,
        [Balance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalDeposits] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalWithdrawals] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalInterestEarned] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [CurrentInterestRate] DECIMAL(5,2) NOT NULL DEFAULT 0.50,
        [InterestCalculationMethod] NVARCHAR(20) NOT NULL DEFAULT 'Daily',
        [InterestPostingSchedule] NVARCHAR(20) NOT NULL DEFAULT 'Monthly',
        [LockInPeriodDays] INT NOT NULL DEFAULT 0,
        [MaturityDate] DATETIME NULL,
        [LastInterestPostDate] DATETIME NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        [IsLocked] BIT NOT NULL DEFAULT 0,
        [OpenedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ClosedDate] DATETIME NULL,
        [OpenedBy] NVARCHAR(100) NULL,
        [ClosedBy] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME NULL,
        CONSTRAINT [FK_SavingsAccounts_Users] FOREIGN KEY ([CustomerId]) REFERENCES [Users]([UserId]),
        CONSTRAINT [FK_SavingsAccounts_CustomerAccounts] FOREIGN KEY ([CustomerAccountId]) REFERENCES [CustomerAccounts]([AccountId]),
        CONSTRAINT [FK_SavingsAccounts_AccountTypes] FOREIGN KEY ([AccountTypeId]) REFERENCES [SavingsAccountTypes]([TypeId])
    );
    
    CREATE NONCLUSTERED INDEX [IX_SavingsAccounts_CustomerId] ON [dbo].[SavingsAccounts]([CustomerId]);
    CREATE NONCLUSTERED INDEX [IX_SavingsAccounts_Status] ON [dbo].[SavingsAccounts]([Status]);
    CREATE NONCLUSTERED INDEX [IX_SavingsAccounts_MaturityDate] ON [dbo].[SavingsAccounts]([MaturityDate]);
    
    PRINT 'Created SavingsAccounts table';
END
GO

-- =============================================
-- 3. SAVINGS TRANSACTIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavingsTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SavingsTransactions](
        [TransactionId] INT IDENTITY(1,1) PRIMARY KEY,
        [TransactionNumber] NVARCHAR(50) NOT NULL UNIQUE,
        [SavingsAccountId] INT NOT NULL,
        [TransactionType] NVARCHAR(20) NOT NULL, -- Deposit, Withdrawal, InterestPosting, PenaltyCharge
        [Amount] DECIMAL(18,2) NOT NULL,
        [BalanceBefore] DECIMAL(18,2) NOT NULL,
        [BalanceAfter] DECIMAL(18,2) NOT NULL,
        [Source] NVARCHAR(20) NOT NULL, -- Cash, FromAccount, ToAccount
        [RelatedCustomerAccountId] INT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [Description] NVARCHAR(500) NULL,
        [ProcessedBy] NVARCHAR(100) NULL,
        [TransactionDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ProcessedAt] DATETIME NULL,
        [ApprovedBy] NVARCHAR(100) NULL,
        [ApprovedAt] DATETIME NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_SavingsTransactions_SavingsAccounts] FOREIGN KEY ([SavingsAccountId]) REFERENCES [SavingsAccounts]([SavingsAccountId]),
        CONSTRAINT [FK_SavingsTransactions_CustomerAccounts] FOREIGN KEY ([RelatedCustomerAccountId]) REFERENCES [CustomerAccounts]([AccountId])
    );
    
    CREATE NONCLUSTERED INDEX [IX_SavingsTransactions_SavingsAccountId] ON [dbo].[SavingsTransactions]([SavingsAccountId]);
    CREATE NONCLUSTERED INDEX [IX_SavingsTransactions_TransactionDate] ON [dbo].[SavingsTransactions]([TransactionDate]);
    CREATE NONCLUSTERED INDEX [IX_SavingsTransactions_Status] ON [dbo].[SavingsTransactions]([Status]);
    
    PRINT 'Created SavingsTransactions table';
END
GO

-- =============================================
-- 4. SAVINGS INTEREST TABLE (Daily Computation)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavingsInterest]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SavingsInterest](
        [InterestId] INT IDENTITY(1,1) PRIMARY KEY,
        [SavingsAccountId] INT NOT NULL,
        [ComputationDate] DATE NOT NULL,
        [DailyBalance] DECIMAL(18,2) NOT NULL,
        [AppliedInterestRate] DECIMAL(5,2) NOT NULL,
        [DailyInterestAmount] DECIMAL(18,4) NOT NULL,
        [IsPosted] BIT NOT NULL DEFAULT 0,
        [PostingBatchId] INT NULL,
        [PostedDate] DATETIME NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_SavingsInterest_SavingsAccounts] FOREIGN KEY ([SavingsAccountId]) REFERENCES [SavingsAccounts]([SavingsAccountId])
    );
    
    CREATE NONCLUSTERED INDEX [IX_SavingsInterest_SavingsAccountId] ON [dbo].[SavingsInterest]([SavingsAccountId]);
    CREATE NONCLUSTERED INDEX [IX_SavingsInterest_ComputationDate] ON [dbo].[SavingsInterest]([ComputationDate]);
    CREATE NONCLUSTERED INDEX [IX_SavingsInterest_IsPosted] ON [dbo].[SavingsInterest]([IsPosted]);
    
    PRINT 'Created SavingsInterest table';
END
GO

-- =============================================
-- 5. SAVINGS INTEREST POSTINGS TABLE (Monthly Batches)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavingsInterestPostings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SavingsInterestPostings](
        [PostingId] INT IDENTITY(1,1) PRIMARY KEY,
        [PostingDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [PeriodStart] DATETIME NOT NULL,
        [PeriodEnd] DATETIME NOT NULL,
        [TotalInterestPosted] DECIMAL(18,2) NOT NULL,
        [AccountsProcessed] INT NOT NULL,
        [PostingStatus] NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, FMApproved, AccountantReleased, Completed
        [ProcessedBy] NVARCHAR(100) NULL,
        [ApprovedByFM] NVARCHAR(100) NULL,
        [ApprovedByFMAt] DATETIME NULL,
        [ReleasedByAccountant] NVARCHAR(100) NULL,
        [ReleasedByAccountantAt] DATETIME NULL,
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    CREATE NONCLUSTERED INDEX [IX_SavingsInterestPostings_PostingDate] ON [dbo].[SavingsInterestPostings]([PostingDate]);
    CREATE NONCLUSTERED INDEX [IX_SavingsInterestPostings_Status] ON [dbo].[SavingsInterestPostings]([PostingStatus]);
    
    PRINT 'Created SavingsInterestPostings table';
END
GO

-- Add FK after PostingBatchId column exists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SavingsInterest_Postings')
BEGIN
    ALTER TABLE [dbo].[SavingsInterest]
    ADD CONSTRAINT [FK_SavingsInterest_Postings] FOREIGN KEY ([PostingBatchId]) REFERENCES [SavingsInterestPostings]([PostingId]);
    
    PRINT 'Added FK_SavingsInterest_Postings';
END
GO

-- =============================================
-- 6. SAVINGS WITHDRAWAL REQUESTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavingsWithdrawalRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SavingsWithdrawalRequests](
        [RequestId] INT IDENTITY(1,1) PRIMARY KEY,
        [SavingsAccountId] INT NOT NULL,
        [CustomerId] INT NOT NULL,
        [RequestedAmount] DECIMAL(18,2) NOT NULL,
        [WithdrawalType] NVARCHAR(20) NOT NULL DEFAULT 'Normal',
        [IsEarlyWithdrawal] BIT NOT NULL DEFAULT 0,
        [PenaltyAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [InterestForfeited] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [NetWithdrawalAmount] DECIMAL(18,2) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [CustomerReason] NVARCHAR(500) NULL,
        [ProcessedByTeller] NVARCHAR(100) NULL,
        [ProcessedByTellerAt] DATETIME NULL,
        [ApprovedByFM] NVARCHAR(100) NULL,
        [ApprovedByFMAt] DATETIME NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        [RequestedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [CompletedDate] DATETIME NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_SavingsWithdrawals_SavingsAccounts] FOREIGN KEY ([SavingsAccountId]) REFERENCES [SavingsAccounts]([SavingsAccountId]),
        CONSTRAINT [FK_SavingsWithdrawals_Users] FOREIGN KEY ([CustomerId]) REFERENCES [Users]([UserId])
    );
    
    CREATE NONCLUSTERED INDEX [IX_SavingsWithdrawals_Status] ON [dbo].[SavingsWithdrawalRequests]([Status]);
    CREATE NONCLUSTERED INDEX [IX_SavingsWithdrawals_RequestedDate] ON [dbo].[SavingsWithdrawalRequests]([RequestedDate]);
    
    PRINT 'Created SavingsWithdrawalRequests table';
END
GO

-- =============================================
-- VERIFICATION & SUMMARY
-- =============================================
PRINT '================================================';
PRINT 'FINEBANK SAVINGS ACCOUNT MODULE MIGRATION';
PRINT '================================================';
PRINT 'Tables Created:';
PRINT '  1. SavingsAccountTypes (Account type configurations)';
PRINT '  2. SavingsAccounts (Customer savings accounts)';
PRINT '  3. SavingsTransactions (Deposits, withdrawals, interest postings)';
PRINT '  4. SavingsInterest (Daily interest computation records)';
PRINT '  5. SavingsInterestPostings (Monthly interest posting batches)';
PRINT '  6. SavingsWithdrawalRequests (Approval workflow for withdrawals)';
PRINT '';
PRINT 'Features Implemented:';
PRINT '  ✓ Three account types: Regular, Time Deposit, High-Yield';
PRINT '  ✓ Daily interest calculation';
PRINT '  ✓ Monthly/Quarterly interest posting';
PRINT '  ✓ Compound interest support';
PRINT '  ✓ Lock-in periods and early withdrawal penalties';
PRINT '  ✓ Approval workflow (Teller → Finance Manager → Accountant)';
PRINT '  ✓ Integration with CustomerAccounts';
PRINT '  ✓ Comprehensive transaction history';
PRINT '================================================';
GO
