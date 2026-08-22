using Biblia.Application.Interfaces;

namespace Biblia.Infrastructure.Platform;

public sealed class MauiApplicationExitService : IApplicationExitService
{
#if ANDROID || WINDOWS || MACCATALYST
    public bool CanExit => true;
#else
    public bool CanExit => false;
#endif

    public Task ExitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
#if ANDROID
        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.FinishAndRemoveTask();
#elif WINDOWS
        Microsoft.UI.Xaml.Application.Current.Exit();
#elif MACCATALYST
        UIKit.UIApplication.SharedApplication.PerformSelector(new ObjCRuntime.Selector("terminateWithSuccess"));
#endif
        return Task.CompletedTask;
    }
}
