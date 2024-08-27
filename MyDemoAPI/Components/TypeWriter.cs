using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using MyDemoAPI.Utils;

namespace MyDemoAPI.Components;

// public sealed class TypeWriter: ComponentBase
// {
//   [Inject]
//   public IJSRuntime JSRuntime { get; set; } = default!;

//   protected override async Task OnAfterRenderAsync(bool firstRender) {
//     if (firstRender) {
//       try
//       {
//         await using var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./typeWriter.js");
//         // var timeZone = await module.InvokeAsync<string>("writeText");
//         // browserTimeProvider.SetBrowserTimeZone(timeZone);

//       }
//       catch (JSDisconnectedException)
//       {
//       }
//     }
//   }

// }

public sealed class TypeWriter: ComponentBase
{
  [Inject]
  public IJSRuntime JSRuntime { get; set; } = default!;

  private IJSObjectReference? reference { get; set; }

  // [Parameter]
  // public string? Text { get; set; } = string.Empty;

  public async Task WriteAsync(string Text) {
    try
    {
      await using var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./typeWriter.js");
      await module.InvokeAsync<string>("writeText", "type_writer", Text);
    }
    catch (JSDisconnectedException)
    {
    }
  }

  protected override void BuildRenderTree(RenderTreeBuilder builder)
  {
    builder.AddMarkupContent(0, "<div id=\"type_writer\"></div>");
  }


}
