using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace application.infrastructure;

public static class DisposeHelperExtensions
{
    public static void DisposeDisposables(this object? toDispose, ILogger? log)
    {
        if (toDispose == null)
            return;

        if (log == null)
            log = NullLoggerFactory.Instance.CreateLogger(nameof(DisposeDisposables));
        var allProps = toDispose.GetType().GetProperties().ToList();
        foreach (var prop in allProps)
        {
            var disposable = prop.GetValue(toDispose) as IDisposable;
            if (disposable != null)
            {
                if (disposable == log) continue;
                log.LogDebug($"Disposing {prop.Name} [{disposable.GetType().Name}]");
            }
            disposable?.Dispose();
        }
    }

}
