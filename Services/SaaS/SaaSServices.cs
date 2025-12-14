using Microsoft.EntityFrameworkCore;
using FinanceBank.Data;
using FinanceBank.Models.SaaS;
using System.Text;

namespace FinanceBank.Services.SaaS
{
    /// <summary>
    /// Service for managing SaaS clients
    /// </summary>
    public class SaaSClientService
    {
        private readonly SaaSDbContext _context;

        public SaaSClientService(SaaSDbContext context)
        {
            _context = context;
        }

        #region Client CRUD

        public async Task<List<SaaSClient>> GetAllClientsAsync()
        {
            return await _context.Clients
                .Include(c => c.Subscriptions.Where(s => s.Status == "Active"))
                .ThenInclude(s => s.Plan)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<SaaSClient?> GetClientByIdAsync(int clientId)
        {
            return await _context.Clients
                .Include(c => c.Users)
                .Include(c => c.Subscriptions)
                .ThenInclude(s => s.Plan)
                .Include(c => c.Modules)
                .ThenInclude(m => m.Module)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        public async Task<SaaSClient?> GetClientByCodeAsync(string clientCode)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientCode == clientCode);
        }

        public async Task<SaaSClient> CreateClientAsync(SaaSClient client)
        {
            // Generate client code
            var lastClient = await _context.Clients
                .OrderByDescending(c => c.ClientId)
                .FirstOrDefaultAsync();

            int nextNumber = (lastClient?.ClientId ?? 0) + 1;
            client.ClientCode = $"CLT-{nextNumber:D4}";
            client.CreatedAt = DateTime.Now;

            // Generate license key
            client.LicenseKey = GenerateLicenseKey(client.ClientCode);

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Create license key record
            var license = new LicenseKey
            {
                ClientId = client.ClientId,
                Key = client.LicenseKey,
                ExpiresAt = client.SubscriptionEndDate ?? DateTime.Now.AddYears(1)
            };
            _context.LicenseKeys.Add(license);
            await _context.SaveChangesAsync();

            return client;
        }

        public async Task<SaaSClient> UpdateClientAsync(SaaSClient client)
        {
            client.UpdatedAt = DateTime.Now;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<bool> DeleteClientAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return false;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SuspendClientAsync(int clientId, string reason)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return false;

            client.Status = "Suspended";
            client.Notes = $"Suspended: {reason}\n{client.Notes}";
            client.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateClientAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return false;

            client.Status = "Active";
            client.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Client Users

        public async Task<ClientUser?> AuthenticateClientUserAsync(string email, string password)
        {
            return await _context.ClientUsers
                .Include(u => u.Client)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password && u.IsActive);
        }

