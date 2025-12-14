-- =============================================
-- SaaS Management System Database Schema
-- VERSION: Fixed - Renamed tables to avoid conflicts
-- =============================================
-- INSTRUCTIONS:
-- 1. Connect to your existing database in SSMS
-- 2. Run this script
-- =============================================
-- NOTE: All tables are prefixed with "SaaS_" to avoid
-- conflicts with existing tables like "Invoices"
-- =============================================

-- =============================================
-- SYSTEM OWNER TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_SystemOwners' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_SystemOwners (
        OwnerId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Email NVARCHAR(256) NOT NULL UNIQUE,
        FullName NVARCHAR(200) NOT NULL,
        Phone NVARCHAR(50),
        CompanyName NVARCHAR(200) NOT NULL DEFAULT 'ERP Solutions Provider',
        CompanyAddress NVARCHAR(500),
        CompanyLogo NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        LastLoginAt DATETIME
    );
    PRINT 'Created table: SaaS_SystemOwners';
END
GO

-- =============================================
-- SYSTEM MODULES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_SystemModules' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_SystemModules (
        ModuleId INT IDENTITY(1,1) PRIMARY KEY,
        ModuleCode NVARCHAR(50) NOT NULL UNIQUE,
        ModuleName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000),
        Category NVARCHAR(100),
        BasePrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        MonthlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        YearlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        IsCore BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        IconClass NVARCHAR(100),
        Icon NVARCHAR(100),
        SortOrder INT NOT NULL DEFAULT 0,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME
    );
    PRINT 'Created table: SaaS_SystemModules';
END
GO

-- =============================================
-- SUBSCRIPTION PLANS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_SubscriptionPlans' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_SubscriptionPlans (
        PlanId INT IDENTITY(1,1) PRIMARY KEY,
        PlanCode NVARCHAR(50) NOT NULL UNIQUE,
        PlanName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000),
        MonthlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        YearlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        SetupFee DECIMAL(18,2) NOT NULL DEFAULT 0,
        MaxUsers INT NOT NULL DEFAULT 5,
        MaxBranches INT NOT NULL DEFAULT 1,
        MaxTransactionsPerMonth INT,
        MaxStorageGB INT DEFAULT 5,
        Features NVARCHAR(2000),
        IncludesSupport BIT NOT NULL DEFAULT 1,
        SupportLevel NVARCHAR(50) NOT NULL DEFAULT 'Basic',
        IsActive BIT NOT NULL DEFAULT 1,
        IsPopular BIT NOT NULL DEFAULT 0,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME
    );
    PRINT 'Created table: SaaS_SubscriptionPlans';
END
GO

-- =============================================
-- PLAN MODULES (Junction Table)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_PlanModules' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_PlanModules (
        PlanModuleId INT IDENTITY(1,1) PRIMARY KEY,
        PlanId INT NOT NULL,
        ModuleId INT NOT NULL,
        IsIncluded BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_SaaS_PlanModules_Plan FOREIGN KEY (PlanId) REFERENCES SaaS_SubscriptionPlans(PlanId),
        CONSTRAINT FK_SaaS_PlanModules_Module FOREIGN KEY (ModuleId) REFERENCES SaaS_SystemModules(ModuleId),
        CONSTRAINT UQ_SaaS_PlanModules UNIQUE (PlanId, ModuleId)
    );
    PRINT 'Created table: SaaS_PlanModules';
END
GO

-- =============================================
-- SAAS CLIENTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_Clients' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_Clients (
        ClientId INT IDENTITY(1,1) PRIMARY KEY,
        ClientCode NVARCHAR(50) NOT NULL UNIQUE,
        CompanyName NVARCHAR(300) NOT NULL,
        TradeName NVARCHAR(300),
        BusinessType NVARCHAR(100),
        TaxId NVARCHAR(100),
        Address NVARCHAR(500),
        City NVARCHAR(100),
        Province NVARCHAR(100),
        PostalCode NVARCHAR(20),
        Country NVARCHAR(100) DEFAULT 'Philippines',
        PrimaryEmail NVARCHAR(256) NOT NULL,
        SecondaryEmail NVARCHAR(256),
        Phone NVARCHAR(50),
        Mobile NVARCHAR(50),
        Website NVARCHAR(256),
        ContactPersonName NVARCHAR(200),
        ContactPersonTitle NVARCHAR(100),
        ContactPersonEmail NVARCHAR(256),
        ContactPersonPhone NVARCHAR(50),
        DatabaseName NVARCHAR(100),
        SystemUrl NVARCHAR(256),
        LicenseKey NVARCHAR(256),
        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        TrialEndsAt DATETIME,
        SubscriptionStartDate DATETIME,
        SubscriptionEndDate DATETIME,
        BillingCycle NVARCHAR(50) DEFAULT 'Monthly',
        BillingDay INT DEFAULT 1,
        CreditBalance DECIMAL(18,2) DEFAULT 0,
        OutstandingBalance DECIMAL(18,2) DEFAULT 0,
        TotalPaid DECIMAL(18,2) DEFAULT 0,
        Logo NVARCHAR(500),
        PrimaryColor NVARCHAR(20),
        Notes NVARCHAR(2000),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME,
        CreatedBy INT
    );
    PRINT 'Created table: SaaS_Clients';
