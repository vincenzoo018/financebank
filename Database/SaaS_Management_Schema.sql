-- =============================================
-- SaaS Management System Database Schema
-- For ERP System Owner/Vendor Portal
-- =============================================
-- This schema manages:
-- 1. Client/Company management
-- 2. Module management and pricing
-- 3. Subscription plans and bundles
-- 4. Transactions and payments
-- 5. Invoices and billing
-- 6. Support tickets
-- 7. License validation
-- =============================================

-- =============================================
-- SYSTEM OWNER TABLE
-- =============================================
CREATE TABLE IF NOT EXISTS SystemOwners (
    OwnerId INTEGER PRIMARY KEY AUTOINCREMENT,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50),
    CompanyName NVARCHAR(200) NOT NULL DEFAULT 'ERP Solutions Provider',
    CompanyAddress NVARCHAR(500),
    CompanyLogo NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME,
    UNIQUE(Email)
);

-- =============================================
-- SYSTEM MODULES TABLE
-- Available modules in the ERP system
-- =============================================
CREATE TABLE IF NOT EXISTS SystemModules (
    ModuleId INTEGER PRIMARY KEY AUTOINCREMENT,
    ModuleCode NVARCHAR(50) NOT NULL UNIQUE,
    ModuleName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Category NVARCHAR(100), -- 'Core', 'Finance', 'HR', 'Inventory', 'Reports'
    BasePrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    MonthlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    YearlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsCore BIT NOT NULL DEFAULT 0, -- Core modules included in all plans
    IsActive BIT NOT NULL DEFAULT 1,
    IconClass NVARCHAR(100), -- CSS icon class
    SortOrder INTEGER NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME
);

-- =============================================
-- SUBSCRIPTION PLANS TABLE
-- Tiered plans (Basic, Pro, Enterprise)
-- =============================================
CREATE TABLE IF NOT EXISTS SubscriptionPlans (
    PlanId INTEGER PRIMARY KEY AUTOINCREMENT,
    PlanCode NVARCHAR(50) NOT NULL UNIQUE,
    PlanName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    MonthlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    YearlyPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    SetupFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxUsers INTEGER NOT NULL DEFAULT 5,
    MaxTransactionsPerMonth INTEGER,
    MaxStorageGB INTEGER DEFAULT 5,
    IncludesSupport BIT NOT NULL DEFAULT 1,
    SupportLevel NVARCHAR(50) DEFAULT 'Basic', -- 'Basic', 'Priority', 'Premium'
    IsActive BIT NOT NULL DEFAULT 1,
    IsPopular BIT NOT NULL DEFAULT 0,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME
);

-- =============================================
-- PLAN MODULES TABLE
-- Which modules are included in each plan
-- =============================================
CREATE TABLE IF NOT EXISTS PlanModules (
    PlanModuleId INTEGER PRIMARY KEY AUTOINCREMENT,
    PlanId INTEGER NOT NULL,
    ModuleId INTEGER NOT NULL,
    IsIncluded BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(PlanId) ON DELETE CASCADE,
    FOREIGN KEY (ModuleId) REFERENCES SystemModules(ModuleId) ON DELETE CASCADE,
    UNIQUE(PlanId, ModuleId)
);

