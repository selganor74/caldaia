using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace application.infrastructure;

public static class DisposeHelperExtensions
{
    public static void DisposeDisposables(this object? toDispose, ILogger? log)
    {
        if (toDispose == null)
            return;

        if (log == null)
            log = NullLoggerFactory.Instance.CreateLogger(nameof(DisposeDisposables));
        var allFields = toDispose.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .ToList();

    
        foreach (var field in allFields)
        {
            var disposable = field.GetValue(toDispose) as IDisposable;
            if (disposable != null)
            {
                if (disposable == log) continue;
                log.LogDebug($"Disposing {field.Name} [{disposable.GetType().Name}]");
            }
            disposable?.Dispose();
        }


        var allProperties = toDispose.GetType()
         .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
         .ToList();

        foreach (var prop in allProperties)
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