END
GO

-- =============================================
-- CLIENT USERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_ClientUsers' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_ClientUsers (
        ClientUserId INT IDENTITY(1,1) PRIMARY KEY,
        ClientId INT NOT NULL,
        Username NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(256) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        FullName NVARCHAR(200),
        Role NVARCHAR(50) DEFAULT 'User',
        IsActive BIT NOT NULL DEFAULT 1,
        LastLoginAt DATETIME,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SaaS_ClientUsers_Client FOREIGN KEY (ClientId) REFERENCES SaaS_Clients(ClientId),
        CONSTRAINT UQ_SaaS_ClientUsers_Email UNIQUE (ClientId, Email)
    );
    PRINT 'Created table: SaaS_ClientUsers';
END
GO

-- =============================================
-- CLIENT SUBSCRIPTIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_ClientSubscriptions' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_ClientSubscriptions (
        SubscriptionId INT IDENTITY(1,1) PRIMARY KEY,
        ClientId INT NOT NULL,
        PlanId INT NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        BillingCycle NVARCHAR(50) DEFAULT 'Monthly',
        StartDate DATETIME NOT NULL,
        EndDate DATETIME,
        NextBillingDate DATETIME,
        BasePrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        Discount DECIMAL(18,2) DEFAULT 0,
        TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        AutoRenew BIT NOT NULL DEFAULT 1,
        CancelledAt DATETIME,
        CancellationReason NVARCHAR(500),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME,
        CONSTRAINT FK_SaaS_ClientSubscriptions_Client FOREIGN KEY (ClientId) REFERENCES SaaS_Clients(ClientId),
        CONSTRAINT FK_SaaS_ClientSubscriptions_Plan FOREIGN KEY (PlanId) REFERENCES SaaS_SubscriptionPlans(PlanId)
    );
    PRINT 'Created table: SaaS_ClientSubscriptions';
END
GO

-- =============================================
-- CLIENT MODULES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_ClientModules' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_ClientModules (
        ClientModuleId INT IDENTITY(1,1) PRIMARY KEY,
        ClientId INT NOT NULL,
        ModuleId INT NOT NULL,
        IsEnabled BIT NOT NULL DEFAULT 1,
        AddedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME,
        CustomPrice DECIMAL(18,2),
        LicenseType NVARCHAR(50) DEFAULT 'Subscription',
        CONSTRAINT FK_SaaS_ClientModules_Client FOREIGN KEY (ClientId) REFERENCES SaaS_Clients(ClientId),
        CONSTRAINT FK_SaaS_ClientModules_Module FOREIGN KEY (ModuleId) REFERENCES SaaS_SystemModules(ModuleId),
        CONSTRAINT UQ_SaaS_ClientModules UNIQUE (ClientId, ModuleId)
    );
    PRINT 'Created table: SaaS_ClientModules';
END
GO

-- =============================================
-- PAYMENT METHODS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_PaymentMethods' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_PaymentMethods (
        PaymentMethodId INT IDENTITY(1,1) PRIMARY KEY,
        MethodCode NVARCHAR(50) NOT NULL UNIQUE,
        MethodName NVARCHAR(100) NOT NULL,
        MethodType NVARCHAR(50),
        Description NVARCHAR(500),
        AccountName NVARCHAR(200),
        AccountNumber NVARCHAR(100),
        BankName NVARCHAR(200),
        Instructions NVARCHAR(1000),
        IconClass NVARCHAR(100),
        Icon NVARCHAR(100),
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table: SaaS_PaymentMethods';
END
GO

-- =============================================
-- INVOICES TABLE (SaaS_Invoices to avoid conflict)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_Invoices' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_Invoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
        ClientId INT NOT NULL,
        SubscriptionId INT,
        InvoiceDate DATETIME NOT NULL DEFAULT GETDATE(),
        DueDate DATETIME NOT NULL,
        PeriodStart DATETIME,
        PeriodEnd DATETIME,
        Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxRate DECIMAL(5,2) DEFAULT 0,
        Tax DECIMAL(18,2) DEFAULT 0,
        Discount DECIMAL(18,2) DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        AmountPaid DECIMAL(18,2) DEFAULT 0,
        BalanceDue DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        Notes NVARCHAR(1000),
        InternalNotes NVARCHAR(1000),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME,
        SentAt DATETIME,
        PaidAt DATETIME,
        CONSTRAINT FK_SaaS_Invoices_Client FOREIGN KEY (ClientId) REFERENCES SaaS_Clients(ClientId),
        CONSTRAINT FK_SaaS_Invoices_Subscription FOREIGN KEY (SubscriptionId) REFERENCES SaaS_ClientSubscriptions(SubscriptionId)
    );
    PRINT 'Created table: SaaS_Invoices';
