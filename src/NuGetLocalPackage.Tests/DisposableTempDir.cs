using System;
using System.IO;

namespace NuGetLocalPackage.Tests;

public sealed class DisposableTempDir : IDisposable
{
    private readonly DirectoryInfo _di = Directory.CreateTempSubdirectory("ResolveUsingTasemblyName.Tests");

    public string CreateFile(string name, string content)
    {
        var path = Path.Combine(_di.FullName, name);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        _di.Delete(true);
    }
}
