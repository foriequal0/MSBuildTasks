using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Shouldly;
using Xunit;

namespace NuGetLocalPackage.Tests;

public sealed class SmokeTest
{
    [Fact]
    public void ShouldFailWithoutInit()
    {
        using var tempdir = new DisposableTempDir();
        tempdir.CreateFile("NuGet.Config", """
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <fallbackPackageFolders>
       <add key="local packages" value="./packages" />
    </fallbackPackageFolders>
</configuration>
""");

        var xml = tempdir.CreateFile("test/test.proj", """
<Project>
  <UsingTask TaskName="InstallLocalPackage" AssemblyFile="$(NuGetLocalPackageAssemblyFile)" />

  <Target Name="Hello">
    <InstallLocalPackage PackagePath="E:\workspace\MsBuildTasks\src\NuGetLocalPackage\bin\Release\NuGetLocalPackage.0.0.1.nupkg" />
  </Target>
</Project>
""");
        var project = new Project(xml, new Dictionary<string, string>()
        {
            ["NuGetLocalPackageAssemblyFile"] = typeof(InstallLocalPackage).Assembly.Location,
        }, null);

        var logger = new MockLogger();
        using (var bm = new IsolatedBuildManager())
        {
            bm.Build(project, "Hello", logger);
        }
    }
}