        public async Task<ClientUser> CreateClientUserAsync(ClientUser user)
        {
            user.CreatedAt = DateTime.Now;
            _context.ClientUsers.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<ClientUser>> GetClientUsersAsync(int clientId)
        {
            return await _context.ClientUsers
                .Where(u => u.ClientId == clientId)
                .ToListAsync();
        }

        #endregion

        #region Statistics

        public async Task<ClientStatistics> GetClientStatisticsAsync()
        {
            var clients = await _context.Clients.ToListAsync();
            var activeSubscriptions = await _context.ClientSubscriptions
                .Where(s => s.Status == "Active")
                .ToListAsync();

            return new ClientStatistics
            {
                TotalClients = clients.Count,
                ActiveClients = clients.Count(c => c.Status == "Active"),
                SuspendedClients = clients.Count(c => c.Status == "Suspended"),
                TrialClients = clients.Count(c => c.Status == "Trial"),
                MonthlyRecurringRevenue = activeSubscriptions.Sum(s => s.TotalPrice),
                TotalOutstandingBalance = clients.Sum(c => c.OutstandingBalance)
            };
        }

        #endregion

        #region Helpers

        private string GenerateLicenseKey(string clientCode)
        {
            var guid = Guid.NewGuid().ToString("N").ToUpper();
            return $"{clientCode}-{guid.Substring(0, 8)}-{guid.Substring(8, 4)}-{DateTime.Now.Year}";
        }

        #endregion

        #region Subscription and Module Management

        public async Task<ClientSubscription> CreateSubscriptionAsync(int clientId, ClientSubscription subscription)
        {
            subscription.ClientId = clientId;
            subscription.CreatedAt = DateTime.Now;
            _context.ClientSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<bool> AddModuleToClientAsync(int clientId, int moduleId, decimal customPrice)
        {
            try
            {
                var existingModule = await _context.ClientModules
                    .FirstOrDefaultAsync(cm => cm.ClientId == clientId && cm.ModuleId == moduleId);

                if (existingModule != null)
                {
                    existingModule.IsEnabled = true;
                    existingModule.CustomPrice = customPrice;
                }
                else
                {
                    var clientModule = new ClientModule
                    {
                        ClientId = clientId,
                        ModuleId = moduleId,
                        Source = "AddOn",
                        CustomPrice = customPrice,
                        IsEnabled = true,
                        EnabledAt = DateTime.Now
                    };
                    _context.ClientModules.Add(clientModule);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    public class ClientStatistics
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int SuspendedClients { get; set; }
        public int TrialClients { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
    }

    /// <summary>
    /// Service for managing system modules
    /// </summary>
    public class SaaSModuleService
    {
        private readonly SaaSDbContext _context;

        public SaaSModuleService(SaaSDbContext context)
        {
            _context = context;
        }

        public async Task<List<SystemModule>> GetAllModulesAsync()
        {
            return await _context.SystemModules
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<List<SystemModule>> GetModulesByCategoryAsync(string category)
        {
            return await _context.SystemModules
                .Where(m => m.Category == category && m.IsActive)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<SystemModule?> GetModuleByIdAsync(int moduleId)
        {
            return await _context.SystemModules.FindAsync(moduleId);
        }

        public async Task<SystemModule> CreateModuleAsync(SystemModule module)
        {
            module.CreatedAt = DateTime.Now;
            _context.SystemModules.Add(module);
            await _context.SaveChangesAsync();
            return module;
        }

        public async Task<SystemModule> UpdateModuleAsync(SystemModule module)
        {
            module.UpdatedAt = DateTime.Now;
            _context.SystemModules.Update(module);
            await _context.SaveChangesAsync();
            return module;
        }

        public async Task<List<string>> GetModuleCategoriesAsync()
        {
            return await _context.SystemModules
                .Where(m => m.Category != null)
                .Select(m => m.Category!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<ClientModule>> GetClientModulesAsync(int clientId)
        {
            return await _context.ClientModules
                .Include(cm => cm.Module)
                .Where(cm => cm.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<bool> EnableModuleForClientAsync(int clientId, int moduleId, string source = "AddOn", decimal? customPrice = null)
        {
            var existing = await _context.ClientModules
                .FirstOrDefaultAsync(cm => cm.ClientId == clientId && cm.ModuleId == moduleId);

            if (existing != null)
            {
                existing.IsEnabled = true;
                existing.DisabledAt = null;
                existing.EnabledAt = DateTime.Now;
            }
            else
            {
                var clientModule = new ClientModule
                {
                    ClientId = clientId,
                    ModuleId = moduleId,
                    Source = source,
                    CustomPrice = customPrice,
                    IsCustomPrice = customPrice.HasValue,
                    IsEnabled = true,
                    EnabledAt = DateTime.Now
                };
                _context.ClientModules.Add(clientModule);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DisableModuleForClientAsync(int clientId, int moduleId)
        {
            var clientModule = await _context.ClientModules
                .FirstOrDefaultAsync(cm => cm.ClientId == clientId && cm.ModuleId == moduleId);

            if (clientModule == null) return false;

            clientModule.IsEnabled = false;
            clientModule.DisabledAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }

    /// <summary>
    /// Service for managing subscription plans
    /// </summary>
    public class SaaSPlanService
    {
        private readonly SaaSDbContext _context;

        public SaaSPlanService(SaaSDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetAllPlansAsync()
        {
            return await _context.SubscriptionPlans
                .Include(p => p.PlanModules)
                .ThenInclude(pm => pm.Module)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetPlanByIdAsync(int planId)
        {
            return await _context.SubscriptionPlans
                .Include(p => p.PlanModules)
                .ThenInclude(pm => pm.Module)
                .FirstOrDefaultAsync(p => p.PlanId == planId);
        }

        public async Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan)
        {
            plan.CreatedAt = DateTime.Now;
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan plan)
        {
            plan.UpdatedAt = DateTime.Now;
            _context.SubscriptionPlans.Update(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> AddModuleToPlanAsync(int planId, int moduleId)
        {
            var existing = await _context.PlanModules
                .FirstOrDefaultAsync(pm => pm.PlanId == planId && pm.ModuleId == moduleId);

            if (existing != null) return true;

            var planModule = new PlanModule
            {
                PlanId = planId,
                ModuleId = moduleId,
                IsIncluded = true
            };

            _context.PlanModules.Add(planModule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveModuleFromPlanAsync(int planId, int moduleId)
        {
            var planModule = await _context.PlanModules
                .FirstOrDefaultAsync(pm => pm.PlanId == planId && pm.ModuleId == moduleId);

            if (planModule == null) return false;

            _context.PlanModules.Remove(planModule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SetPlanModulesAsync(int planId, List<int> moduleIds)
        {
            // Remove existing modules
            var existingModules = await _context.PlanModules
                .Where(pm => pm.PlanId == planId)
                .ToListAsync();

            _context.PlanModules.RemoveRange(existingModules);

            // Add new modules
            foreach (var moduleId in moduleIds)
            {
                var planModule = new PlanModule
                {
                    PlanId = planId,
                    ModuleId = moduleId,
                    IsIncluded = true
                };
                _context.PlanModules.Add(planModule);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ClientSubscription> SubscribeClientToPlanAsync(int clientId, int planId, string billingCycle = "Monthly")
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.PlanModules)
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            if (plan == null)
                throw new Exception("Plan not found");

            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                throw new Exception("Client not found");

            // Calculate dates and pricing
            var startDate = DateTime.Now;
            var endDate = billingCycle == "Yearly" ? startDate.AddYears(1) : startDate.AddMonths(1);
            var price = billingCycle == "Yearly" ? plan.YearlyPrice : plan.MonthlyPrice;

            // Create subscription
            var subscription = new ClientSubscription
            {
                ClientId = clientId,
                PlanId = planId,
                StartDate = startDate,
                EndDate = endDate,
                BillingCycle = billingCycle,
                BasePrice = price,
                TotalPrice = price,
                MaxUsers = plan.MaxUsers,
                Status = "Active"
            };

            _context.ClientSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Add plan modules to client
            foreach (var pm in plan.PlanModules)
            {
                var existingModule = await _context.ClientModules
                    .FirstOrDefaultAsync(cm => cm.ClientId == clientId && cm.ModuleId == pm.ModuleId);

                if (existingModule == null)
                {
                    _context.ClientModules.Add(new ClientModule
                    {
                        ClientId = clientId,
                        ModuleId = pm.ModuleId,
                        SubscriptionId = subscription.SubscriptionId,
                        Source = "Plan",
                        IsEnabled = true,
                        EnabledAt = DateTime.Now
                    });
                }
                else
                {
                    existingModule.IsEnabled = true;
                    existingModule.SubscriptionId = subscription.SubscriptionId;
                }
            }

            // Update client
            client.SubscriptionStartDate = startDate;
            client.SubscriptionEndDate = endDate;
            client.Status = "Active";
            client.BillingCycle = billingCycle;

            await _context.SaveChangesAsync();
            return subscription;
        }
    }

    /// <summary>
    /// Service for managing invoices and billing
    /// </summary>
    public class SaaSInvoiceService
    {
        private readonly SaaSDbContext _context;

        public SaaSInvoiceService(SaaSDbContext context)
        {
            _context = context;
        }

        public async Task<List<SaaSInvoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Items)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<List<SaaSInvoice>> GetClientInvoicesAsync(int clientId)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<SaaSInvoice?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Items)
                .ThenInclude(item => item.Module)
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }

        public async Task<SaaSInvoice> CreateInvoiceAsync(int clientId, int? subscriptionId = null)
        {
            var client = await _context.Clients
                .Include(c => c.Subscriptions.Where(s => s.Status == "Active"))
                .ThenInclude(s => s.Plan)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
                throw new Exception("Client not found");

            var subscription = client.Subscriptions.FirstOrDefault();

            // Generate invoice number
            var year = DateTime.Now.Year;
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}"))
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int num))
                {
                    nextNumber = num + 1;
                }
            }

            var invoice = new SaaSInvoice
            {
                InvoiceNumber = $"INV-{year}-{nextNumber:D4}",
                ClientId = clientId,
                SubscriptionId = subscription?.SubscriptionId,
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(15),
                PeriodStart = DateTime.Now,
                PeriodEnd = DateTime.Now.AddMonths(1),
                Status = "Pending"
            };

            // Add subscription as line item
            if (subscription != null)
            {
                var item = new SaaSInvoiceItem
                {
                    Description = $"{subscription.Plan?.PlanName ?? "Subscription"} - {subscription.BillingCycle}",
                    Quantity = 1,
                    UnitPrice = subscription.TotalPrice,
                    Amount = subscription.TotalPrice,
                    ItemType = "Subscription"
                };
                invoice.Items.Add(item);
            }

            // Calculate totals
            invoice.Subtotal = invoice.Items.Sum(i => i.Amount);
            invoice.Tax = invoice.Subtotal * (invoice.TaxRate / 100);
            invoice.TotalAmount = invoice.Subtotal + invoice.Tax;
            invoice.BalanceDue = invoice.TotalAmount;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Update client outstanding balance
            client.OutstandingBalance += invoice.TotalAmount;
            await _context.SaveChangesAsync();

            return invoice;
        }

        /// <summary>
        /// Creates an invoice with a simple description and amount
        /// </summary>
        public async Task<SaaSInvoice> CreateInvoiceAsync(int clientId, string description, decimal totalAmount, DateTime dueDate)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                throw new Exception("Client not found");

            // Generate invoice number
            var year = DateTime.Now.Year;
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}"))
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int num))
                {
                    nextNumber = num + 1;
                }
            }

            var invoice = new SaaSInvoice
            {
                InvoiceNumber = $"INV-{year}-{nextNumber:D4}",
                ClientId = clientId,
                InvoiceDate = DateTime.Now,
                DueDate = dueDate,
                PeriodStart = DateTime.Now,
                PeriodEnd = DateTime.Now.AddMonths(1),
                Status = "Pending",
                Subtotal = totalAmount / 1.12m, // Remove VAT to get subtotal
                TaxRate = 12m,
                Tax = totalAmount - (totalAmount / 1.12m),
                TotalAmount = totalAmount,
                BalanceDue = totalAmount,
                CreatedAt = DateTime.Now
            };

            // Add a line item for the subscription
            var item = new SaaSInvoiceItem
            {
                Description = description,
                Quantity = 1,
                UnitPrice = totalAmount / 1.12m,
                Amount = totalAmount / 1.12m,
                ItemType = "Subscription"
            };
            invoice.Items.Add(item);

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<SaaSInvoice> AddInvoiceItemAsync(int invoiceId, SaaSInvoiceItem item)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
                throw new Exception("Invoice not found");

            item.InvoiceId = invoiceId;
            invoice.Items.Add(item);

            // Recalculate totals
            invoice.Subtotal = invoice.Items.Sum(i => i.Amount);
            invoice.Tax = invoice.Subtotal * (invoice.TaxRate / 100);
            invoice.TotalAmount = invoice.Subtotal + invoice.Tax;
            invoice.BalanceDue = invoice.TotalAmount - invoice.AmountPaid;

            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<SaaSInvoice> CreateInvoiceAsync(SaaSInvoice invoiceData, List<(string Description, int Quantity, decimal UnitPrice)> items)
        {
            // Generate invoice number
            var year = DateTime.Now.Year;
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}"))
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int num))
                {
                    nextNumber = num + 1;
                }
            }

            invoiceData.InvoiceNumber = $"INV-{year}-{nextNumber:D4}";
            invoiceData.InvoiceDate = DateTime.Now;
            invoiceData.CreatedAt = DateTime.Now;

            // Add items
            foreach (var item in items)
            {
                var invoiceItem = new SaaSInvoiceItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Amount = item.Quantity * item.UnitPrice,
                    ItemType = "Service"
                };
                invoiceData.Items.Add(invoiceItem);
            }

            // Calculate totals
            invoiceData.Subtotal = invoiceData.Items.Sum(i => i.Amount);
            invoiceData.Tax = invoiceData.Subtotal * (invoiceData.TaxRate / 100);
            invoiceData.TotalAmount = invoiceData.Subtotal + invoiceData.Tax;
            invoiceData.BalanceDue = invoiceData.TotalAmount;

            _context.Invoices.Add(invoiceData);
            await _context.SaveChangesAsync();

            return invoiceData;
        }

        public async Task<List<SaaSInvoice>> GetOverdueInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Client)
                .Where(i => i.Status != "Paid" && i.DueDate < DateTime.Now)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task MarkOverdueAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice != null && invoice.Status != "Paid" && invoice.DueDate < DateTime.Now)
            {
                invoice.Status = "Overdue";
                await _context.SaveChangesAsync();
            }
        }

        public async Task SendInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice != null)
            {
                invoice.Status = "Sent";
                invoice.SentAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RecordPaymentAsync(int invoiceId, decimal amount)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            
            if (invoice != null)
            {
                invoice.AmountPaid += amount;
                invoice.BalanceDue = invoice.TotalAmount - invoice.AmountPaid;
                
                if (invoice.BalanceDue <= 0)
                {
                    invoice.Status = "Paid";
                    invoice.PaidAt = DateTime.Now;
                }
                
                // Update client outstanding balance
                if (invoice.Client != null)
                {
                    invoice.Client.OutstandingBalance -= amount;
                    invoice.Client.TotalPaid += amount;
                }
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task CancelInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice != null)
            {
                invoice.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Invoices.Where(i => i.Status == "Paid");

            if (startDate.HasValue)
                query = query.Where(i => i.PaidAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.PaidAt <= endDate.Value);

            return await query.SumAsync(i => i.AmountPaid);
        }
    }

    /// <summary>
    /// Service for managing payments and transactions
    /// </summary>
    public class SaaSPaymentService
    {
        private readonly SaaSDbContext _context;

        public SaaSPaymentService(SaaSDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
        {
            return await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.SortOrder)
                .ToListAsync();
        }

        public async Task<List<SaaSTransaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Client)
                .Include(t => t.Invoice)
                .Include(t => t.PaymentMethod)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<SaaSTransaction>> GetClientTransactionsAsync(int clientId)
        {
            return await _context.Transactions
                .Include(t => t.Invoice)
                .Include(t => t.PaymentMethod)
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<SaaSTransaction> RecordPaymentAsync(int clientId, int? invoiceId, decimal amount, int paymentMethodId, string? reference = null, string? proofImageBase64 = null, string? description = null, string? notes = null)
        {
            // Generate transaction number
            var year = DateTime.Now.Year;
            var lastTransaction = await _context.Transactions
                .Where(t => t.TransactionNumber.StartsWith($"TXN-{year}"))
                .OrderByDescending(t => t.TransactionId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastTransaction != null)
            {
                var parts = lastTransaction.TransactionNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int num))
                {
                    nextNumber = num + 1;
                }
            }

            var transaction = new SaaSTransaction
            {
                TransactionNumber = $"TXN-{year}-{nextNumber:D5}",
                TransactionRef = $"TXN-{year}-{nextNumber:D5}",
                ClientId = clientId,
                InvoiceId = invoiceId,
                TransactionType = "Payment",
                PaymentMethodId = paymentMethodId,
                ReferenceNumber = reference,
                ProofImageBase64 = proofImageBase64,
                Amount = amount,
                Status = "Pending",
                Description = description ?? (invoiceId.HasValue ? $"Payment for Invoice" : "Account Credit"),
                Notes = notes,
                TransactionDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<SaaSTransaction> ApprovePaymentAsync(int transactionId, int processedBy)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Invoice)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
                throw new Exception("Transaction not found");

            transaction.Status = "Completed";
            transaction.ProcessedAt = DateTime.Now;
            transaction.ProcessedBy = processedBy;

            // Update invoice if exists
            if (transaction.Invoice != null)
            {
                transaction.Invoice.AmountPaid += transaction.Amount;
                transaction.Invoice.BalanceDue = transaction.Invoice.TotalAmount - transaction.Invoice.AmountPaid;

                if (transaction.Invoice.BalanceDue <= 0)
                {
                    transaction.Invoice.Status = "Paid";
                    transaction.Invoice.PaidAt = DateTime.Now;
                }
                else
                {
                    transaction.Invoice.Status = "Partial";
                }
            }

            // Update client balance
            var client = await _context.Clients.FindAsync(transaction.ClientId);
            if (client != null)
            {
                client.OutstandingBalance -= transaction.Amount;
                if (client.OutstandingBalance < 0)
                {
                    client.CreditBalance = Math.Abs(client.OutstandingBalance);
                    client.OutstandingBalance = 0;
                }
            }

            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<SaaSTransaction> RejectPaymentAsync(int transactionId, int processedBy, string reason)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);

            if (transaction == null)
                throw new Exception("Transaction not found");

            transaction.Status = "Failed";
            transaction.ProcessedAt = DateTime.Now;
            transaction.ProcessedBy = processedBy;
            transaction.Notes = reason;

            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<SaaSTransaction> RecordRefundAsync(int clientId, int? invoiceId, decimal amount, string reason)
        {
            var year = DateTime.Now.Year;
            var lastTransaction = await _context.Transactions
                .OrderByDescending(t => t.TransactionId)
                .FirstOrDefaultAsync();

            int nextNumber = (lastTransaction?.TransactionId ?? 0) + 1;

            var transaction = new SaaSTransaction
            {
                TransactionNumber = $"REF-{year}-{nextNumber:D5}",
                ClientId = clientId,
                InvoiceId = invoiceId,
                TransactionType = "Refund",
                Amount = amount,
                Status = "Completed",
                Description = reason,
                TransactionDate = DateTime.Now,
                ProcessedAt = DateTime.Now
            };

            _context.Transactions.Add(transaction);

            // Update client balance
            var client = await _context.Clients.FindAsync(clientId);
            if (client != null)
            {
                client.CreditBalance += amount;
            }

            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<SaaSTransaction>> GetPendingApprovalsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Client)
                .Include(t => t.Invoice)
                .Include(t => t.PaymentMethod)
                .Where(t => t.Status == "Pending")
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<SaaSTransaction?> GetTransactionByIdAsync(int transactionId)
        {
            return await _context.Transactions
                .Include(t => t.Client)
                .Include(t => t.Invoice)
                .Include(t => t.PaymentMethod)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<RevenueStats> GetRevenueStatsAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);

            var thisMonthRevenue = await _context.Transactions
                .Where(t => t.Status == "Completed" && t.TransactionType == "Payment" && t.TransactionDate >= startOfMonth)
                .SumAsync(t => t.Amount);

            var lastMonthRevenue = await _context.Transactions
                .Where(t => t.Status == "Completed" && t.TransactionType == "Payment" && t.TransactionDate >= startOfLastMonth && t.TransactionDate < startOfMonth)
                .SumAsync(t => t.Amount);

            var totalRevenue = await _context.Transactions
                .Where(t => t.Status == "Completed" && t.TransactionType == "Payment")
                .SumAsync(t => t.Amount);

            var pendingPayments = await _context.Transactions
                .Where(t => t.Status == "Pending")
                .SumAsync(t => t.Amount);

            return new RevenueStats
            {
                ThisMonthRevenue = thisMonthRevenue,
                LastMonthRevenue = lastMonthRevenue,
                TotalRevenue = totalRevenue,
                PendingPayments = pendingPayments,
                GrowthPercentage = lastMonthRevenue > 0 ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0
            };
        }
    }