-- =============================================
-- CLIENTS TABLE
-- Companies/Organizations using the system
-- =============================================
CREATE TABLE IF NOT EXISTS SaaSClients (
    ClientId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientCode NVARCHAR(50) NOT NULL UNIQUE,
    CompanyName NVARCHAR(300) NOT NULL,
    TradeName NVARCHAR(300),
    BusinessType NVARCHAR(100), -- 'Bank', 'Cooperative', 'Lending Company', 'Microfinance'
    TaxId NVARCHAR(100),
    
    -- Address
    Address NVARCHAR(500),
    City NVARCHAR(100),
    Province NVARCHAR(100),
    PostalCode NVARCHAR(20),
    Country NVARCHAR(100) DEFAULT 'Philippines',
    
    -- Contact Information
    PrimaryEmail NVARCHAR(256) NOT NULL,
    SecondaryEmail NVARCHAR(256),
    Phone NVARCHAR(50),
    Mobile NVARCHAR(50),
    Website NVARCHAR(256),
    
    -- Contact Person
    ContactPersonName NVARCHAR(200),
    ContactPersonTitle NVARCHAR(100),
    ContactPersonEmail NVARCHAR(256),
    ContactPersonPhone NVARCHAR(50),
    
    -- System Access
    DatabaseName NVARCHAR(100), -- Client's dedicated database name
    SystemUrl NVARCHAR(256), -- Client's system URL
    LicenseKey NVARCHAR(256),
    
    -- Subscription Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- 'Active', 'Suspended', 'Cancelled', 'Pending', 'Trial'
    TrialEndsAt DATETIME,
    SubscriptionStartDate DATETIME,
    SubscriptionEndDate DATETIME,
    
    -- Billing
    BillingCycle NVARCHAR(50) DEFAULT 'Monthly', -- 'Monthly', 'Quarterly', 'Yearly'
    BillingDay INTEGER DEFAULT 1, -- Day of month for billing
    CreditBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    OutstandingBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Logo and Branding
    Logo NVARCHAR(500),
    PrimaryColor NVARCHAR(20),
    
    -- Notes
    Notes NVARCHAR(2000),
    
    -- Audit
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    CreatedBy INTEGER,
    
    FOREIGN KEY (CreatedBy) REFERENCES SystemOwners(OwnerId)
);

-- =============================================
-- CLIENT USERS TABLE
-- Users within each client company
-- =============================================
CREATE TABLE IF NOT EXISTS ClientUsers (
    ClientUserId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Admin', -- 'Admin', 'Manager', 'User'
    Phone NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    IsPrimaryContact BIT NOT NULL DEFAULT 0,
    LastLoginAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId) ON DELETE CASCADE,
    UNIQUE(ClientId, Username),
    UNIQUE(Email)
);

-- =============================================
-- CLIENT SUBSCRIPTIONS TABLE
-- Current active subscription for each client
-- =============================================
CREATE TABLE IF NOT EXISTS ClientSubscriptions (
    SubscriptionId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER NOT NULL,
    PlanId INTEGER,
    
    -- Subscription Period
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    BillingCycle NVARCHAR(50) NOT NULL DEFAULT 'Monthly',
    
    -- Pricing
    BasePrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    AdditionalModulesPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountType NVARCHAR(50), -- 'Percentage', 'Fixed'
    DiscountReason NVARCHAR(200),
    TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- 'Active', 'Expired', 'Cancelled', 'Suspended'
    AutoRenew BIT NOT NULL DEFAULT 1,
    
    -- Limits
    MaxUsers INTEGER NOT NULL DEFAULT 5,
    CurrentUsers INTEGER NOT NULL DEFAULT 1,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    CancelledAt DATETIME,
    CancellationReason NVARCHAR(500),
    
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId) ON DELETE CASCADE,
    FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(PlanId)
);

-- =============================================
-- CLIENT MODULES TABLE
-- Which modules each client has access to
-- =============================================
CREATE TABLE IF NOT EXISTS ClientModules (
    ClientModuleId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER NOT NULL,
    ModuleId INTEGER NOT NULL,
    SubscriptionId INTEGER,
    
    -- Pricing (if custom pricing)
    CustomPrice DECIMAL(18,2),
    IsCustomPrice BIT NOT NULL DEFAULT 0,
    
    -- Access Control
    IsEnabled BIT NOT NULL DEFAULT 1,
    EnabledAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DisabledAt DATETIME,
    
    -- Source
    Source NVARCHAR(50) NOT NULL DEFAULT 'Plan', -- 'Plan', 'AddOn', 'Custom', 'Trial'
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId) ON DELETE CASCADE,
    FOREIGN KEY (ModuleId) REFERENCES SystemModules(ModuleId),
    FOREIGN KEY (SubscriptionId) REFERENCES ClientSubscriptions(SubscriptionId),
    UNIQUE(ClientId, ModuleId)
);

