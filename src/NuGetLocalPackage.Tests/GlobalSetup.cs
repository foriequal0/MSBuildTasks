using System;
using Microsoft.Build.Locator;
using NuGetLocalPackage.Tests;
using Xunit;

// MSBuild internal doesn't like parallel build invocation even with isolated BuildManager
[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: AssemblyFixture(typeof(GlobalSetup))]

namespace NuGetLocalPackage.Tests;

public sealed class GlobalSetup : IDisposable
{
    public GlobalSetup()
    {
        MSBuildLocator.RegisterDefaults();
    }

    public void Dispose()
    {
        MSBuildLocator.Unregister();
    }
}
