using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace NuGetLocalPackage.Tests;

internal sealed class MockLogger : ILogger, IEnumerable<BuildEventArgs>
{
    private readonly List<BuildEventArgs> _events = new List<BuildEventArgs>();

    public void Initialize(IEventSource eventSource)
    {
        eventSource.HandleAnyEventRaised(OnAnyEvent);
    }

    private void OnAnyEvent(object sender, BuildEventArgs e)
    {
        _events.Add(e);
    }

    public void Shutdown()
    {
    }

    public LoggerVerbosity Verbosity { get; set; }
    public string? Parameters { get; set; }

    public IEnumerator<BuildEventArgs> GetEnumerator() => _events.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