-- =============================================
-- PAYMENT METHODS TABLE
-- Available payment methods
-- =============================================
CREATE TABLE IF NOT EXISTS PaymentMethods (
    PaymentMethodId INTEGER PRIMARY KEY AUTOINCREMENT,
    MethodCode NVARCHAR(50) NOT NULL UNIQUE,
    MethodName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    AccountName NVARCHAR(200),
    AccountNumber NVARCHAR(100),
    BankName NVARCHAR(200),
    Instructions NVARCHAR(1000),
    IconClass NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- INVOICES TABLE
-- Generated invoices for clients
-- =============================================
CREATE TABLE IF NOT EXISTS Invoices (
    InvoiceId INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    ClientId INTEGER NOT NULL,
    SubscriptionId INTEGER,
    
    -- Invoice Details
    InvoiceDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DueDate DATETIME NOT NULL,
    PeriodStart DATETIME,
    PeriodEnd DATETIME,
    
    -- Amounts
    Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Tax DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxRate DECIMAL(5,2) NOT NULL DEFAULT 12.00, -- 12% VAT
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    BalanceDue DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Draft', 'Pending', 'Paid', 'Partial', 'Overdue', 'Cancelled', 'Refunded'
    
    -- Notes
    Notes NVARCHAR(1000),
    InternalNotes NVARCHAR(1000),
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    SentAt DATETIME,
    PaidAt DATETIME,
    
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
    FOREIGN KEY (SubscriptionId) REFERENCES ClientSubscriptions(SubscriptionId)
);

-- =============================================
-- INVOICE ITEMS TABLE
-- Line items on each invoice
-- =============================================
CREATE TABLE IF NOT EXISTS InvoiceItems (
    InvoiceItemId INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceId INTEGER NOT NULL,
    ModuleId INTEGER,
    
    Description NVARCHAR(500) NOT NULL,
    Quantity INTEGER NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    ItemType NVARCHAR(50) NOT NULL DEFAULT 'Subscription', -- 'Subscription', 'Module', 'Setup', 'Support', 'Other'
    
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId) ON DELETE CASCADE,
    FOREIGN KEY (ModuleId) REFERENCES SystemModules(ModuleId)
);

-- =============================================
-- TRANSACTIONS TABLE
-- All payment/transaction records
-- =============================================
CREATE TABLE IF NOT EXISTS SaaSTransactions (
    TransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
    TransactionNumber NVARCHAR(50) NOT NULL UNIQUE,
    ClientId INTEGER NOT NULL,
    InvoiceId INTEGER,
    
    -- Transaction Details
    TransactionDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    TransactionType NVARCHAR(50) NOT NULL, -- 'Payment', 'Refund', 'Credit', 'Adjustment', 'Penalty'
    
    -- Payment Info
    PaymentMethodId INTEGER,
    PaymentReference NVARCHAR(200), -- Reference number from payment provider
    PaymentProof NVARCHAR(500), -- File path to payment proof image
    
    -- Amount
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'PHP',
    
    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Completed', 'Failed', 'Cancelled', 'Refunded'
    
    -- Notes
    Description NVARCHAR(500),
    Notes NVARCHAR(1000),
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ProcessedAt DATETIME,
    ProcessedBy INTEGER,
    
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId),
    FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(PaymentMethodId),
    FOREIGN KEY (ProcessedBy) REFERENCES SystemOwners(OwnerId)
);

