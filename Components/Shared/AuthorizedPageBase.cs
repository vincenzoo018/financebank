using Microsoft.AspNetCore.Components;
using FinanceBank.Services;
using System.Threading.Tasks;

namespace FinanceBank.Components.Shared
{
    public partial class AuthorizedPageBase : ComponentBase
    {
        [Inject]
        protected AuthService AuthService { get; set; } = default!;

        [Inject]
        protected NavigationManager Navigation { get; set; } = default!;

        [Parameter]
        public string[]? RequiredRoles { get; set; }

        [Parameter]
        public string[]? RequiredPermissions { get; set; }

        protected override void OnInitialized()
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
        }
    }
}

