using System;
using System.Linq;
using System.Reflection;

namespace NuGetLocalPackage.Tests;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class FixturesTargetPathAttribute(string name, string targetPath) : Attribute
{
    public string Name { get; } = name;
    public string TargetPath { get; } = targetPath;

    public static string GetTargetPath(string name)
    {
        return Assembly.GetExecutingAssembly()
            .GetCustomAttributes<FixturesTargetPathAttribute>()
            .Single(x => x.Name == name)
            .TargetPath;
    }
}