-- =============================================
-- SUPPORT TICKETS TABLE
-- Client support requests
-- =============================================
CREATE TABLE IF NOT EXISTS SupportTickets (
    TicketId INTEGER PRIMARY KEY AUTOINCREMENT,
    TicketNumber NVARCHAR(50) NOT NULL UNIQUE,
    ClientId INTEGER NOT NULL,
    ClientUserId INTEGER,
    
    -- Ticket Details
    Subject NVARCHAR(300) NOT NULL,
    Description NVARCHAR(4000) NOT NULL,
    Category NVARCHAR(100), -- 'Technical', 'Billing', 'Feature Request', 'Bug Report', 'General'
    Priority NVARCHAR(50) NOT NULL DEFAULT 'Normal', -- 'Low', 'Normal', 'High', 'Urgent'
    
    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open', -- 'Open', 'In Progress', 'Waiting', 'Resolved', 'Closed'
    
    -- Assignment
    AssignedTo INTEGER,
    
    -- Dates
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    ResolvedAt DATETIME,
    ClosedAt DATETIME,
    
    -- Resolution
    Resolution NVARCHAR(4000),
    SatisfactionRating INTEGER, -- 1-5 stars
    
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId),
    FOREIGN KEY (ClientUserId) REFERENCES ClientUsers(ClientUserId),
    FOREIGN KEY (AssignedTo) REFERENCES SystemOwners(OwnerId)
);

-- =============================================
-- TICKET COMMENTS TABLE
-- Comments/replies on support tickets
-- =============================================
CREATE TABLE IF NOT EXISTS TicketComments (
    CommentId INTEGER PRIMARY KEY AUTOINCREMENT,
    TicketId INTEGER NOT NULL,
    
    -- Author (one of these will be set)
    ClientUserId INTEGER,
    OwnerId INTEGER,
    
    -- Comment
    Comment NVARCHAR(4000) NOT NULL,
    IsInternal BIT NOT NULL DEFAULT 0, -- Internal notes not visible to client
    
    -- Attachment
    Attachment NVARCHAR(500),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (TicketId) REFERENCES SupportTickets(TicketId) ON DELETE CASCADE,
    FOREIGN KEY (ClientUserId) REFERENCES ClientUsers(ClientUserId),
    FOREIGN KEY (OwnerId) REFERENCES SystemOwners(OwnerId)
);

-- =============================================
-- ACTIVITY LOG TABLE
-- Audit trail for all SaaS activities
-- =============================================
CREATE TABLE IF NOT EXISTS SaaSActivityLog (
    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Actor
    OwnerId INTEGER,
    ClientUserId INTEGER,
    ClientId INTEGER,
    
    -- Activity
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100), -- 'Client', 'Subscription', 'Invoice', 'Payment', 'Module'
    EntityId INTEGER,
    
    -- Details
    Description NVARCHAR(1000),
    OldValues NVARCHAR(4000),
    NewValues NVARCHAR(4000),
    
    -- Context
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (OwnerId) REFERENCES SystemOwners(OwnerId),
    FOREIGN KEY (ClientUserId) REFERENCES ClientUsers(ClientUserId),
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId)
);

-- =============================================
-- LICENSE KEYS TABLE
-- For license validation
-- =============================================
CREATE TABLE IF NOT EXISTS LicenseKeys (
    LicenseId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER NOT NULL,
    LicenseKey NVARCHAR(256) NOT NULL UNIQUE,
    
    -- Validity
    IssuedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NOT NULL,
    
    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    LastValidatedAt DATETIME,
    ValidationCount INTEGER NOT NULL DEFAULT 0,
    
    -- Hardware binding (optional)
    MachineId NVARCHAR(256),
    
    FOREIGN KEY (ClientId) REFERENCES SaaSClients(ClientId) ON DELETE CASCADE
);

-- =============================================
-- INSERT DEFAULT DATA
-- =============================================

-- Default System Owner
INSERT INTO SystemOwners (Username, PasswordHash, Email, FullName, CompanyName, CompanyAddress, Phone)
VALUES ('admin', 'admin123', 'admin@erpsolutions.com', 'System Administrator', 'ERP Solutions Inc.', '123 Tech Park, Manila, Philippines', '+63 912 345 6789');

