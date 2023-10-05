using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

[ShutdownDotNetAfterServerBuild]
[GitHubActions(
    nameof(PublishNuGet),
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    Submodules = GitHubActionsSubmodules.True,
    OnWorkflowDispatchRequiredInputs = new[] { nameof(Version) },
    InvokedTargets = new[] { nameof(PublishNuGet) },
    ImportSecrets = new[] { "NUGET_TOKEN", "GITHUB_TOKEN" },
    WritePermissions = new [] { GitHubActionsPermissions.Contents, GitHubActionsPermissions.PullRequests })]
[GitHubActions(
    nameof(PullRequest),
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    Submodules = GitHubActionsSubmodules.True,
    OnPullRequestBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(PullRequest) },
    EnableGitHubToken = true)]
class Build : NukeBuild
{
    const string ChangelogFileName = "CHANGELOG.md";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [CI] readonly GitHubActions GitHubActions;

    [PathVariable] readonly Tool Git;
    [GitRepository] readonly GitRepository GitRepository;
    [Parameter] [Secret] readonly string NugetToken;

    [Solution(SuppressBuildProjectCheck = true)] readonly Solution Solution;

    [Parameter("Version to be deployed")] public readonly string Version = "0.1.2-test1";

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TargetProjectDirectory => SourceDirectory / "CommandLine";
    AbsolutePath PackageDirectory => TargetProjectDirectory / "packages";

    string MainVersion
    {
        get
        {
            var split = Version?.Split("-", 2);
            return split?[0];
        }
    }

    Target None =>
        _ => _
            .Description("✨ Recreate CI scripts")
            .Executes(() =>
            {
            });
    
    Target Clean => _ => _
        .Description("✨ Clean")
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
        });

    Target Restore => _ => _
        .Description("🧱 Restore")
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .Description("🧱 Build")
        .DependsOn(Restore)
        .Requires(() => Version)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(Version)
                .SetAssemblyVersion(MainVersion)
                .SetFileVersion(MainVersion)
                .SetInformationalVersion(Version)
                .EnableNoRestore());
        });

    Target UnitTests => _ => _
        .Description("🐛 Unit Tests")
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(o => o
                .SetProjectFile(SourceDirectory / "UnitTests")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetLoggers("console;verbosity=normal"));
        });

    Target IntegrationTests => _ => _
        .Description("🐛 Integration Tests")
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(o => o
                .SetProjectFile(SourceDirectory / "IntegrationTests")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetLoggers("console;verbosity=normal"));
        });

    Target SystemTests => _ => _
        .Description("🐛 System Tests")
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(o => o
                .SetProjectFile(SourceDirectory / "SystemTests")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetLoggers("console;verbosity=normal"));
        });

    Target Pack => _ => _
        .Description("📦 NuGet Pack")
        .DependsOn(Compile)
        .Requires(() => Version)
        .Produces(PackageDirectory / "*.nupkg")
        .Executes(() =>
        {
            PackageDirectory.CreateOrCleanDirectory();
            DotNetPack(s => s
                .SetProject(TargetProjectDirectory)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetProperty("PackageVersion", Version)
                .SetOutputDirectory(PackageDirectory));
        });

    public Target Push => _ => _
        .Description("📢 NuGet Push")
        .DependsOn(Pack, UnitTests, IntegrationTests, SystemTests, BumpChangelogVersionDate)
        .Consumes(Pack)
        .Triggers(TagCommit)
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetTargetPath(PackageDirectory / "*.nupkg")
                .SetApiKey(NugetToken)
                .SetSource("https://api.nuget.org/v3/index.json"));
        });

    Target TagCommit => _ => _
        .Description("🏷 Tag commit and push")
        .Requires(() => GitHubActions)
        .Requires(() => Version)
        .Executes(() =>
        {
            Git($"config --global user.email \"{GitHubActions.Actor}@users.noreply.github.com\"");
            Git($"config --global user.name \"{GitHubActions.Actor}\"");

            Git($"tag v{Version} -a -m \"Package published in NuGet.org\"");
            Git($"push --tags");
        });

    Target BumpChangelogVersionDate => _ => _
        .Description("👀 Verify CHANGELOG and bump version date")
        .Requires(() => GitHubActions)
        .Requires(() => Version)
        .Executes(() =>
        {
            AbsolutePath changelog = RootDirectory / ChangelogFileName;
            Assert.FileExists(changelog);
            
            var lines = changelog.ReadAllLines().ToList();
            
            var versionTitle = $"## [{MainVersion}] {DateTime.UtcNow:YYYY-MM-dd}";
            var versionTitleLineIndex = lines.IndexOf(versionTitle);
            Log.Information("New version title: {NewVersionTitle}", versionTitle);
            Log.Information("Current version title: {CurrentVersionTitle}", lines[versionTitleLineIndex]);
            if (lines[versionTitleLineIndex] == versionTitle) return;
            lines[versionTitleLineIndex] = versionTitle;
            changelog.WriteAllLines(lines);
            Git($"config --global user.email \"{GitHubActions.Actor}@users.noreply.github.com\"");
            Git($"config --global user.name \"{GitHubActions.Actor}\"");

            Git($"commit -am \"Bump CHANGELOG date\"");
            Git($"push");
        });

    Target PullRequest => _ => _
        .Description("🏷 Pull Request")
        .Requires(() => GitHubActions)
        .Triggers(UnitTests, IntegrationTests, SystemTests)
        .Executes(async () =>
        {
            var tokenAuth = new Credentials(GitHubActions.Token);
            var github = new GitHubClient(new ProductHeaderValue("build-script"))
            {
                Credentials = tokenAuth
            };
            var split = GitHubActions.Repository.Split("/");
            var owner = split[0];
            var name = split[1];
            if (GitHubActions.PullRequestNumber != null)
            {
                var pullRequestFiles =
                    await github.PullRequest.Files(owner, name, GitHubActions.PullRequestNumber.Value);
                try
                {
                    Assert.True(pullRequestFiles.Any(x => x.FileName == ChangelogFileName));
                }
                catch
                {
                    foreach (var fileName in pullRequestFiles.Select(x => x.FileName))
                        Log.Information("PR File: {FileName}", fileName);

                    throw;
                }
            }
        });

    Target PublishNuGet => _ => _
        .Description("Publish NuGet")
        .Triggers(Push);

    public static int Main() => Execute<Build>(x => x.Compile);
}