END
GO

-- =============================================
-- INVOICE ITEMS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_InvoiceItems' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_InvoiceItems (
        InvoiceItemId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceId INT NOT NULL,
        ModuleId INT,
        Description NVARCHAR(500) NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        Discount DECIMAL(18,2) DEFAULT 0,
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        ItemType NVARCHAR(50) DEFAULT 'Subscription',
        CONSTRAINT FK_SaaS_InvoiceItems_Invoice FOREIGN KEY (InvoiceId) REFERENCES SaaS_Invoices(InvoiceId) ON DELETE CASCADE,
        CONSTRAINT FK_SaaS_InvoiceItems_Module FOREIGN KEY (ModuleId) REFERENCES SaaS_SystemModules(ModuleId)
    );
    PRINT 'Created table: SaaS_InvoiceItems';
END
GO

-- =============================================
-- SAAS TRANSACTIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_Transactions' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_Transactions (
        TransactionId INT IDENTITY(1,1) PRIMARY KEY,
        TransactionNumber NVARCHAR(50) NOT NULL UNIQUE,
        ReferenceNumber NVARCHAR(100),
        TransactionRef NVARCHAR(100),
        ClientId INT NOT NULL,
        InvoiceId INT,
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        TransactionType NVARCHAR(50) NOT NULL,
        PaymentMethodId INT,
        PaymentReference NVARCHAR(200),
        PaymentProof NVARCHAR(500),
        Amount DECIMAL(18,2) NOT NULL,
        Currency NVARCHAR(10) DEFAULT 'PHP',
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        Description NVARCHAR(500),
        Notes NVARCHAR(1000),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ProcessedAt DATETIME,
        ProcessedBy INT,
        CONSTRAINT FK_SaaS_Transactions_Client FOREIGN KEY (ClientId) REFERENCES SaaS_Clients(ClientId),
        CONSTRAINT FK_SaaS_Transactions_Invoice FOREIGN KEY (InvoiceId) REFERENCES SaaS_Invoices(InvoiceId),
        CONSTRAINT FK_SaaS_Transactions_PaymentMethod FOREIGN KEY (PaymentMethodId) REFERENCES SaaS_PaymentMethods(PaymentMethodId)
    );
    PRINT 'Created table: SaaS_Transactions';
END
GO

-- =============================================
-- SUPPORT TICKETS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_SupportTickets' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_SupportTickets (
        TicketId INT IDENTITY(1,1) PRIMARY KEY,
        TicketNumber NVARCHAR(50) NOT NULL UNIQUE,
        ClientId INT NOT NULL,
        UserId INT,
        Subject NVARCHAR(300) NOT NULL,
        Description NVARCHAR(4000) NOT NULL,
        Category NVARCHAR(100),
        Priority NVARCHAR(50) DEFAULT 'Medium',
        Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
        AssignedTo INT,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME,
        ResolvedAt DATETIME,
        ClosedAt DATETIME,
        CONSTRAINT FK_SaaS_SupportTickets_Client FOREIGN KEY (ClientId) REFERENCES SaaS_Clients(ClientId),
        CONSTRAINT FK_SaaS_SupportTickets_User FOREIGN KEY (UserId) REFERENCES SaaS_ClientUsers(ClientUserId)
    );
    PRINT 'Created table: SaaS_SupportTickets';
END
GO

-- =============================================
-- TICKET COMMENTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_TicketComments' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_TicketComments (
        CommentId INT IDENTITY(1,1) PRIMARY KEY,
        TicketId INT NOT NULL,
        UserId INT,
        OwnerId INT,
        Comment NVARCHAR(4000) NOT NULL,
        IsInternal BIT NOT NULL DEFAULT 0,
        AttachmentPath NVARCHAR(500),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SaaS_TicketComments_Ticket FOREIGN KEY (TicketId) REFERENCES SaaS_SupportTickets(TicketId) ON DELETE CASCADE
    );
    PRINT 'Created table: SaaS_TicketComments';
END
GO

-- =============================================
-- ACTIVITY LOG TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_ActivityLog' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_ActivityLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        EntityType NVARCHAR(100) NOT NULL,
        EntityId INT NOT NULL,
        Action NVARCHAR(100) NOT NULL,
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX),
        PerformedBy NVARCHAR(200),
        PerformedAt DATETIME NOT NULL DEFAULT GETDATE(),
        IpAddress NVARCHAR(50),
        UserAgent NVARCHAR(500)
    );
    PRINT 'Created table: SaaS_ActivityLog';
END
GO

