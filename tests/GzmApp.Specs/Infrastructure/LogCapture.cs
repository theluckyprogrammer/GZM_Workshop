using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace GzmApp.Specs.Infrastructure;

// Collects log messages of the app under test (category + text), e.g. the "SQL" log of SqlRunner.
public sealed class LogCapture : ILoggerProvider
{
    private readonly ConcurrentQueue<(string Category, string Message)> _entries = new();

    public IReadOnlyList<(string Category, string Message)> Entries => _entries.ToList();

    public IEnumerable<string> Messages(string category) => _entries.Where(e => e.Category == category).Select(e => e.Message);

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _entries);

    public void Dispose()
    {
    }

    private sealed class Logger(string category, ConcurrentQueue<(string, string)> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            entries.Enqueue((category, formatter(state, exception)));
    }
}
