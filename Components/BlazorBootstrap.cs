using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace EntraSamlLab.Components;

public sealed class BlazorBootstrap : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "script");
        builder.AddAttribute(1, "src", "_framework/blazor.web.js");
        builder.CloseElement();
    }
}