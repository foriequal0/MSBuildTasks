using System;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace NuGetLocalPackage.Tests;

public sealed class IsolatedBuildManager : IDisposable
{
    private readonly BuildManager _buildManager;

    public IsolatedBuildManager()
    {
        _buildManager = new BuildManager();
    }

    public BuildResult Build(Project project, string target, ILogger logger)
    {
        var instance = project.CreateProjectInstance();
        var data = new BuildRequestData(instance, [target,]);
        var parameters = new BuildParameters
        {
            Loggers = [logger,],
            LogTaskInputs = true,
            MaxNodeCount = 1,
        };

        return _buildManager.Build(parameters, data);
    }

    public void Dispose()
    {
        _buildManager.Dispose();
    }
}