    public class RevenueStats
    {
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public decimal GrowthPercentage { get; set; }
    }

    /// <summary>
    /// Service for managing support tickets
    /// </summary>
    public class SaaSSupportService
    {
        private readonly SaaSDbContext _context;

        public SaaSSupportService(SaaSDbContext context)
        {
            _context = context;
        }

        public async Task<List<SupportTicket>> GetAllTicketsAsync()
        {
            return await _context.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SupportTicket>> GetClientTicketsAsync(int clientId)
        {
            return await _context.SupportTickets
                .Include(t => t.Comments)
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket?> GetTicketByIdAsync(int ticketId)
        {
            return await _context.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.User)
                .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        public async Task<SupportTicket> CreateTicketAsync(SupportTicket ticket)
        {
            // Generate ticket number
            var year = DateTime.Now.Year;
            var lastTicket = await _context.SupportTickets
                .OrderByDescending(t => t.TicketId)
                .FirstOrDefaultAsync();

            int nextNumber = (lastTicket?.TicketId ?? 0) + 1;
            ticket.TicketNumber = $"TKT-{year}-{nextNumber:D4}";
            ticket.CreatedAt = DateTime.Now;

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<TicketComment> AddCommentAsync(int ticketId, string comment, int? clientUserId = null, int? ownerId = null, bool isInternal = false)
        {
            var ticketComment = new TicketComment
            {
                TicketId = ticketId,
                Comment = comment,
                ClientUserId = clientUserId,
                OwnerId = ownerId,
                IsInternal = isInternal,
                CreatedAt = DateTime.Now
            };

            _context.TicketComments.Add(ticketComment);

            // Update ticket
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return ticketComment;
        }

        public async Task<SupportTicket> UpdateTicketStatusAsync(int ticketId, string status, string? resolution = null)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found");

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.Now;

            if (status == "Resolved")
            {
                ticket.Resolution = resolution;
                ticket.ResolvedAt = DateTime.Now;
            }
            else if (status == "Closed")
            {
                ticket.ClosedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<SupportTicket> AssignTicketAsync(int ticketId, int ownerId)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found");

            ticket.AssignedTo = ownerId;
            ticket.Status = "In Progress";
            ticket.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return ticket;
        }
    }

    /// <summary>
    /// Service for license validation and module access control
    /// </summary>
    public class SaaSLicenseService
    {
        private readonly SaaSDbContext _context;

        public SaaSLicenseService(SaaSDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey)
        {
            var license = await _context.LicenseKeys
                .Include(l => l.Client)
                .FirstOrDefaultAsync(l => l.Key == licenseKey && l.IsActive);

            if (license == null)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid license key"
                };
            }

            if (license.ExpiresAt < DateTime.Now)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "License has expired",
                    ExpiresAt = license.ExpiresAt
                };
            }

