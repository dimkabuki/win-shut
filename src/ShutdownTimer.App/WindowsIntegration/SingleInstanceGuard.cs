using System.Threading;

namespace ShutdownTimer.App.WindowsIntegration;

/// <summary>
/// Holds a named mutex for the lifetime of the application. Dispose this guard
/// on the same thread that acquired it.
/// </summary>
public sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex _mutex;
    private bool _disposed;

    private SingleInstanceGuard(Mutex mutex)
    {
        _mutex = mutex;
    }

    /// <summary>
    /// Acquires the named mutex for a newly-created instance, or returns null
    /// when another instance already owns or has created it.
    /// </summary>
    public static SingleInstanceGuard? TryAcquire(string mutexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mutexName);

        var mutex = new Mutex(initiallyOwned: true, name: mutexName, createdNew: out var createdNew);
        if (!createdNew)
        {
            mutex.Dispose();
            return null;
        }

        return new SingleInstanceGuard(mutex);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _mutex.ReleaseMutex();
        _mutex.Dispose();
    }
}
