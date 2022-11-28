namespace application.infrastructure;

public static class DisposeHelperExtensions
{
    public static void DisposeDisposables(this object? toDispose)
    {
        if (toDispose == null)
            return;

        var allProps = toDispose.GetType().GetProperties().ToList();
        foreach (var prop in allProps)
        {
            var disposable = prop.GetValue(toDispose) as IDisposable;

            if (disposable != null)
                Console.WriteLine($"Disposing {prop.Name} [{disposable.GetType().Name}]");

            disposable?.Dispose();
        }
    }

}