-- Default Payment Methods
INSERT INTO PaymentMethods (MethodCode, MethodName, Description, AccountName, AccountNumber, BankName, Instructions, IsActive, SortOrder) VALUES
('GCASH', 'GCash', 'Pay via GCash mobile wallet', 'ERP Solutions Inc.', '09171234567', NULL, 'Send payment to the GCash number above and upload proof of payment.', 1, 1),
('PAYMAYA', 'PayMaya', 'Pay via PayMaya mobile wallet', 'ERP Solutions Inc.', '09181234567', NULL, 'Send payment to the PayMaya number above and upload proof of payment.', 1, 2),
('BDO', 'BDO Bank Transfer', 'Bank transfer to BDO account', 'ERP Solutions Inc.', '001234567890', 'BDO Unibank', 'Transfer to the account above and upload deposit slip.', 1, 3),
('BPI', 'BPI Bank Transfer', 'Bank transfer to BPI account', 'ERP Solutions Inc.', '1234567890', 'Bank of the Philippine Islands', 'Transfer to the account above and upload deposit slip.', 1, 4),
('METROBANK', 'Metrobank Transfer', 'Bank transfer to Metrobank account', 'ERP Solutions Inc.', '123456789012', 'Metropolitan Bank', 'Transfer to the account above and upload deposit slip.', 1, 5),
('CASH', 'Cash Payment', 'Pay in cash at our office', 'ERP Solutions Inc.', NULL, NULL, 'Visit our office for cash payment. Receipt will be issued upon payment.', 1, 6),
('CHECK', 'Check Payment', 'Pay via check', 'ERP Solutions Inc.', NULL, NULL, 'Make check payable to "ERP Solutions Inc." and mail to our office.', 1, 7);

-- Default System Modules (based on FinanceBank ERP)
INSERT INTO SystemModules (ModuleCode, ModuleName, Description, Category, BasePrice, MonthlyPrice, YearlyPrice, IsCore, SortOrder) VALUES
-- Core Modules
('DASHBOARD', 'Dashboard', 'Main dashboard with overview and statistics', 'Core', 0, 0, 0, 1, 1),
('USER_MGMT', 'User Management', 'Manage users, roles, and permissions', 'Core', 0, 0, 0, 1, 2),
('SETTINGS', 'System Settings', 'Configure system settings and preferences', 'Core', 0, 0, 0, 1, 3),

-- Customer Management
('CUSTOMER_REG', 'Customer Registration', 'Register and manage customer profiles', 'Customer', 1000, 500, 5000, 0, 10),
('CUSTOMER_ACCT', 'Customer Accounts', 'Manage customer accounts and balances', 'Customer', 1500, 750, 7500, 0, 11),

-- Teller Operations
('TELLER_DASHBOARD', 'Teller Dashboard', 'Teller workstation and operations', 'Teller', 2000, 1000, 10000, 0, 20),
('DEPOSITS', 'Deposit Processing', 'Process customer deposits', 'Teller', 1500, 750, 7500, 0, 21),
('WITHDRAWALS', 'Withdrawal Processing', 'Process customer withdrawals', 'Teller', 1500, 750, 7500, 0, 22),
('TRANSFERS', 'Fund Transfers', 'Process fund transfers between accounts', 'Teller', 1500, 750, 7500, 0, 23),
('PAYMENT_PROC', 'Payment Processing', 'Process various payments', 'Teller', 1500, 750, 7500, 0, 24),

-- Savings Module
('SAVINGS', 'Savings Accounts', 'Manage savings accounts and interest', 'Savings', 2500, 1250, 12500, 0, 30),
('SAVINGS_INTEREST', 'Interest Posting', 'Calculate and post savings interest', 'Savings', 1500, 750, 7500, 0, 31),

-- Loan Module
('LOAN_MGMT', 'Loan Management', 'Manage loan applications and approvals', 'Loans', 3000, 1500, 15000, 0, 40),
('LOAN_RELEASE', 'Loan Release', 'Process loan disbursements', 'Loans', 2000, 1000, 10000, 0, 41),
('LOAN_PAYMENTS', 'Loan Payments', 'Process loan repayments', 'Loans', 2000, 1000, 10000, 0, 42),
('LOAN_REVIEW', 'Loan Review', 'Review and approve loan applications', 'Loans', 1500, 750, 7500, 0, 43),