            if (license.Client?.Status != "Active")
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Client account is {license.Client?.Status ?? "inactive"}"
                };
            }

            // Update validation tracking
            license.LastValidatedAt = DateTime.Now;
            license.ValidationCount++;
            await _context.SaveChangesAsync();

            // Get enabled modules
            var modules = await _context.ClientModules
                .Include(cm => cm.Module)
                .Where(cm => cm.ClientId == license.ClientId && cm.IsEnabled)
                .Select(cm => cm.Module!.ModuleCode)
                .ToListAsync();

            return new LicenseValidationResult
            {
                IsValid = true,
                ClientId = license.ClientId,
                ClientName = license.Client?.CompanyName,
                EnabledModules = modules,
                ExpiresAt = license.ExpiresAt
            };
        }

        public async Task<bool> HasModuleAccessAsync(int clientId, string moduleCode)
        {
            return await _context.ClientModules
                .Include(cm => cm.Module)
                .AnyAsync(cm => cm.ClientId == clientId && cm.Module!.ModuleCode == moduleCode && cm.IsEnabled);
        }

        public async Task<List<string>> GetClientEnabledModulesAsync(int clientId)
        {
            return await _context.ClientModules
                .Include(cm => cm.Module)
                .Where(cm => cm.ClientId == clientId && cm.IsEnabled)
                .Select(cm => cm.Module!.ModuleCode)
                .ToListAsync();
        }

        public async Task<LicenseKey> GenerateNewLicenseAsync(int clientId, DateTime expiresAt)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                throw new Exception("Client not found");

            // Deactivate old licenses
            var oldLicenses = await _context.LicenseKeys
                .Where(l => l.ClientId == clientId && l.IsActive)
                .ToListAsync();

            foreach (var old in oldLicenses)
            {
                old.IsActive = false;
            }

            // Generate new license
            var guid = Guid.NewGuid().ToString("N").ToUpper();
            var newLicense = new LicenseKey
            {
                ClientId = clientId,
                Key = $"{client.ClientCode}-{guid.Substring(0, 8)}-{guid.Substring(8, 4)}-{DateTime.Now.Year}",
                ExpiresAt = expiresAt,
                IsActive = true,
                IssuedAt = DateTime.Now
            };

            _context.LicenseKeys.Add(newLicense);

            // Update client
            client.LicenseKey = newLicense.Key;
            client.SubscriptionEndDate = expiresAt;

            await _context.SaveChangesAsync();
            return newLicense;
        }
    }

    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public List<string> EnabledModules { get; set; } = new();
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Service for owner authentication
    /// </summary>
    public class SaaSOwnerService
    {
        private readonly SaaSDbContext _context;

        public SaaSOwnerService(SaaSDbContext context)
        {
            _context = context;
        }

        public async Task<SystemOwner?> AuthenticateAsync(string username, string password)
        {
            var owner = await _context.SystemOwners
                .FirstOrDefaultAsync(o => o.Username == username && o.PasswordHash == password && o.IsActive);

            if (owner != null)
            {
                owner.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return owner;
        }

        public async Task<SystemOwner?> GetOwnerByIdAsync(int ownerId)
        {
            return await _context.SystemOwners.FindAsync(ownerId);
        }

        public async Task<SystemOwner> UpdateOwnerAsync(SystemOwner owner)
        {
            _context.SystemOwners.Update(owner);
            await _context.SaveChangesAsync();
            return owner;
        }

        public async Task<bool> ChangePasswordAsync(int ownerId, string oldPassword, string newPassword)
        {
            var owner = await _context.SystemOwners.FindAsync(ownerId);
            if (owner == null || owner.PasswordHash != oldPassword)
                return false;

            owner.PasswordHash = newPassword;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
