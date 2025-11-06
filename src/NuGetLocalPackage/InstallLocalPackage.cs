using System;
using System.IO;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.PackageExtraction;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;

namespace NuGetLocalPackage;

public sealed class InstallLocalPackage : Task
{
    [Required]
    public ITaskItem PackagePath { get; set; } = default!;

    private static string GetLocalPackagesFolder(ISettings settings)
    {
        var section = settings.GetSection(ConfigurationConstants.FallbackPackageFolders);
        if (section == null)
        {
            throw new InvalidOperationException("NuGet.Config should have a 'fallbackPackageFolders' section with '<add key=\"local packages\" ... />'");
        }

        var item = section.GetFirstItemWithAttribute<AddItem>(ConfigurationConstants.KeyAttribute, "local packages");
        if (item == null)
        {
            throw new InvalidOperationException("NuGet.Config should have a 'fallbackPackageFolders' section with '<add key=\"local packages\" ... />'");
        }

        var value = item.Value;
        if (Path.IsPathRooted(value))
        {
            throw new InvalidOperationException(
                "local packages folder path should be relative to the NuGet.Config file, not an absolute path.");
        }

        var configPath = item.ConfigPath ?? throw new InvalidOperationException();
        var dir = Path.GetDirectoryName(configPath)!;
        return Path.Combine(dir, value);
    }

    public override bool Execute()
    {
        var packagePath = PackagePath.GetMetadata("FullPath");

        OfflineFeedUtility.ThrowIfInvalidOrNotFound(packagePath, false, "NuPkg file not found at '{0}'");

        var settings = Settings.LoadDefaultSettings(Environment.CurrentDirectory);
        var logger = NullLogger.Instance;

        var localPackageFolder = GetLocalPackagesFolder(settings);
        OfflineFeedUtility.ThrowIfInvalid(localPackageFolder);

        var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, logger);
        var packageExtractionContext = new PackageExtractionContext(
            PackageSaveMode.Defaultv3,
            PackageExtractionBehavior.XmlDocFileSaveMode,
            clientPolicyContext,
            logger);

        var offlineFeedAddContext = new OfflineFeedAddContext(
            packagePath,
            localPackageFolder,
            logger, // IConsole is an ILogger
            throwIfSourcePackageIsInvalid: true,
            throwIfPackageExistsAndInvalid: true,
            throwIfPackageExists: false,
            extractionContext: packageExtractionContext);

        AddPackageToSource(offlineFeedAddContext, CancellationToken.None).Wait();
        return true;
    }

    private static async System.Threading.Tasks.Task AddPackageToSource(
        OfflineFeedAddContext offlineFeedAddContext,
        CancellationToken token)
    {
        if (offlineFeedAddContext == null)
        {
            throw new ArgumentNullException(nameof(offlineFeedAddContext));
        }

        token.ThrowIfCancellationRequested();

        var packagePath = offlineFeedAddContext.PackagePath;
        var source = offlineFeedAddContext.Source;
        var logger = offlineFeedAddContext.Logger;

        using var packageStream = File.OpenRead(packagePath);
        using var packageReader = new PackageArchiveReader(packageStream, leaveStreamOpen: true);
        var packageIdentity = await packageReader.GetIdentityAsync(token);

        var versionFolderPathResolver = new VersionFolderPathResolver(source);
        var packageDirectory = versionFolderPathResolver.GetPackageDirectory(packageIdentity.Id, packageIdentity.Version);
        Directory.Delete(packageDirectory, true);

        using var packageDownloader = new NuGet.Protocol.LocalPackageArchiveDownloader(
            source: null,
            packageFilePath: packagePath,
            packageIdentity: packageIdentity,
            logger: logger);

        // Set Empty parentId here.
        await PackageExtractor.InstallFromSourceAsync(
            packageIdentity,
            packageDownloader,
            versionFolderPathResolver,
            offlineFeedAddContext.ExtractionContext,
            token,
            parentId: Guid.Empty);
    }
}