-- Accounting Module
('CHART_ACCTS', 'Chart of Accounts', 'Manage chart of accounts', 'Accounting', 2000, 1000, 10000, 0, 50),
('JOURNAL', 'Journal Entries', 'Record journal entries', 'Accounting', 2000, 1000, 10000, 0, 51),
('GENERAL_LEDGER', 'General Ledger', 'View and manage general ledger', 'Accounting', 2500, 1250, 12500, 0, 52),
('TRIAL_BALANCE', 'Trial Balance', 'Generate trial balance reports', 'Accounting', 1500, 750, 7500, 0, 53),
('FIN_STATEMENTS', 'Financial Statements', 'Generate financial statements', 'Accounting', 2500, 1250, 12500, 0, 54),

-- Finance Manager Module
('FM_DASHBOARD', 'Finance Manager Dashboard', 'Finance manager overview and controls', 'Finance', 3000, 1500, 15000, 0, 60),
('BUDGET_MGMT', 'Budget Management', 'Manage budgets and allocations', 'Finance', 2500, 1250, 12500, 0, 61),
('CASHFLOW', 'Cash Flow Analysis', 'Analyze cash flow and projections', 'Finance', 2500, 1250, 12500, 0, 62),
('ACCTS_PAYABLE', 'Accounts Payable', 'Manage accounts payable', 'Finance', 2000, 1000, 10000, 0, 63),
('ACCTS_RECEIVABLE', 'Accounts Receivable', 'Manage accounts receivable', 'Finance', 2000, 1000, 10000, 0, 64),
('FORECASTING', 'Financial Forecasting', 'Financial projections and forecasting', 'Finance', 3000, 1500, 15000, 0, 65),

-- Reports Module
('REPORTS_BASIC', 'Basic Reports', 'Standard operational reports', 'Reports', 1500, 750, 7500, 0, 70),
('REPORTS_ADV', 'Advanced Reports', 'Advanced analytics and reports', 'Reports', 2500, 1250, 12500, 0, 71),
('AUDIT_TRAIL', 'Audit Trail', 'System audit logs and tracking', 'Reports', 2000, 1000, 10000, 0, 72),

-- Admin Module
('ADMIN_FULL', 'Full Administration', 'Complete administrative controls', 'Admin', 5000, 2500, 25000, 0, 80),
('DB_SYNC', 'Database Sync', 'Database synchronization tools', 'Admin', 2000, 1000, 10000, 0, 81),
('SECURITY', 'Security Center', 'Security management and monitoring', 'Admin', 2500, 1250, 12500, 0, 82);

-- Default Subscription Plans
INSERT INTO SubscriptionPlans (PlanCode, PlanName, Description, MonthlyPrice, YearlyPrice, SetupFee, MaxUsers, MaxTransactionsPerMonth, MaxStorageGB, SupportLevel, IsPopular, SortOrder) VALUES
('BASIC', 'Basic Plan', 'Essential features for small organizations. Includes core modules and basic support.', 4999, 49990, 5000, 5, 1000, 5, 'Basic', 0, 1),
('PRO', 'Professional Plan', 'Advanced features for growing businesses. Includes most modules and priority support.', 9999, 99990, 10000, 15, 5000, 20, 'Priority', 1, 2),
('ENTERPRISE', 'Enterprise Plan', 'Full-featured solution for large organizations. All modules with premium support.', 24999, 249990, 25000, 50, NULL, 100, 'Premium', 0, 3),
('CUSTOM', 'Custom Plan', 'Tailored solution based on your specific needs. Contact us for pricing.', 0, 0, 0, 0, NULL, NULL, 'Premium', 0, 4);

-- Plan Modules Mapping
-- Basic Plan Modules
INSERT INTO PlanModules (PlanId, ModuleId) 
SELECT 1, ModuleId FROM SystemModules WHERE ModuleCode IN ('DASHBOARD', 'USER_MGMT', 'SETTINGS', 'CUSTOMER_REG', 'CUSTOMER_ACCT', 'TELLER_DASHBOARD', 'DEPOSITS', 'WITHDRAWALS', 'REPORTS_BASIC');

