using System;
using System.Collections.Generic;

namespace FinanceBank.Models
{
    // Transaction Models - Aligned across all roles
    public class TransactionBase
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = ""; // Deposit, Withdrawal, Transfer, Bills, etc.
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed, Rejected
        public string CustomerName { get; set; } = "";
        public string CustomerAccount { get; set; } = "";
        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedDate { get; set; }
        public string RejectionReason { get; set; } = "";
        public string Department { get; set; } = ""; // For employee workflow
    }

    // Deposit Transaction
    public class DepositTransaction
    {
        // Inherited properties explicitly defined
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Pending";
        public string CustomerName { get; set; } = "";
        public string CustomerAccount { get; set; } = "";
        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedDate { get; set; }
        public string RejectionReason { get; set; } = "";
        public string Department { get; set; } = "";
        
        // Deposit-specific properties
        public string DepositType { get; set; } = ""; // Cash, Check, Online Transfer
        public string ReferenceNumber { get; set; } = "";
    }

    // Withdrawal Transaction
    public class WithdrawalTransaction
    {
        // Inherited properties explicitly defined
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Pending";
        public string CustomerName { get; set; } = "";
        public string CustomerAccount { get; set; } = "";
        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedDate { get; set; }
        public string RejectionReason { get; set; } = "";
        public string Department { get; set; } = "";
        
        // Withdrawal-specific properties
        public string WithdrawalMethod { get; set; } = ""; // ATM, Teller, Online
        public string Purpose { get; set; } = "";
    }

    // Transfer Transaction
    public class TransferTransaction
    {
        // Inherited properties explicitly defined
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Pending";
        public string CustomerName { get; set; } = "";
        public string CustomerAccount { get; set; } = "";
        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedDate { get; set; }
        public string RejectionReason { get; set; } = "";
        public string Department { get; set; } = "";
        
        // Transfer-specific properties
        public string FromAccount { get; set; } = "";
        public string ToAccount { get; set; } = "";
        public string RecipientName { get; set; } = "";
        public string Purpose { get; set; } = "";
        public string TransferType { get; set; } = ""; // Internal, External, International
    }

    // Bills Payment Transaction
    public class BillsPaymentTransaction : TransactionBase
    {
        public string BillerName { get; set; } = "";
        public string BillerCategory { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public string ReferenceNumber { get; set; } = "";
    }

    // Loan Application
    public class LoanApplication
    {
        public string Id { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerAccount { get; set; } = "";
        public string LoanType { get; set; } = ""; // Personal, Home, Auto, Education
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public decimal InterestRate { get; set; }
        public string Purpose { get; set; } = "";
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, UnderReview, Approved, Rejected, Disbursed
        public string ReviewedBy { get; set; } = "";
        public DateTime? ReviewDate { get; set; }
        public string Department { get; set; } = "Loans"; // For employee workflow
        public string Comments { get; set; } = "";
    }

    // Card Application
    public class CardApplication
    {
        public string Id { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerAccount { get; set; } = "";
        public string CardType { get; set; } = ""; // Credit, Debit
        public string CardTier { get; set; } = ""; // Classic, Gold, Platinum
        public decimal AnnualIncome { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string ReviewedBy { get; set; } = "";
        public DateTime? ReviewDate { get; set; }
        public string Department { get; set; } = "Cards";
    }

    // Account Opening Application
    public class AccountApplication
    {
        public string Id { get; set; } = "";
        public string ApplicantName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string AccountType { get; set; } = ""; // Savings, Checking, Current
        public decimal InitialDeposit { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string ProcessedBy { get; set; } = "";
        public DateTime? ProcessedDate { get; set; }
        public string Department { get; set; } = "Accounts";
    }

    // Savings Goal
    public class SavingsGoal
    {
        public string Id { get; set; } = "";
        public string CustomerAccount { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Current { get; set; }
        public decimal Target { get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // Reward Points
    public class RewardPoints
    {
        public string CustomerAccount { get; set; } = "";
        public int TotalPoints { get; set; }
        public decimal CashbackThisMonth { get; set; }
        public int TransactionsCount { get; set; }
    }

    // Approval Queue Item (for admin/employee)
    public class ApprovalQueueItem
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = ""; // Transaction, Loan, Card, Account
        public string CustomerName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
        public string Department { get; set; } = "";
        public string AssignedTo { get; set; } = "";
    }

    // User Model (for all roles)
    public class User
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = ""; // Customer, Employee, Admin
        public string Department { get; set; } = ""; // For employees
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Account Model
    public class Account
    {
        public string AccountNumber { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public decimal Balance { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedDate { get; set; }
    }

    // Department Enum for workflow
    public enum Department
    {
        Accounts,
        Loans,
        Cards,
        Transactions,
        CustomerService,
        Operations,
        Compliance
    }

    // Status Enum
    public enum TransactionStatus
    {
        Pending,
        UnderReview,
        Approved,
        Completed,
        Rejected,
        Cancelled
    }
}
