using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service for managing Asset Marketplace operations
    /// </summary>
    public class AssetMarketplaceService
    {
        private readonly IDbContextFactory<BFASDbContext> _contextFactory;
        private readonly LoanService _loanService;
        private readonly AutomaticGLPostingService _glPostingService;

        public AssetMarketplaceService(
            IDbContextFactory<BFASDbContext> contextFactory, 
            LoanService loanService,
            AutomaticGLPostingService glPostingService)
        {
            _contextFactory = contextFactory;
            _loanService = loanService;
            _glPostingService = glPostingService;
        }

        #region Asset Management (Admin)

        /// <summary>
        /// Get all assets with optional filtering
        /// </summary>
        public async Task<List<Asset>> GetAssetsAsync(string? assetType = null, string? status = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Assets.Include(a => a.Images).AsQueryable();

            if (!string.IsNullOrEmpty(assetType))
                query = query.Where(a => a.AssetType == assetType);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Get available assets for customer marketplace
        /// </summary>
        public async Task<List<Asset>> GetAvailableAssetsAsync(string? assetType = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Assets
                .Include(a => a.Images)
                .Where(a => a.Status == "Available")
                .AsQueryable();

            if (!string.IsNullOrEmpty(assetType))
                query = query.Where(a => a.AssetType == assetType);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Get asset by ID with images
        /// </summary>
        public async Task<Asset?> GetAssetByIdAsync(int assetId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Assets
                .Include(a => a.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(a => a.AssetId == assetId);
        }

        /// <summary>
        /// Create a new asset
        /// </summary>
        public async Task<Asset> CreateAssetAsync(Asset asset, string createdBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            asset.CreatedBy = createdBy;
            asset.CreatedAt = DateTime.Now;
            asset.DownPaymentAmount = asset.TotalPrice * (asset.DownPaymentPercent / 100);

            context.Assets.Add(asset);
            await context.SaveChangesAsync();

            return asset;
        }

        /// <summary>
        /// Update an existing asset
        /// </summary>
        public async Task<Asset> UpdateAssetAsync(Asset asset, string updatedBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.Assets.FindAsync(asset.AssetId);
            if (existing == null)
                throw new Exception("Asset not found");

            // Update all properties
            context.Entry(existing).CurrentValues.SetValues(asset);
            existing.UpdatedBy = updatedBy;
            existing.UpdatedAt = DateTime.Now;
            existing.DownPaymentAmount = existing.TotalPrice * (existing.DownPaymentPercent / 100);

            await context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Delete an asset (soft delete by setting status to Inactive)
        /// </summary>
        public async Task<bool> DeleteAssetAsync(int assetId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var asset = await context.Assets.FindAsync(assetId);
            if (asset == null) return false;

            asset.Status = "Inactive";
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Hard delete asset and its images
        /// </summary>
        public async Task<bool> HardDeleteAssetAsync(int assetId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var asset = await context.Assets
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.AssetId == assetId);
            
            if (asset == null) return false;

            // Remove images first
            if (asset.Images != null)
                context.AssetImages.RemoveRange(asset.Images);

            context.Assets.Remove(asset);
            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Asset Images

        /// <summary>
        /// Add image to asset
        /// </summary>
        public async Task<AssetImage> AddAssetImageAsync(int assetId, byte[] imageData, string fileName, string contentType, bool isPrimary = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Get current max display order
            var maxOrder = await context.AssetImages
                .Where(i => i.AssetId == assetId)
                .MaxAsync(i => (int?)i.DisplayOrder) ?? -1;

            // If setting as primary, unset other primary images
            if (isPrimary)
            {
                var existingPrimary = await context.AssetImages
                    .Where(i => i.AssetId == assetId && i.IsPrimary)
                    .ToListAsync();
                foreach (var img in existingPrimary)
                    img.IsPrimary = false;
            }

            var image = new AssetImage
            {
                AssetId = assetId,
                ImageData = imageData,
                FileName = fileName,
                ContentType = contentType,
                DisplayOrder = maxOrder + 1,
                IsPrimary = isPrimary,
                UploadedAt = DateTime.Now
            };

            context.AssetImages.Add(image);
            await context.SaveChangesAsync();

            return image;
        }

        /// <summary>
        /// Remove image from asset
        /// </summary>
        public async Task<bool> RemoveAssetImageAsync(int imageId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var image = await context.AssetImages.FindAsync(imageId);
            if (image == null) return false;

            context.AssetImages.Remove(image);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Get all images for an asset
        /// </summary>
        public async Task<List<AssetImage>> GetAssetImagesAsync(int assetId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetImages
                .Where(i => i.AssetId == assetId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Set primary image
        /// </summary>
        public async Task SetPrimaryImageAsync(int assetId, int imageId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var images = await context.AssetImages
                .Where(i => i.AssetId == assetId)
                .ToListAsync();

            foreach (var img in images)
                img.IsPrimary = img.ImageId == imageId;

            await context.SaveChangesAsync();
        }

        #endregion

        #region Asset Applications (Customer)

        /// <summary>
        /// Submit asset purchase application
        /// </summary>
        public async Task<AssetApplication> SubmitApplicationAsync(
            int assetId,
            int customerId,
            int customerAccountId,
            string purchaseType,
            decimal downPaymentAmount,
            int termMonths,
            string customerName,
            string customerAddress,
            string customerEmail,
            string customerPhone,
            string employmentStatus,
            decimal monthlyIncome,
            string submittedBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var asset = await context.Assets.FindAsync(assetId);
            if (asset == null)
                throw new Exception("Asset not found");

            if (asset.Status != "Available")
                throw new Exception("Asset is not available for purchase");

            // Calculate loan details
            var loanAmount = asset.TotalPrice - downPaymentAmount;
            var monthlyRate = (asset.InterestRate / 100) / 12;
            decimal monthlyPayment = 0;

            if (purchaseType == "Loan" && termMonths > 0)
            {
                if (monthlyRate > 0)
                {
                    monthlyPayment = loanAmount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), termMonths))
                                     / ((decimal)Math.Pow((double)(1 + monthlyRate), termMonths) - 1);
                }
                else
                {
                    monthlyPayment = loanAmount / termMonths;
                }
            }

            var application = new AssetApplication
            {
                ApplicationNumber = AssetApplication.GenerateApplicationNumber(),
                AssetId = assetId,
                CustomerId = customerId,
                CustomerAccountId = customerAccountId,
                PurchaseType = purchaseType,
                AssetPrice = asset.TotalPrice,
                DownPaymentAmount = downPaymentAmount,
                LoanAmount = loanAmount,
                TermMonths = termMonths,
                InterestRate = asset.InterestRate,
                MonthlyPayment = Math.Round(monthlyPayment, 2),
                Status = "SUBMITTED",
                CustomerName = customerName,
                CustomerAddress = customerAddress,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                EmploymentStatus = employmentStatus,
                MonthlyIncome = monthlyIncome,
                SubmittedBy = submittedBy,
                SubmittedAt = DateTime.Now
            };

            // Mark asset as reserved
            asset.Status = "Reserved";

            context.AssetApplications.Add(application);
            await context.SaveChangesAsync();

            return application;
        }

        /// <summary>
        /// Get applications by customer
        /// </summary>
        public async Task<List<AssetApplication>> GetApplicationsByCustomerAsync(int customerId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetApplications
                .Include(a => a.Asset)
                .ThenInclude(a => a.Images)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get pending applications for teller review
        /// </summary>
        public async Task<List<AssetApplication>> GetPendingTellerReviewAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetApplications
                .Include(a => a.Asset)
                .Include(a => a.Customer)
                .Where(a => a.Status == "SUBMITTED" || a.Status == "PENDING_TELLER_REVIEW")
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications for accountant assessment
        /// </summary>
        public async Task<List<AssetApplication>> GetPendingAccountantAssessmentAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetApplications
                .Include(a => a.Asset)
                .Include(a => a.Customer)
                .Where(a => a.Status == "VERIFIED" || a.Status == "PENDING_ACCOUNTANT_REVIEW")
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get applications for finance manager approval
        /// </summary>
        public async Task<List<AssetApplication>> GetPendingFinanceManagerApprovalAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetApplications
                .Include(a => a.Asset)
                .Include(a => a.Customer)
                .Where(a => a.Status == "ASSESSED" || a.Status == "PENDING_FINANCEMANAGER_APPROVAL")
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get approved applications ready for release
        /// </summary>
        public async Task<List<AssetApplication>> GetApprovedForReleaseAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetApplications
                .Include(a => a.Asset)
                .ThenInclude(a => a.Images)
                .Include(a => a.Customer)
                .Where(a => a.Status == "APPROVED" || a.Status == "APPROVED_READY_FOR_RELEASE")
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Teller reviews application
        /// </summary>
        public async Task<(bool Success, string Message)> TellerReviewAsync(
            int applicationId, bool approved, string remarks, string reviewedBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var application = await context.AssetApplications
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                return (false, "Application not found");

            var previousStatus = application.Status;
            application.TellerReviewedBy = reviewedBy;
            application.TellerReviewedAt = DateTime.Now;
            application.TellerRemarks = remarks;

            if (approved)
            {
                application.Status = "VERIFIED";
                // Log forwarding to accountant
                await LogHistoryAsync(context, application, "FORWARDED_TO_ACCOUNTANT", reviewedBy, "Teller", 
                    remarks, previousStatus, "VERIFIED");
            }
            else
            {
                application.Status = "REJECTED_TELLER";
                // Log rejection
                await LogHistoryAsync(context, application, "REJECTED", reviewedBy, "Teller", 
                    remarks, previousStatus, "REJECTED_TELLER");
                // Make asset available again
                if (application.Asset != null)
                    application.Asset.Status = "Available";
            }

            await context.SaveChangesAsync();
            return (true, approved ? "Application verified and forwarded to accountant" : "Application rejected");
        }

        /// <summary>
        /// Accountant assesses application
        /// </summary>
        public async Task<(bool Success, string Message)> AccountantAssessAsync(
            int applicationId, bool approved, string remarks, string assessedBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var application = await context.AssetApplications
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                return (false, "Application not found");

            var previousStatus = application.Status;
            application.AccountantAssessedBy = assessedBy;
            application.AccountantAssessedAt = DateTime.Now;
            application.AccountantRemarks = remarks;

            if (approved)
            {
                application.Status = "ASSESSED";
                // Log forwarding to finance manager
                await LogHistoryAsync(context, application, "FORWARDED_TO_FINANCEMANAGER", assessedBy, "Accountant", 
                    remarks, previousStatus, "ASSESSED");
            }
            else
            {
                application.Status = "REJECTED_ACCOUNTANT";
                // Log rejection
                await LogHistoryAsync(context, application, "REJECTED", assessedBy, "Accountant", 
                    remarks, previousStatus, "REJECTED_ACCOUNTANT");
                if (application.Asset != null)
                    application.Asset.Status = "Available";
            }

            await context.SaveChangesAsync();
            return (true, approved ? "Application assessed and forwarded to finance manager" : "Application rejected");
        }

        /// <summary>
        /// Finance manager approves/rejects application
        /// </summary>
        public async Task<(bool Success, string Message)> FinanceManagerApproveAsync(
            int applicationId, bool approved, string remarks, string approvedBy)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var application = await context.AssetApplications
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                return (false, "Application not found");

            var previousStatus = application.Status;
            application.FinanceManagerApprovedBy = approvedBy;
            application.FinanceManagerApprovedAt = DateTime.Now;
            application.FinanceManagerRemarks = remarks;

            if (approved)
            {
                application.Status = "APPROVED";
                // Log approval - ready for release
                await LogHistoryAsync(context, application, "APPROVED", approvedBy, "FinanceManager", 
                    remarks, previousStatus, "APPROVED");
            }
            else
            {
                application.Status = "REJECTED_FINANCEMANAGER";
                // Log rejection
                await LogHistoryAsync(context, application, "REJECTED", approvedBy, "FinanceManager", 
                    remarks, previousStatus, "REJECTED_FINANCEMANAGER");
                if (application.Asset != null)
                    application.Asset.Status = "Available";
            }

            await context.SaveChangesAsync();
            return (true, approved ? "Application approved! Ready for release by teller" : "Application rejected");
        }

        /// <summary>
        /// Release asset and generate contract (by Teller)
        /// </summary>
        public async Task<(bool Success, string Message, int? LoanId)> ReleaseAssetAsync(
            int applicationId, string releaseRemarks, string releasedBy,
            string? deedOfSaleNumber = null, string? orNumber = null, string? crNumber = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var application = await context.AssetApplications
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                return (false, "Application not found", null);

            if (application.Status != "APPROVED" && application.Status != "APPROVED_READY_FOR_RELEASE")
                return (false, "Application is not approved for release", null);

            // Generate contract
            application.ContractNumber = AssetApplication.GenerateContractNumber();
            application.ContractDate = DateTime.Now;
            application.DeedOfSaleNumber = deedOfSaleNumber;
            application.DeedOfSaleDate = DateTime.Now;
            application.ORNumber = orNumber;
            application.CRNumber = crNumber;
            application.ReleasedBy = releasedBy;
            application.ReleasedAt = DateTime.Now;
            application.ReleaseRemarks = releaseRemarks;
            application.Status = "RELEASED";

            // Mark asset as sold
            if (application.Asset != null)
                application.Asset.Status = "Sold";

            int? loanId = null;

            // If loan purchase, create loan record
            if (application.PurchaseType == "Loan" && application.LoanAmount > 0)
            {
                var loan = new Loan
                {
                    LoanNumber = $"AST-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                    AccountId = application.CustomerAccountId ?? 0,
                    LoanType = $"Asset - {application.Asset?.AssetType ?? "Purchase"}",
                    LoanAmount = application.LoanAmount,
                    InterestRate = application.InterestRate,
                    TermMonths = application.TermMonths,
                    MonthlyPayment = application.MonthlyPayment,
                    OutstandingBalance = application.LoanAmount,
                    Status = "ACTIVE",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(application.TermMonths),
                    NextDueDate = DateTime.Now.AddMonths(1),
                    Purpose = $"Asset Purchase: {application.Asset?.DisplayTitle}",
                    CreatedAt = DateTime.Now
                };

                context.Loans.Add(loan);
                await context.SaveChangesAsync();

                application.LinkedLoanId = loan.LoanId;
                loanId = loan.LoanId;

                // Generate payment schedule
                await GeneratePaymentScheduleAsync(context, loan);
            }

            // Log release to history with contract info
            await LogHistoryAsync(context, application, "RELEASED", releasedBy, "Teller", 
                releaseRemarks, "APPROVED", "RELEASED");

            await context.SaveChangesAsync();
            return (true, $"Asset released successfully. Contract #: {application.ContractNumber}", loanId);
        }

        /// <summary>
        /// Release asset, generate contract, and post GL entries (by Teller)
        /// </summary>
        public async Task<(bool Success, string Message, int? LoanId)> ReleaseAssetWithGLPostingAsync(
            int applicationId, string releaseRemarks, string releasedBy,
            string? deedOfSaleNumber = null, string? orNumber = null, string? crNumber = null)
        {
            // First release the asset using existing method (but we'll duplicate the logic with GL posting)
            using var context = await _contextFactory.CreateDbContextAsync();
            var application = await context.AssetApplications
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
                return (false, "Application not found", null);

            if (application.Status != "APPROVED" && application.Status != "APPROVED_READY_FOR_RELEASE")
                return (false, "Application is not approved for release", null);

            // Generate contract
            application.ContractNumber = AssetApplication.GenerateContractNumber();
            application.ContractDate = DateTime.Now;
            application.DeedOfSaleNumber = deedOfSaleNumber;
            application.DeedOfSaleDate = DateTime.Now;
            application.ORNumber = orNumber;
            application.CRNumber = crNumber;
            application.ReleasedBy = releasedBy;
            application.ReleasedAt = DateTime.Now;
            application.ReleaseRemarks = releaseRemarks;
            application.Status = "RELEASED";

            var assetDescription = application.Asset?.DisplayTitle ?? "Asset Purchase";

            // Mark asset as sold
            if (application.Asset != null)
                application.Asset.Status = "Sold";

            int? loanId = null;

            // Post GL entries based on purchase type
            if (application.PurchaseType == "Cash" || application.LoanAmount <= 0)
            {
                // Cash sale - post GL entries
                await _glPostingService.PostAssetCashSaleAsync(
                    application.ApplicationNumber,
                    application.AssetPrice,
                    application.CustomerName,
                    assetDescription,
                    DateTime.Now);
            }
            else if (application.PurchaseType == "Loan" && application.LoanAmount > 0)
            {
                // Loan sale - post GL entries for down payment + loan receivable
                await _glPostingService.PostAssetLoanSaleAsync(
                    application.ApplicationNumber,
                    application.DownPaymentAmount,
                    application.LoanAmount,
                    application.CustomerName,
                    assetDescription,
                    DateTime.Now);

                // Create loan record
                var loan = new Loan
                {
                    LoanNumber = $"AST-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                    AccountId = application.CustomerAccountId ?? 0,
                    LoanType = $"Asset - {application.Asset?.AssetType ?? "Purchase"}",
                    LoanAmount = application.LoanAmount,
                    InterestRate = application.InterestRate,
                    TermMonths = application.TermMonths,
                    MonthlyPayment = application.MonthlyPayment,
                    OutstandingBalance = application.LoanAmount,
                    Status = "ACTIVE",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(application.TermMonths),
                    NextDueDate = DateTime.Now.AddMonths(1),
                    Purpose = $"Asset Purchase: {assetDescription}",
                    CreatedAt = DateTime.Now
                };

                context.Loans.Add(loan);
                await context.SaveChangesAsync();

                application.LinkedLoanId = loan.LoanId;
                loanId = loan.LoanId;

                // Generate payment schedule
                await GeneratePaymentScheduleAsync(context, loan);
            }

            // Log release to history with contract info
            await LogHistoryAsync(context, application, "RELEASED", releasedBy, "Teller", 
                releaseRemarks, "APPROVED", "RELEASED");

            await context.SaveChangesAsync();
            return (true, $"Asset released successfully. Contract #: {application.ContractNumber}. GL entries posted.", loanId);
        }

        private async Task GeneratePaymentScheduleAsync(BFASDbContext context, Loan loan)
        {
            var schedules = new List<LoanPaymentSchedule>();
            var balance = loan.LoanAmount;
            var monthlyRate = (loan.InterestRate / 100) / 12;

            for (int month = 1; month <= loan.TermMonths; month++)
            {
                var interestPayment = balance * monthlyRate;
                var principalPayment = loan.MonthlyPayment - interestPayment;
                
                if (month == loan.TermMonths)
                {
                    principalPayment = balance;
                }

                balance -= principalPayment;

                var schedule = new LoanPaymentSchedule
                {
                    LoanId = loan.LoanId,
                    PaymentNumber = month,
                    DueDate = loan.StartDate.AddMonths(month),
                    PrincipalAmount = Math.Round(principalPayment, 2),
                    InterestAmount = Math.Round(interestPayment, 2),
                    MinimumPayment = loan.MonthlyPayment,
                    PaymentStatus = "PENDING"
                };

                schedules.Add(schedule);
            }

            context.LoanPaymentSchedules.AddRange(schedules);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Cancel application (by customer)
        /// </summary>
        public async Task<(bool Success, string Message)> CancelApplicationAsync(int applicationId, int customerId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var application = await context.AssetApplications
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.CustomerId == customerId);

            if (application == null)
                return (false, "Application not found");

            if (application.Status == "RELEASED" || application.Status == "CANCELLED")
                return (false, "Cannot cancel this application");

            application.Status = "CANCELLED";
            
            // Make asset available again
            if (application.Asset != null)
                application.Asset.Status = "Available";

            await context.SaveChangesAsync();
            return (true, "Application cancelled successfully");
        }

        /// <summary>
        /// Get application by ID
        /// </summary>
        public async Task<AssetApplication?> GetApplicationByIdAsync(int applicationId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetApplications
                .Include(a => a.Asset)
                .ThenInclude(a => a.Images)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get marketplace statistics for admin dashboard
        /// </summary>
        public async Task<MarketplaceStats> GetMarketplaceStatsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var stats = new MarketplaceStats
            {
                TotalAssets = await context.Assets.CountAsync(),
                AvailableAssets = await context.Assets.CountAsync(a => a.Status == "Available"),
                SoldAssets = await context.Assets.CountAsync(a => a.Status == "Sold"),
                ReservedAssets = await context.Assets.CountAsync(a => a.Status == "Reserved"),
                PendingApplications = await context.AssetApplications.CountAsync(a => 
                    a.Status != "RELEASED" && a.Status != "CANCELLED" && !a.Status.StartsWith("REJECTED")),
                TotalSalesValue = await context.AssetApplications
                    .Where(a => a.Status == "RELEASED")
                    .SumAsync(a => a.AssetPrice),
                PropertyCount = await context.Assets.CountAsync(a => a.AssetType == "Property" && a.Status == "Available"),
                VehicleCount = await context.Assets.CountAsync(a => a.AssetType == "Vehicle" && a.Status == "Available"),
                OtherCount = await context.Assets.CountAsync(a => a.AssetType == "Other" && a.Status == "Available")
            };

            return stats;
        }

        #endregion

        #region Asset History Management

        /// <summary>
        /// Log an action to asset history
        /// </summary>
        public async Task LogHistoryAsync(
            BFASDbContext context,
            AssetApplication application,
            string actionType,
            string actionBy,
            string actionByRole,
            string? remarks = null,
            string? previousStatus = null,
            string? newStatus = null)
        {
            var history = new AssetHistory
            {
                ApplicationId = application.ApplicationId,
                ActionType = actionType,
                ActionBy = actionBy,
                ActionByRole = actionByRole,
                ActionDate = DateTime.Now,
                Remarks = remarks,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                AssetPrice = application.AssetPrice,
                DownPaymentAmount = application.DownPaymentAmount,
                LoanAmount = application.LoanAmount,
                TermMonths = application.TermMonths,
                InterestRate = application.InterestRate,
                MonthlyPayment = application.MonthlyPayment,
                DebtToIncomeRatio = application.DebtToIncomeRatio,
                CreditRiskLevel = application.CreditRiskLevel,
                Recommendation = application.AccountantRecommendation,
                ContractNumber = application.ContractNumber,
                ContractDate = application.ContractDate,
                DeedOfSaleNumber = application.DeedOfSaleNumber,
                DeedOfSaleDate = application.DeedOfSaleDate,
                ORNumber = application.ORNumber,
                CRNumber = application.CRNumber,
                LinkedLoanId = application.LinkedLoanId
            };

            context.AssetHistories.Add(history);
        }

        /// <summary>
        /// Get history for an application
        /// </summary>
        public async Task<List<AssetHistory>> GetApplicationHistoryAsync(int applicationId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetHistories
                .Where(h => h.ApplicationId == applicationId)
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get all released/forwarded asset histories with contracts
        /// </summary>
        public async Task<List<AssetHistory>> GetReleasedHistoryAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetHistories
                .Include(h => h.Application)
                    .ThenInclude(a => a.Asset)
                .Where(h => h.ActionType == "RELEASED" || h.ActionType == "FORWARDED_TO_ACCOUNTANT" || 
                            h.ActionType == "FORWARDED_TO_FINANCEMANAGER")
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get history by action type
        /// </summary>
        public async Task<List<AssetHistory>> GetHistoryByActionTypeAsync(string actionType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AssetHistories
                .Include(h => h.Application)
                    .ThenInclude(a => a.Asset)
                .Where(h => h.ActionType == actionType)
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get all history with filtering options
        /// </summary>
        public async Task<List<AssetHistory>> GetAllHistoryAsync(
            DateTime? fromDate = null, 
            DateTime? toDate = null,
            string? actionType = null,
            string? actionBy = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.AssetHistories
                .Include(h => h.Application)
                    .ThenInclude(a => a.Asset)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(h => h.ActionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.ActionDate <= toDate.Value.AddDays(1));

            if (!string.IsNullOrEmpty(actionType))
                query = query.Where(h => h.ActionType == actionType);

            if (!string.IsNullOrEmpty(actionBy))
                query = query.Where(h => h.ActionBy != null && h.ActionBy.Contains(actionBy));

            return await query.OrderByDescending(h => h.ActionDate).ToListAsync();
        }

        #endregion
    }

    /// <summary>
    /// Statistics model for marketplace dashboard
    /// </summary>
    public class MarketplaceStats
    {
        public int TotalAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int SoldAssets { get; set; }
        public int ReservedAssets { get; set; }
        public int PendingApplications { get; set; }
        public decimal TotalSalesValue { get; set; }
        public int PropertyCount { get; set; }
        public int VehicleCount { get; set; }
        public int OtherCount { get; set; }
    }
}