-- Pro Plan Modules
INSERT INTO PlanModules (PlanId, ModuleId)
SELECT 2, ModuleId FROM SystemModules WHERE ModuleCode IN ('DASHBOARD', 'USER_MGMT', 'SETTINGS', 'CUSTOMER_REG', 'CUSTOMER_ACCT', 'TELLER_DASHBOARD', 'DEPOSITS', 'WITHDRAWALS', 'TRANSFERS', 'PAYMENT_PROC', 'SAVINGS', 'SAVINGS_INTEREST', 'LOAN_MGMT', 'LOAN_RELEASE', 'LOAN_PAYMENTS', 'CHART_ACCTS', 'JOURNAL', 'GENERAL_LEDGER', 'REPORTS_BASIC', 'REPORTS_ADV', 'AUDIT_TRAIL');

-- Enterprise Plan Modules (All modules)
INSERT INTO PlanModules (PlanId, ModuleId)
SELECT 3, ModuleId FROM SystemModules;

-- Sample Client
INSERT INTO SaaSClients (ClientCode, CompanyName, TradeName, BusinessType, Address, City, Province, PrimaryEmail, Phone, ContactPersonName, ContactPersonTitle, ContactPersonEmail, Status, BillingCycle, SubscriptionStartDate, SubscriptionEndDate) VALUES
('CLT-001', 'FineBank Financial Services', 'FineBank', 'Microfinance', '456 Financial District, Makati City', 'Makati City', 'Metro Manila', 'info@finebank.com', '+63 2 8888 9999', 'Juan Dela Cruz', 'IT Manager', 'juan@finebank.com', 'Active', 'Monthly', datetime('now'), datetime('now', '+1 year'));

-- Sample Client User
INSERT INTO ClientUsers (ClientId, Username, PasswordHash, Email, FullName, Role, IsPrimaryContact) VALUES
(1, 'admin', 'admin123', 'admin@finebank.com', 'FineBank Admin', 'Admin', 1);

-- Sample Client Subscription
INSERT INTO ClientSubscriptions (ClientId, PlanId, StartDate, EndDate, BillingCycle, BasePrice, TotalPrice, MaxUsers, CurrentUsers) VALUES
(1, 2, datetime('now'), datetime('now', '+1 month'), 'Monthly', 9999, 9999, 15, 5);

-- Sample Client Modules (from Pro Plan)
INSERT INTO ClientModules (ClientId, ModuleId, SubscriptionId, Source)
SELECT 1, ModuleId, 1, 'Plan' FROM PlanModules WHERE PlanId = 2;

-- Sample Invoice
INSERT INTO Invoices (InvoiceNumber, ClientId, SubscriptionId, InvoiceDate, DueDate, PeriodStart, PeriodEnd, Subtotal, Tax, TotalAmount, BalanceDue, Status) VALUES
('INV-2024-0001', 1, 1, datetime('now'), datetime('now', '+15 days'), datetime('now'), datetime('now', '+1 month'), 8927.68, 1071.32, 9999.00, 9999.00, 'Pending');

-- Sample Invoice Items
INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, Amount, ItemType) VALUES
(1, 'Professional Plan - Monthly Subscription', 1, 8927.68, 8927.68, 'Subscription');

-- Generate License Key for Sample Client
INSERT INTO LicenseKeys (ClientId, LicenseKey, ExpiresAt) VALUES
(1, 'FB-PRO-' || hex(randomblob(8)) || '-2024', datetime('now', '+1 year'));

-- =============================================
-- VIEWS FOR REPORTING
-- =============================================

