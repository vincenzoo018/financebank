-- =============================================
-- SaaS Management System Database Schema
-- VERSION: No Database Creation (use existing DB)
-- =============================================
-- INSTRUCTIONS:
-- 1. Connect to your existing database in SSMS
-- 2. Run this script
-- =============================================

-- =============================================
-- SYSTEM OWNER TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SystemOwners' AND xtype='U')
BEGIN
    CREATE TABLE SystemOwners (
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
    PRINT 'Created table: SystemOwners';
END
GO

-- =============================================
-- SYSTEM MODULES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SystemModules' AND xtype='U')
BEGIN
    CREATE TABLE SystemModules (
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
    PRINT 'Created table: SystemModules';
END
GO

-- =============================================
-- SUBSCRIPTION PLANS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SubscriptionPlans' AND xtype='U')
BEGIN
    CREATE TABLE SubscriptionPlans (
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
    PRINT 'Created table: SubscriptionPlans';
END
GO

-- =============================================
-- PLAN MODULES (Junction Table)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PlanModules' AND xtype='U')
BEGIN
    CREATE TABLE PlanModules (
        PlanModuleId INT IDENTITY(1,1) PRIMARY KEY,
        PlanId INT NOT NULL,
        ModuleId INT NOT NULL,
        IsIncluded BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_PlanModules_Plan FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(PlanId),
        CONSTRAINT FK_PlanModules_Module FOREIGN KEY (ModuleId) REFERENCES SystemModules(ModuleId),
        CONSTRAINT UQ_PlanModules UNIQUE (PlanId, ModuleId)
    );
    PRINT 'Created table: PlanModules';
END
GO

-- =============================================
-- SAAS CLIENTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaSClients' AND xtype='U')
BEGIN
    CREATE TABLE SaaSClients (
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
    PRINT 'Created table: SaaSClients';
END
GO

-- =============================================
-- CLIENT USERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ClientUsers' AND xtype='U')
BEGIN
    CREATE TABLE ClientUsers (
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
        CONSTRAINT FK_ClientUsers_Client FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
        CONSTRAINT UQ_ClientUsers_Email UNIQUE (ClientId, Email)
    );
    PRINT 'Created table: ClientUsers';
END
GO

-- =============================================
-- CLIENT SUBSCRIPTIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ClientSubscriptions' AND xtype='U')
BEGIN
    CREATE TABLE ClientSubscriptions (
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
        CONSTRAINT FK_ClientSubscriptions_Client FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
        CONSTRAINT FK_ClientSubscriptions_Plan FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(PlanId)
    );
    PRINT 'Created table: ClientSubscriptions';
END
GO

-- =============================================
-- CLIENT MODULES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ClientModules' AND xtype='U')
BEGIN
    CREATE TABLE ClientModules (
        ClientModuleId INT IDENTITY(1,1) PRIMARY KEY,
        ClientId INT NOT NULL,
        ModuleId INT NOT NULL,
        IsEnabled BIT NOT NULL DEFAULT 1,
        AddedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME,
        CustomPrice DECIMAL(18,2),
        LicenseType NVARCHAR(50) DEFAULT 'Subscription',
        CONSTRAINT FK_ClientModules_Client FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
        CONSTRAINT FK_ClientModules_Module FOREIGN KEY (ModuleId) REFERENCES SystemModules(ModuleId),
        CONSTRAINT UQ_ClientModules UNIQUE (ClientId, ModuleId)
    );
    PRINT 'Created table: ClientModules';
END
GO

-- =============================================
-- PAYMENT METHODS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PaymentMethods' AND xtype='U')
BEGIN
    CREATE TABLE PaymentMethods (
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
    PRINT 'Created table: PaymentMethods';
END
GO

-- =============================================
-- INVOICES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Invoices' AND xtype='U')
BEGIN
    CREATE TABLE Invoices (
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
        CONSTRAINT FK_Invoices_Client FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
        CONSTRAINT FK_Invoices_Subscription FOREIGN KEY (SubscriptionId) REFERENCES ClientSubscriptions(SubscriptionId)
    );
    PRINT 'Created table: Invoices';
END
GO

-- =============================================
-- INVOICE ITEMS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InvoiceItems' AND xtype='U')
BEGIN
    CREATE TABLE InvoiceItems (
        InvoiceItemId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceId INT NOT NULL,
        ModuleId INT,
        Description NVARCHAR(500) NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        Discount DECIMAL(18,2) DEFAULT 0,
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        ItemType NVARCHAR(50) DEFAULT 'Subscription',
        CONSTRAINT FK_InvoiceItems_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId) ON DELETE CASCADE,
        CONSTRAINT FK_InvoiceItems_Module FOREIGN KEY (ModuleId) REFERENCES SystemModules(ModuleId)
    );
    PRINT 'Created table: InvoiceItems';
END
GO

-- =============================================
-- SAAS TRANSACTIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaSTransactions' AND xtype='U')
BEGIN
    CREATE TABLE SaaSTransactions (
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
        CONSTRAINT FK_SaaSTransactions_Client FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
        CONSTRAINT FK_SaaSTransactions_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId),
        CONSTRAINT FK_SaaSTransactions_PaymentMethod FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(PaymentMethodId)
    );
    PRINT 'Created table: SaaSTransactions';
END
GO

-- =============================================
-- SUPPORT TICKETS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SupportTickets' AND xtype='U')
BEGIN
    CREATE TABLE SupportTickets (
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
        CONSTRAINT FK_SupportTickets_Client FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
        CONSTRAINT FK_SupportTickets_User FOREIGN KEY (UserId) REFERENCES ClientUsers(ClientUserId)
    );
    PRINT 'Created table: SupportTickets';
END
GO

-- =============================================
-- TICKET COMMENTS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TicketComments' AND xtype='U')
BEGIN
    CREATE TABLE TicketComments (
        CommentId INT IDENTITY(1,1) PRIMARY KEY,
        TicketId INT NOT NULL,
        UserId INT,
        OwnerId INT,
        Comment NVARCHAR(4000) NOT NULL,
        IsInternal BIT NOT NULL DEFAULT 0,
        AttachmentPath NVARCHAR(500),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TicketComments_Ticket FOREIGN KEY (TicketId) REFERENCES SupportTickets(TicketId) ON DELETE CASCADE
    );
    PRINT 'Created table: TicketComments';
END
GO

-- =============================================
-- ACTIVITY LOG TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaaSActivityLog' AND xtype='U')
BEGIN
    CREATE TABLE SaaSActivityLog (
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
    PRINT 'Created table: SaaSActivityLog';
END
GO

-- =============================================
-- LICENSE KEYS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LicenseKeys' AND xtype='U')
BEGIN
    CREATE TABLE LicenseKeys (
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
        CONSTRAINT FK_LicenseKeys_Client FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId)
    );
    PRINT 'Created table: LicenseKeys';
END
GO

-- =============================================
-- INDEXES (with IF NOT EXISTS checks)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaSClients_Status')
    CREATE NONCLUSTERED INDEX IX_SaaSClients_Status ON SaaSClients(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaSClients_Email')
    CREATE NONCLUSTERED INDEX IX_SaaSClients_Email ON SaaSClients(PrimaryEmail);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientSubscriptions_Client')
    CREATE NONCLUSTERED INDEX IX_ClientSubscriptions_Client ON ClientSubscriptions(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientSubscriptions_Status')
    CREATE NONCLUSTERED INDEX IX_ClientSubscriptions_Status ON ClientSubscriptions(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_Client')
    CREATE NONCLUSTERED INDEX IX_Invoices_Client ON Invoices(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_Status')
    CREATE NONCLUSTERED INDEX IX_Invoices_Status ON Invoices(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_DueDate')
    CREATE NONCLUSTERED INDEX IX_Invoices_DueDate ON Invoices(DueDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaSTransactions_Client')
    CREATE NONCLUSTERED INDEX IX_SaaSTransactions_Client ON SaaSTransactions(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SaaSTransactions_Date')
    CREATE NONCLUSTERED INDEX IX_SaaSTransactions_Date ON SaaSTransactions(TransactionDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SupportTickets_Client')
    CREATE NONCLUSTERED INDEX IX_SupportTickets_Client ON SupportTickets(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SupportTickets_Status')
    CREATE NONCLUSTERED INDEX IX_SupportTickets_Status ON SupportTickets(Status);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientModules_Client')
    CREATE NONCLUSTERED INDEX IX_ClientModules_Client ON ClientModules(ClientId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LicenseKeys_Client')
    CREATE NONCLUSTERED INDEX IX_LicenseKeys_Client ON LicenseKeys(ClientId);
GO

PRINT 'Indexes created successfully';
GO

-- =============================================
-- VIEWS
-- =============================================
CREATE OR ALTER VIEW vw_ClientOverview AS
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
    (SELECT COUNT(*) FROM ClientModules cm WHERE cm.ClientId = c.ClientId AND cm.IsEnabled = 1) AS ActiveModules,
    (SELECT COUNT(*) FROM ClientUsers cu WHERE cu.ClientId = c.ClientId AND cu.IsActive = 1) AS ActiveUsers
FROM SaaSClients c
LEFT JOIN ClientSubscriptions cs ON c.ClientId = cs.ClientId AND cs.Status = 'Active'
LEFT JOIN SubscriptionPlans sp ON cs.PlanId = sp.PlanId;
GO

CREATE OR ALTER VIEW vw_MonthlyRevenue AS
SELECT 
    YEAR(t.TransactionDate) AS Year,
    MONTH(t.TransactionDate) AS Month,
    SUM(CASE WHEN t.TransactionType = 'Payment' THEN t.Amount ELSE 0 END) AS TotalPayments,
    SUM(CASE WHEN t.TransactionType = 'Refund' THEN t.Amount ELSE 0 END) AS TotalRefunds,
    SUM(CASE WHEN t.TransactionType = 'Payment' THEN t.Amount ELSE 0 END) - 
    SUM(CASE WHEN t.TransactionType = 'Refund' THEN t.Amount ELSE 0 END) AS NetRevenue,
    COUNT(*) AS TransactionCount
FROM SaaSTransactions t
WHERE t.Status = 'Completed'
GROUP BY YEAR(t.TransactionDate), MONTH(t.TransactionDate);
GO

CREATE OR ALTER VIEW vw_PendingInvoices AS
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
FROM Invoices i
JOIN SaaSClients c ON i.ClientId = c.ClientId
WHERE i.Status IN ('Pending', 'Sent', 'Overdue');
GO

CREATE OR ALTER VIEW vw_ModuleUsage AS
SELECT 
    m.ModuleId,
    m.ModuleCode,
    m.ModuleName,
    m.Category,
    m.MonthlyPrice,
    COUNT(cm.ClientModuleId) AS TotalClients,
    SUM(CASE WHEN cm.IsEnabled = 1 THEN 1 ELSE 0 END) AS ActiveClients
FROM SystemModules m
LEFT JOIN ClientModules cm ON m.ModuleId = cm.ModuleId
GROUP BY m.ModuleId, m.ModuleCode, m.ModuleName, m.Category, m.MonthlyPrice;
GO

PRINT 'Views created successfully';
GO

-- =============================================
-- SEED DATA: Default Owner
-- =============================================
IF NOT EXISTS (SELECT 1 FROM SystemOwners WHERE Username = 'admin')
BEGIN
    INSERT INTO SystemOwners (Username, PasswordHash, Email, FullName, CompanyName, IsActive)
    VALUES ('admin', 'admin123', 'admin@erpsolutions.com', 'System Administrator', 'ERP Solutions Provider', 1);
    PRINT 'Inserted default admin user';
END
GO

-- =============================================
-- SEED DATA: Payment Methods
-- =============================================
IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE MethodCode = 'GCASH')
BEGIN
    INSERT INTO PaymentMethods (MethodCode, MethodName, MethodType, Description, AccountNumber, Instructions, IsActive, SortOrder)
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
IF NOT EXISTS (SELECT 1 FROM SystemModules WHERE ModuleCode = 'CORE')
BEGIN
    INSERT INTO SystemModules (ModuleCode, ModuleName, Description, Category, MonthlyPrice, YearlyPrice, IsCore, IsActive, SortOrder, DisplayOrder)
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
IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans WHERE PlanCode = 'BASIC')
BEGIN
    INSERT INTO SubscriptionPlans (PlanCode, PlanName, Description, MonthlyPrice, YearlyPrice, SetupFee, MaxUsers, MaxBranches, Features, SupportLevel, IsActive, IsPopular, SortOrder)
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
IF NOT EXISTS (SELECT 1 FROM SaaSClients WHERE ClientCode = 'DEMO001')
BEGIN
    INSERT INTO SaaSClients (ClientCode, CompanyName, PrimaryEmail, Status, IsActive)
    VALUES ('DEMO001', 'Demo Company Inc.', 'demo@company.com', 'Active', 1);
    PRINT 'Inserted demo client';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'SaaS Schema created successfully!';
PRINT '========================================';
PRINT 'Admin login: admin / admin123';
PRINT 'Client demo: demo@company.com / demo123';
PRINT '========================================';
GO
