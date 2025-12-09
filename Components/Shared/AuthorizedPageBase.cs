using Microsoft.AspNetCore.Components;
using FinanceBank.Services;
using FinanceBank.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace FinanceBank.Components.Shared
{
    public partial class AuthorizedPageBase : ComponentBase
    {
        [Inject]
        protected AuthService AuthService { get; set; } = default!;

        [Inject]
        protected NavigationManager Navigation { get; set; } = default!;

        [Inject]
        protected IDbContextFactory<BFASDbContext> DbContextFactory { get; set; } = default!;

        [Parameter]
        public string[]? RequiredRoles { get; set; }

        [Parameter]
        public string[]? RequiredPermissions { get; set; }

        [Parameter]
        public string PageName { get; set; } = "";

        [Parameter]
        public string Module { get; set; } = "General";

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();

            // Check if user is authenticated
            if (!AuthService.IsAuthenticated)
            {
                Navigation.NavigateTo("/login", true);
                return;
            }

            // Check if user has required roles
            if (RequiredRoles != null && RequiredRoles.Length > 0)
            {
                if (!AuthService.HasAnyRole(RequiredRoles))
                {
                    Navigation.NavigateTo("/unauthorized", true);
                    return;
                }
            }

            // Check if user has required permissions
            if (RequiredPermissions != null && RequiredPermissions.Length > 0)
            {
                var hasPermission = RequiredPermissions.Any(p => AuthService.HasPermission(p));
                if (!hasPermission)
                {
                    Navigation.NavigateTo("/unauthorized", true);
                    return;
                }
            }

            // Check route-based access
            var currentRoute = Navigation.Uri;
            if (!AuthService.CanAccessRoute(currentRoute))
            {
                Navigation.NavigateTo("/unauthorized", true);
                return;
            }

            // Log page access for authenticated users
            if (AuthService.IsAuthenticated && AuthService.CurrentUserId.HasValue)
            {
                await LogPageAccessAsync();
            }
        }

        /// <summary>
        /// Log page access to audit logs
        /// </summary>
        private async Task LogPageAccessAsync()
        {
            try
            {
                using var context = await DbContextFactory.CreateDbContextAsync();
                var auditService = new AuditLogService(context);

                var userId = AuthService.CurrentUserId?.ToString() ?? "Unknown";
                var userName = AuthService.FullName ?? AuthService.CurrentUser ?? "Unknown";
                var pageName = string.IsNullOrEmpty(PageName) ? Navigation.Uri : PageName;

                await auditService.LogPageAccessAsync(userId, userName, pageName, Module);
            }
            catch
            {
                // Ignore audit logging errors to prevent page load failures
            }
        }

        /// <summary>
        /// Helper method for pages to log custom audit events
        /// </summary>
        protected async Task LogAuditEventAsync(string action, string description, decimal? amount = null, decimal? balanceBefore = null, decimal? balanceAfter = null)
        {
            if (!AuthService.IsAuthenticated || !AuthService.CurrentUserId.HasValue)
                return;

            try
            {
                using var context = await DbContextFactory.CreateDbContextAsync();
                var auditService = new AuditLogService(context);

                await auditService.LogTransactionAsync(
                    AuthService.CurrentUserId.Value.ToString(),
                    AuthService.FullName ?? AuthService.CurrentUser ?? "Unknown",
                    action,
                    Module,
                    description,
                    amount,
                    balanceBefore,
                    balanceAfter
                );
            }
            catch
            {
                // Ignore audit logging errors
            }
        }
    }
}

