using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using static Microsoft.AspNetCore.Components.Web.RenderMode;

namespace Notes.Components
{
    public partial class App
    {
        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        private IComponentRenderMode? PageRenderMode =>
            HttpContext.AcceptsInteractiveRouting() ? InteractiveWebAssembly : null;




    }
}