-- =============================================
-- LICENSE KEYS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaS_LicenseKeys' AND xtype='U')
BEGIN
    CREATE TABLE SaaS_LicenseKeys (
        LicenseId INT IDENTITY(1,1) PRIMARY KEY,
        LicenseKey NVARCHAR(256) NOT NULL UNIQUE,
        ClientId INT NOT NULL,
        ProductCode NVARCHAR(100),
        LicenseType NVARCHAR(50) DEFAULT 'Subscription',
        IssuedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME,
        IsActive BIT NOT NULL DEFAULT 1,
        LastValidatedAt DATETIME,
        ValidationCount INT DEFAULT 0,
        MachineId NVARCHAR(256),
        Notes NVARCHAR(500),
        CONSTRAINT FK_SaaS_LicenseKeys_Client FOREIGN KEY (ClientId) REFERENCES SaaS_Clients(ClientId)
    );
    PRINT 'Created table: SaaS_LicenseKeys';
END
GO

-- =============================================
-- INDEXES (with IF NOT EXISTS checks)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_Clients_Status')
    CREATE NONCLUSTERED INDEX IX_SaaS_Clients_Status ON SaaS_Clients(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_Clients_Email')
    CREATE NONCLUSTERED INDEX IX_SaaS_Clients_Email ON SaaS_Clients(PrimaryEmail);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_ClientSubscriptions_Client')
    CREATE NONCLUSTERED INDEX IX_SaaS_ClientSubscriptions_Client ON SaaS_ClientSubscriptions(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_ClientSubscriptions_Status')
    CREATE NONCLUSTERED INDEX IX_SaaS_ClientSubscriptions_Status ON SaaS_ClientSubscriptions(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_Invoices_Client')
    CREATE NONCLUSTERED INDEX IX_SaaS_Invoices_Client ON SaaS_Invoices(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_Invoices_Status')
    CREATE NONCLUSTERED INDEX IX_SaaS_Invoices_Status ON SaaS_Invoices(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_Invoices_DueDate')
    CREATE NONCLUSTERED INDEX IX_SaaS_Invoices_DueDate ON SaaS_Invoices(DueDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_Transactions_Client')
    CREATE NONCLUSTERED INDEX IX_SaaS_Transactions_Client ON SaaS_Transactions(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_Transactions_Date')
    CREATE NONCLUSTERED INDEX IX_SaaS_Transactions_Date ON SaaS_Transactions(TransactionDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_SupportTickets_Client')
    CREATE NONCLUSTERED INDEX IX_SaaS_SupportTickets_Client ON SaaS_SupportTickets(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_SupportTickets_Status')
    CREATE NONCLUSTERED INDEX IX_SaaS_SupportTickets_Status ON SaaS_SupportTickets(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_ClientModules_Client')
    CREATE NONCLUSTERED INDEX IX_SaaS_ClientModules_Client ON SaaS_ClientModules(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaS_LicenseKeys_Client')
    CREATE NONCLUSTERED INDEX IX_SaaS_LicenseKeys_Client ON SaaS_LicenseKeys(ClientId);
GO

PRINT 'Indexes created successfully';
GO

-- =============================================
-- VIEWS (using SaaS_ prefixed tables)
-- =============================================
CREATE OR ALTER VIEW vw_SaaS_ClientOverview AS
SELECT 
    c.ClientId,
    c.ClientCode,
    c.CompanyName,
    c.Status,
    c.PrimaryEmail,
    cs.PlanId,
    sp.PlanName,
    cs.Status AS SubscriptionStatus,
    cs.NextBillingDate,
    c.OutstandingBalance,
    c.TotalPaid,
    (SELECT COUNT(*) FROM SaaS_ClientModules cm WHERE cm.ClientId = c.ClientId AND cm.IsEnabled = 1) AS ActiveModules,
    (SELECT COUNT(*) FROM SaaS_ClientUsers cu WHERE cu.ClientId = c.ClientId AND cu.IsActive = 1) AS ActiveUsers
FROM SaaS_Clients c
LEFT JOIN SaaS_ClientSubscriptions cs ON c.ClientId = cs.ClientId AND cs.Status = 'Active'
LEFT JOIN SaaS_SubscriptionPlans sp ON cs.PlanId = sp.PlanId;
GO

CREATE OR ALTER VIEW vw_SaaS_MonthlyRevenue AS
SELECT 
    YEAR(t.TransactionDate) AS Year,
    MONTH(t.TransactionDate) AS Month,
    SUM(CASE WHEN t.TransactionType = 'Payment' THEN t.Amount ELSE 0 END) AS TotalPayments,
    SUM(CASE WHEN t.TransactionType = 'Refund' THEN t.Amount ELSE 0 END) AS TotalRefunds,
    SUM(CASE WHEN t.TransactionType = 'Payment' THEN t.Amount ELSE 0 END) - 
    SUM(CASE WHEN t.TransactionType = 'Refund' THEN t.Amount ELSE 0 END) AS NetRevenue,
    COUNT(*) AS TransactionCount
FROM SaaS_Transactions t
WHERE t.Status = 'Completed'
GROUP BY YEAR(t.TransactionDate), MONTH(t.TransactionDate);
GO

CREATE OR ALTER VIEW vw_SaaS_PendingInvoices AS
SELECT 
    i.InvoiceId,
    i.InvoiceNumber,
    c.CompanyName,
    c.PrimaryEmail,
    i.TotalAmount,
    i.BalanceDue,
    i.DueDate,
    DATEDIFF(DAY, i.DueDate, GETDATE()) AS DaysOverdue,
    i.Status
FROM SaaS_Invoices i
JOIN SaaS_Clients c ON i.ClientId = c.ClientId
WHERE i.Status IN ('Pending', 'Sent', 'Overdue');
GO

CREATE OR ALTER VIEW vw_SaaS_ModuleUsage AS
SELECT 
    m.ModuleId,
    m.ModuleCode,
    m.ModuleName,
    m.Category,
    m.MonthlyPrice,
    COUNT(cm.ClientModuleId) AS TotalClients,
    SUM(CASE WHEN cm.IsEnabled = 1 THEN 1 ELSE 0 END) AS ActiveClients
FROM SaaS_SystemModules m
LEFT JOIN SaaS_ClientModules cm ON m.ModuleId = cm.ModuleId
GROUP BY m.ModuleId, m.ModuleCode, m.ModuleName, m.Category, m.MonthlyPrice;
GO

PRINT 'Views created successfully';
GO

-- =============================================
-- SEED DATA: Default Owner
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_SystemOwners WHERE Username = 'admin')
BEGIN
    INSERT INTO SaaS_SystemOwners (Username, PasswordHash, Email, FullName, CompanyName, IsActive)
    VALUES ('admin', 'admin123', 'admin@erpsolutions.com', 'System Administrator', 'ERP Solutions Provider', 1);
    PRINT 'Inserted default admin user';
END
GO

-- =============================================
-- SEED DATA: Payment Methods
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_PaymentMethods WHERE MethodCode = 'GCASH')
BEGIN
    INSERT INTO SaaS_PaymentMethods (MethodCode, MethodName, MethodType, Description, AccountNumber, Instructions, IsActive, SortOrder)
    VALUES 
    ('GCASH', 'GCash', 'E-Wallet', 'Pay via GCash mobile wallet', '09XX-XXX-XXXX', 'Send payment to the GCash number and upload screenshot', 1, 1),
    ('PAYMAYA', 'PayMaya', 'E-Wallet', 'Pay via PayMaya mobile wallet', '09XX-XXX-XXXX', 'Send payment to the PayMaya number and upload screenshot', 1, 2),
    ('BDO', 'BDO Bank Transfer', 'Bank', 'Bank transfer to BDO account', '1234-5678-9012', 'Transfer to BDO account and upload deposit slip', 1, 3),
    ('BPI', 'BPI Bank Transfer', 'Bank', 'Bank transfer to BPI account', '1234-5678-9012', 'Transfer to BPI account and upload deposit slip', 1, 4),
    ('CASH', 'Cash Payment', 'Cash', 'Pay in cash at our office', NULL, 'Visit our office during business hours', 1, 5);
    PRINT 'Inserted payment methods';
END
GO

-- =============================================
-- SEED DATA: System Modules
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_SystemModules WHERE ModuleCode = 'CORE')
BEGIN
    INSERT INTO SaaS_SystemModules (ModuleCode, ModuleName, Description, Category, MonthlyPrice, YearlyPrice, IsCore, IsActive, SortOrder, DisplayOrder)
    VALUES 
    ('CORE', 'Core System', 'Core banking system foundation', 'Core', 0, 0, 1, 1, 1, 1),
    ('TELLER', 'Teller Operations', 'Cash transactions, deposits, withdrawals', 'Operations', 500, 5000, 0, 1, 2, 2),
    ('LOANS', 'Loan Management', 'Loan applications, approvals, payments', 'Finance', 1000, 10000, 0, 1, 3, 3),
    ('SAVINGS', 'Savings Accounts', 'Savings account management and interest', 'Accounts', 500, 5000, 0, 1, 4, 4),
    ('GL', 'General Ledger', 'Chart of accounts, journal entries', 'Accounting', 800, 8000, 0, 1, 5, 5),
    ('REPORTS', 'Financial Reports', 'Balance sheets, income statements', 'Reports', 500, 5000, 0, 1, 6, 6),
    ('ADMIN', 'User Administration', 'User management, roles, permissions', 'Administration', 300, 3000, 0, 1, 7, 7),
    ('AUDIT', 'Audit Trail', 'System audit logs and security', 'Security', 400, 4000, 0, 1, 8, 8);
    PRINT 'Inserted system modules';
END
GO

-- =============================================
-- SEED DATA: Subscription Plans
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_SubscriptionPlans WHERE PlanCode = 'BASIC')
BEGIN
    INSERT INTO SaaS_SubscriptionPlans (PlanCode, PlanName, Description, MonthlyPrice, YearlyPrice, SetupFee, MaxUsers, MaxBranches, Features, SupportLevel, IsActive, IsPopular, SortOrder)
    VALUES 
    ('BASIC', 'Basic Plan', 'Essential banking features for small cooperatives', 2999, 29990, 5000, 5, 1, 'Core System,Teller Operations,Savings Accounts,Basic Reports', 'Email', 1, 0, 1),
    ('PRO', 'Professional Plan', 'Complete solution for growing cooperatives', 5999, 59990, 10000, 15, 3, 'All Basic Features,Loan Management,General Ledger,Advanced Reports,Audit Trail', 'Priority Email', 1, 1, 2),
    ('ENTERPRISE', 'Enterprise Plan', 'Full-featured solution for large organizations', 9999, 99990, 20000, 50, 10, 'All Pro Features,Multi-Branch Support,Custom Reports,API Access,Dedicated Support', '24/7 Phone & Email', 1, 0, 3);
    PRINT 'Inserted subscription plans';
END
GO

-- =============================================
-- SEED DATA: Demo Client for testing
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_Clients WHERE ClientCode = 'DEMO001')
BEGIN
    INSERT INTO SaaS_Clients (ClientCode, CompanyName, PrimaryEmail, Status, IsActive, Phone, Address, City, Province, Country, ContactPersonName, SubscriptionStartDate, BillingCycle, OutstandingBalance, TotalPaid)
    VALUES ('DEMO001', 'Demo Company Inc.', 'demo@company.com', 'Active', 1, '09123456789', '123 Main Street', 'Manila', 'Metro Manila', 'Philippines', 'John Demo', DATEADD(MONTH, -6, GETDATE()), 'Monthly', 2999.00, 17994.00);
    PRINT 'Inserted demo client';
END
GO

-- =============================================
-- SEED DATA: Your Company as a Client
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_Clients WHERE ClientCode = 'FINEBANK001')
BEGIN
    INSERT INTO SaaS_Clients (ClientCode, CompanyName, TradeName, PrimaryEmail, Status, IsActive, Phone, Mobile, Address, City, Province, Country, ContactPersonName, ContactPersonTitle, SubscriptionStartDate, SubscriptionEndDate, BillingCycle, BillingDay, OutstandingBalance, TotalPaid, CreatedAt)
    VALUES (
        'FINEBANK001', 
        'FineBank Cooperative', 
        'FINEBANK',
        'admin@finebank.com', 
        'Active', 
        1, 
        '(02) 8123-4567',
        '09171234567',
        '456 Finance Avenue, Makati Business District', 
        'Makati City', 
        'Metro Manila', 
        'Philippines',
        'System Administrator',
        'IT Manager',
        DATEADD(MONTH, -12, GETDATE()),  -- Started 12 months ago
        DATEADD(MONTH, 12, GETDATE()),   -- Valid for another 12 months
        'Monthly',
        1,
        0.00,      -- No outstanding balance (paid up)
        71988.00,  -- 12 months x 5999 = total paid
        DATEADD(MONTH, -12, GETDATE())
    );
    PRINT 'Inserted your company: FineBank Cooperative';
END
GO

-- =============================================
-- SEED DATA: Client Subscription for FineBank
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_ClientSubscriptions cs 
               JOIN SaaS_Clients c ON cs.ClientId = c.ClientId 
               WHERE c.ClientCode = 'FINEBANK001')
BEGIN
    DECLARE @FineBankClientId INT = (SELECT ClientId FROM SaaS_Clients WHERE ClientCode = 'FINEBANK001');
    DECLARE @ProPlanId INT = (SELECT PlanId FROM SaaS_SubscriptionPlans WHERE PlanCode = 'PRO');
    
    IF @FineBankClientId IS NOT NULL AND @ProPlanId IS NOT NULL
    BEGIN
        INSERT INTO SaaS_ClientSubscriptions (ClientId, PlanId, Status, BillingCycle, StartDate, EndDate, NextBillingDate, BasePrice, TotalPrice, AutoRenew)
        VALUES (
            @FineBankClientId,
            @ProPlanId,
            'Active',
            'Monthly',
            DATEADD(MONTH, -12, GETDATE()),
            DATEADD(MONTH, 12, GETDATE()),
            DATEADD(DAY, 15, GETDATE()),  -- Next billing in 15 days
            5999.00,
            5999.00,
            1
        );
        PRINT 'Inserted FineBank subscription';
    END
END
GO

-- =============================================
-- SEED DATA: Sample Invoices (Payment History)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_Invoices WHERE InvoiceNumber = 'INV-2024-001')
BEGIN
    DECLARE @ClientId INT = (SELECT ClientId FROM SaaS_Clients WHERE ClientCode = 'FINEBANK001');
    
    IF @ClientId IS NOT NULL
    BEGIN
        -- Invoice 1: Paid (6 months ago)
        INSERT INTO SaaS_Invoices (InvoiceNumber, ClientId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, TotalAmount, AmountPaid, BalanceDue, Status, PaidAt)
        VALUES ('INV-2024-001', @ClientId, DATEADD(MONTH, -6, GETDATE()), DATEADD(MONTH, -6, DATEADD(DAY, 15, GETDATE())), DATEADD(MONTH, -7, GETDATE()), DATEADD(MONTH, -6, GETDATE()), 5999.00, 5999.00, 5999.00, 0, 'Paid', DATEADD(MONTH, -6, DATEADD(DAY, 5, GETDATE())));
        
        -- Invoice 2: Paid (5 months ago)
        INSERT INTO SaaS_Invoices (InvoiceNumber, ClientId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, TotalAmount, AmountPaid, BalanceDue, Status, PaidAt)
        VALUES ('INV-2024-002', @ClientId, DATEADD(MONTH, -5, GETDATE()), DATEADD(MONTH, -5, DATEADD(DAY, 15, GETDATE())), DATEADD(MONTH, -6, GETDATE()), DATEADD(MONTH, -5, GETDATE()), 5999.00, 5999.00, 5999.00, 0, 'Paid', DATEADD(MONTH, -5, DATEADD(DAY, 3, GETDATE())));
        
        -- Invoice 3: Paid (4 months ago)
        INSERT INTO SaaS_Invoices (InvoiceNumber, ClientId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, TotalAmount, AmountPaid, BalanceDue, Status, PaidAt)
        VALUES ('INV-2024-003', @ClientId, DATEADD(MONTH, -4, GETDATE()), DATEADD(MONTH, -4, DATEADD(DAY, 15, GETDATE())), DATEADD(MONTH, -5, GETDATE()), DATEADD(MONTH, -4, GETDATE()), 5999.00, 5999.00, 5999.00, 0, 'Paid', DATEADD(MONTH, -4, DATEADD(DAY, 2, GETDATE())));
        
        -- Invoice 4: Paid (3 months ago)
        INSERT INTO SaaS_Invoices (InvoiceNumber, ClientId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, TotalAmount, AmountPaid, BalanceDue, Status, PaidAt)
        VALUES ('INV-2024-004', @ClientId, DATEADD(MONTH, -3, GETDATE()), DATEADD(MONTH, -3, DATEADD(DAY, 15, GETDATE())), DATEADD(MONTH, -4, GETDATE()), DATEADD(MONTH, -3, GETDATE()), 5999.00, 5999.00, 5999.00, 0, 'Paid', DATEADD(MONTH, -3, DATEADD(DAY, 1, GETDATE())));
        
        -- Invoice 5: Paid (2 months ago)
        INSERT INTO SaaS_Invoices (InvoiceNumber, ClientId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, TotalAmount, AmountPaid, BalanceDue, Status, PaidAt)
        VALUES ('INV-2024-005', @ClientId, DATEADD(MONTH, -2, GETDATE()), DATEADD(MONTH, -2, DATEADD(DAY, 15, GETDATE())), DATEADD(MONTH, -3, GETDATE()), DATEADD(MONTH, -2, GETDATE()), 5999.00, 5999.00, 5999.00, 0, 'Paid', DATEADD(MONTH, -2, DATEADD(DAY, 4, GETDATE())));
        
        -- Invoice 6: Paid (1 month ago)
        INSERT INTO SaaS_Invoices (InvoiceNumber, ClientId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, TotalAmount, AmountPaid, BalanceDue, Status, PaidAt)
        VALUES ('INV-2024-006', @ClientId, DATEADD(MONTH, -1, GETDATE()), DATEADD(MONTH, -1, DATEADD(DAY, 15, GETDATE())), DATEADD(MONTH, -2, GETDATE()), DATEADD(MONTH, -1, GETDATE()), 5999.00, 5999.00, 5999.00, 0, 'Paid', DATEADD(MONTH, -1, DATEADD(DAY, 2, GETDATE())));
        
        -- Invoice 7: Current month - Pending
        INSERT INTO SaaS_Invoices (InvoiceNumber, ClientId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, TotalAmount, AmountPaid, BalanceDue, Status)
        VALUES ('INV-2024-007', @ClientId, GETDATE(), DATEADD(DAY, 15, GETDATE()), DATEADD(MONTH, -1, GETDATE()), GETDATE(), 5999.00, 5999.00, 0, 5999.00, 'Pending');
        
        PRINT 'Inserted sample invoices for FineBank';
    END
END
GO

-- =============================================
-- SEED DATA: Sample Transactions (Payment Records)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_Transactions WHERE TransactionNumber = 'TXN-2024-001')
BEGIN
    DECLARE @ClientId2 INT = (SELECT ClientId FROM SaaS_Clients WHERE ClientCode = 'FINEBANK001');
    DECLARE @GCashId INT = (SELECT PaymentMethodId FROM SaaS_PaymentMethods WHERE MethodCode = 'GCASH');
    DECLARE @BDOId INT = (SELECT PaymentMethodId FROM SaaS_PaymentMethods WHERE MethodCode = 'BDO');
    
    IF @ClientId2 IS NOT NULL
    BEGIN
        -- Transaction 1: Payment via GCash (6 months ago)
        INSERT INTO SaaS_Transactions (TransactionNumber, ClientId, TransactionDate, TransactionType, PaymentMethodId, Amount, Status, Description, ProcessedAt)
        VALUES ('TXN-2024-001', @ClientId2, DATEADD(MONTH, -6, DATEADD(DAY, 5, GETDATE())), 'Payment', @GCashId, 5999.00, 'Completed', 'Monthly subscription payment - July 2024', DATEADD(MONTH, -6, DATEADD(DAY, 5, GETDATE())));
        
        -- Transaction 2: Payment via BDO (5 months ago)
        INSERT INTO SaaS_Transactions (TransactionNumber, ClientId, TransactionDate, TransactionType, PaymentMethodId, Amount, Status, Description, ProcessedAt)
        VALUES ('TXN-2024-002', @ClientId2, DATEADD(MONTH, -5, DATEADD(DAY, 3, GETDATE())), 'Payment', @BDOId, 5999.00, 'Completed', 'Monthly subscription payment - August 2024', DATEADD(MONTH, -5, DATEADD(DAY, 3, GETDATE())));
        
        -- Transaction 3: Payment via GCash (4 months ago)
        INSERT INTO SaaS_Transactions (TransactionNumber, ClientId, TransactionDate, TransactionType, PaymentMethodId, Amount, Status, Description, ProcessedAt)
        VALUES ('TXN-2024-003', @ClientId2, DATEADD(MONTH, -4, DATEADD(DAY, 2, GETDATE())), 'Payment', @GCashId, 5999.00, 'Completed', 'Monthly subscription payment - September 2024', DATEADD(MONTH, -4, DATEADD(DAY, 2, GETDATE())));
        
        -- Transaction 4: Payment via GCash (3 months ago)
        INSERT INTO SaaS_Transactions (TransactionNumber, ClientId, TransactionDate, TransactionType, PaymentMethodId, Amount, Status, Description, ProcessedAt)
        VALUES ('TXN-2024-004', @ClientId2, DATEADD(MONTH, -3, DATEADD(DAY, 1, GETDATE())), 'Payment', @GCashId, 5999.00, 'Completed', 'Monthly subscription payment - October 2024', DATEADD(MONTH, -3, DATEADD(DAY, 1, GETDATE())));
        
        -- Transaction 5: Payment via BDO (2 months ago)
        INSERT INTO SaaS_Transactions (TransactionNumber, ClientId, TransactionDate, TransactionType, PaymentMethodId, Amount, Status, Description, ProcessedAt)
        VALUES ('TXN-2024-005', @ClientId2, DATEADD(MONTH, -2, DATEADD(DAY, 4, GETDATE())), 'Payment', @BDOId, 5999.00, 'Completed', 'Monthly subscription payment - November 2024', DATEADD(MONTH, -2, DATEADD(DAY, 4, GETDATE())));
        
        -- Transaction 6: Payment via GCash (1 month ago)
        INSERT INTO SaaS_Transactions (TransactionNumber, ClientId, TransactionDate, TransactionType, PaymentMethodId, Amount, Status, Description, ProcessedAt)
        VALUES ('TXN-2024-006', @ClientId2, DATEADD(MONTH, -1, DATEADD(DAY, 2, GETDATE())), 'Payment', @GCashId, 5999.00, 'Completed', 'Monthly subscription payment - December 2024', DATEADD(MONTH, -1, DATEADD(DAY, 2, GETDATE())));
        
        PRINT 'Inserted sample transactions for FineBank';
    END
END
GO

-- =============================================
-- SEED DATA: Client Modules for FineBank (Pro Plan modules)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SaaS_ClientModules cm 
               JOIN SaaS_Clients c ON cm.ClientId = c.ClientId 
               WHERE c.ClientCode = 'FINEBANK001')
BEGIN
    DECLARE @FBClientId INT = (SELECT ClientId FROM SaaS_Clients WHERE ClientCode = 'FINEBANK001');
    
    IF @FBClientId IS NOT NULL
    BEGIN
        -- Add all Pro Plan modules
        INSERT INTO SaaS_ClientModules (ClientId, ModuleId, IsEnabled, AddedAt)
        SELECT @FBClientId, ModuleId, 1, DATEADD(MONTH, -12, GETDATE())
        FROM SaaS_SystemModules
        WHERE ModuleCode IN ('CORE', 'TELLER', 'LOANS', 'SAVINGS', 'GL', 'REPORTS', 'ADMIN', 'AUDIT');
        
        PRINT 'Inserted client modules for FineBank';
    END
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'SaaS Schema created successfully!';
PRINT '==============================================';
PRINT 'All tables prefixed with SaaS_ to avoid conflicts';
PRINT '';
PRINT 'Admin login: admin / admin123';
PRINT 'Client demo: demo@company.com / demo123';
PRINT '';
PRINT 'Sample Data Included:';
PRINT '- FineBank Cooperative (12 months of history)';
PRINT '- 7 Invoices (6 paid, 1 pending)';
PRINT '- 6 Payment transactions';
PRINT '- All Pro Plan modules enabled';
PRINT '==============================================';
GO
