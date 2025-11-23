using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models;

namespace FinanceBank.Services
{
    /// <summary>
    /// Service for Approvals module CRUD operations
    /// </summary>
    public class ApprovalsService
    {
        private readonly BFASDbContext _context;

        public ApprovalsService(BFASDbContext context)
        {
            _context = context;
        }

        // Approval Queue Operations
        public async Task<List<ApprovalQueueEntity>> GetAllApprovalsAsync()
        {
            try
            {
                return await _context.ApprovalQueues.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving approvals: {ex.Message}", ex);
            }
        }

        public async Task<ApprovalQueueEntity?> GetApprovalByIdAsync(int id)
        {
            try
            {
                return await _context.ApprovalQueues.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving approval: {ex.Message}", ex);
            }
        }

        public async Task<List<ApprovalQueueEntity>> GetPendingApprovalsAsync()
        {
            try
            {
                return await _context.ApprovalQueues
                    .Where(a => a.Status == "Pending")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving pending approvals: {ex.Message}", ex);
            }
        }

        public async Task<List<ApprovalQueueEntity>> GetApprovalsByTypeAsync(string type)
        {
            try
            {
                return await _context.ApprovalQueues
                    .Where(a => a.RequestType == type)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving approvals by type: {ex.Message}", ex);
            }
        }

        public async Task<List<ApprovalQueueEntity>> GetApprovalsByStatusAsync(string status)
        {
            try
            {
                return await _context.ApprovalQueues
                    .Where(a => a.Status == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving approvals by status: {ex.Message}", ex);
            }
        }

        public async Task<ApprovalQueueEntity> CreateApprovalAsync(ApprovalQueueEntity approval)
        {
            try
            {
                approval.RequestedAt = DateTime.Now;
                approval.Status = "Pending";
                _context.ApprovalQueues.Add(approval);
                await _context.SaveChangesAsync();
                return approval;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error creating approval: {ex.Message}", ex);
            }
        }

        public async Task<ApprovalQueueEntity> UpdateApprovalAsync(ApprovalQueueEntity approval)
        {
            try
            {
                _context.ApprovalQueues.Update(approval);
                await _context.SaveChangesAsync();
                return approval;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error updating approval: {ex.Message}", ex);
            }
        }

        public async Task<ApprovalQueueEntity> ApproveAsync(int approvalId, string approverNotes = "")
        {
            try
            {
                var approval = await GetApprovalByIdAsync(approvalId);
                if (approval == null)
                    throw new ServiceException("Approval not found");

                approval.Status = "Approved";
                approval.ProcessedAt = DateTime.Now;
                approval.ApprovalNotes = approverNotes;
                
                _context.ApprovalQueues.Update(approval);
                await _context.SaveChangesAsync();
                return approval;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error approving request: {ex.Message}", ex);
            }
        }

        public async Task<ApprovalQueueEntity> RejectAsync(int approvalId, string rejectionReason = "")
        {
            try
            {
                var approval = await GetApprovalByIdAsync(approvalId);
                if (approval == null)
                    throw new ServiceException("Approval not found");

                approval.Status = "Rejected";
                approval.ProcessedAt = DateTime.Now;
                approval.ApprovalNotes = rejectionReason;
                
                _context.ApprovalQueues.Update(approval);
                await _context.SaveChangesAsync();
                return approval;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error rejecting request: {ex.Message}", ex);
            }
        }

        public async Task DeleteApprovalAsync(int id)
        {
            try
            {
                var approval = await GetApprovalByIdAsync(id);
                if (approval != null)
                {
                    _context.ApprovalQueues.Remove(approval);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error deleting approval: {ex.Message}", ex);
            }
        }

        // Summary statistics
        public async Task<Dictionary<string, int>> GetApprovalSummaryAsync()
        {
            try
            {
                var summary = new Dictionary<string, int>
                {
                    { "TotalPending", await _context.ApprovalQueues.CountAsync(a => a.Status == "Pending") },
                    { "TotalApproved", await _context.ApprovalQueues.CountAsync(a => a.Status == "Approved") },
                    { "TotalRejected", await _context.ApprovalQueues.CountAsync(a => a.Status == "Rejected") },
                    { "Total", await _context.ApprovalQueues.CountAsync() }
                };
                return summary;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving approval summary: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, int>> GetApprovalsByTypeCountAsync()
        {
            try
            {
                var summary = await _context.ApprovalQueues
                    .GroupBy(a => a.RequestType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Type ?? "Unknown", x => x.Count);
                return summary;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error retrieving approvals by type count: {ex.Message}", ex);
            }
        }
    }
}
