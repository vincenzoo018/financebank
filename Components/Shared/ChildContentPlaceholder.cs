using Microsoft.AspNetCore.Components;

namespace FinanceBank.Components.Shared
{
    public class ChildContentPlaceholder : ComponentBase
    {
        [Parameter]
        public RenderFragment? ChildContent { get; set; } = null;

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            if (ChildContent != null)
            {
                builder.AddContent(0, ChildContent);
            }
        }
    }
}