-- Client Overview View
CREATE VIEW IF NOT EXISTS vw_ClientOverview AS
SELECT 
    c.ClientId,
    c.ClientCode,
    c.CompanyName,
    c.Status,
    c.PrimaryEmail,
    c.Phone,
    c.ContactPersonName,
    cs.PlanId,
    sp.PlanName,
    cs.StartDate AS SubscriptionStart,
    cs.EndDate AS SubscriptionEnd,
    cs.TotalPrice AS MonthlyRate,
    c.OutstandingBalance,
    (SELECT COUNT(*) FROM ClientModules cm WHERE cm.ClientId = c.ClientId AND cm.IsEnabled = 1) AS ActiveModules,
    (SELECT COUNT(*) FROM ClientUsers cu WHERE cu.ClientId = c.ClientId AND cu.IsActive = 1) AS ActiveUsers
FROM SaaSClients c
LEFT JOIN ClientSubscriptions cs ON c.ClientId = cs.ClientId AND cs.Status = 'Active'
LEFT JOIN SubscriptionPlans sp ON cs.PlanId = sp.PlanId;

-- Monthly Revenue View
CREATE VIEW IF NOT EXISTS vw_MonthlyRevenue AS
SELECT 
    strftime('%Y-%m', t.TransactionDate) AS Month,
    COUNT(*) AS TransactionCount,
    SUM(CASE WHEN t.TransactionType = 'Payment' THEN t.Amount ELSE 0 END) AS TotalPayments,
    SUM(CASE WHEN t.TransactionType = 'Refund' THEN t.Amount ELSE 0 END) AS TotalRefunds,
    SUM(CASE WHEN t.TransactionType = 'Payment' THEN t.Amount ELSE -t.Amount END) AS NetRevenue
FROM SaaSTransactions t
WHERE t.Status = 'Completed'
GROUP BY strftime('%Y-%m', t.TransactionDate);

-- Pending Invoices View
CREATE VIEW IF NOT EXISTS vw_PendingInvoices AS
SELECT 
    i.*,
    c.CompanyName,
    c.PrimaryEmail,
    c.ContactPersonName,
    julianday(i.DueDate) - julianday('now') AS DaysUntilDue
FROM Invoices i
JOIN SaaSClients c ON i.ClientId = c.ClientId
WHERE i.Status IN ('Pending', 'Overdue', 'Partial')
ORDER BY i.DueDate;

-- Module Usage View
CREATE VIEW IF NOT EXISTS vw_ModuleUsage AS
SELECT 
    m.ModuleId,
    m.ModuleCode,
    m.ModuleName,
    m.Category,
    m.MonthlyPrice,
    COUNT(DISTINCT cm.ClientId) AS ClientCount,
    SUM(CASE WHEN cm.IsEnabled = 1 THEN 1 ELSE 0 END) AS ActiveCount
FROM SystemModules m
LEFT JOIN ClientModules cm ON m.ModuleId = cm.ModuleId
GROUP BY m.ModuleId, m.ModuleCode, m.ModuleName, m.Category, m.MonthlyPrice;

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================
CREATE INDEX IF NOT EXISTS idx_clients_status ON SaaSClients(Status);
CREATE INDEX IF NOT EXISTS idx_clients_email ON SaaSClients(PrimaryEmail);
CREATE INDEX IF NOT EXISTS idx_subscriptions_client ON ClientSubscriptions(ClientId);
CREATE INDEX IF NOT EXISTS idx_subscriptions_status ON ClientSubscriptions(Status);
CREATE INDEX IF NOT EXISTS idx_invoices_client ON Invoices(ClientId);
CREATE INDEX IF NOT EXISTS idx_invoices_status ON Invoices(Status);
CREATE INDEX IF NOT EXISTS idx_invoices_duedate ON Invoices(DueDate);
CREATE INDEX IF NOT EXISTS idx_transactions_client ON SaaSTransactions(ClientId);
CREATE INDEX IF NOT EXISTS idx_transactions_date ON SaaSTransactions(TransactionDate);
CREATE INDEX IF NOT EXISTS idx_tickets_client ON SupportTickets(ClientId);
CREATE INDEX IF NOT EXISTS idx_tickets_status ON SupportTickets(Status);
CREATE INDEX IF NOT EXISTS idx_clientmodules_client ON ClientModules(ClientId);
CREATE INDEX IF NOT EXISTS idx_license_client ON LicenseKeys(ClientId);
