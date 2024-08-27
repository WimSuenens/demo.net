using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using MyDemoAPI.Utils;

namespace MyDemoAPI.Components;

public sealed class InitializeTimeZone : ComponentBase
{
    [Inject] public TimeProvider TimeProvider { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && TimeProvider is BrowserTimeProvider browserTimeProvider && !browserTimeProvider.IsLocalTimeZoneSet)
        {
            try
            {
                await using var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./timezone.js");
                var timeZone = await module.InvokeAsync<string>("getBrowserTimeZone");
                browserTimeProvider.SetBrowserTimeZone(timeZone);
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}

public static class TimeProviderExtensions
{
    public static DateTime ToLocalDateTime(this TimeProvider timeProvider, DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Unspecified => throw new InvalidOperationException("Unable to convert unspecified DateTime to local time"),
            DateTimeKind.Local => dateTime,
            _ => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeProvider.LocalTimeZone), DateTimeKind.Local),
        };
    }

    public static DateTime ToLocalDateTime(this TimeProvider timeProvider, DateTimeOffset dateTime)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(dateTime.UtcDateTime, timeProvider.LocalTimeZone);
        local = DateTime.SpecifyKind(local, DateTimeKind.Local);
        return local;
    }
}

public sealed class LocalTime : ComponentBase, IDisposable
{
    [Inject]
    public TimeProvider TimeProvider { get; set; } = default!;

    [Parameter]
    public DateTime? DateTime { get; set; }

    [Parameter]
    public string? Format { get; set; }

    protected override void OnInitialized()
    {
        if (TimeProvider is BrowserTimeProvider browserTimeProvider)
        {
            browserTimeProvider.LocalTimeZoneChanged += LocalTimeZoneChanged;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (DateTime != null)
        {
            builder.AddContent(0, Format is not null ? TimeProvider.ToLocalDateTime(DateTime.Value).ToString(Format) : TimeProvider.ToLocalDateTime(DateTime.Value));
        }
    }

    public void Dispose()
    {
        if (TimeProvider is BrowserTimeProvider browserTimeProvider)
        {
            browserTimeProvider.LocalTimeZoneChanged -= LocalTimeZoneChanged;
        }
    }

    private void LocalTimeZoneChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }
}