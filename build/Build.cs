using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
[GitHubActions(
    nameof(PublishNuGet),
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnWorkflowDispatchRequiredInputs = new[] { nameof(Version) },
    InvokedTargets = new []{ nameof(PublishNuGet) },
    ImportSecrets = new[] { "NUGET_TOKEN", "GITHUB_TOKEN" })]
[GitHubActions(
    nameof(PullRequest),
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main" },
    InvokedTargets = new []{ nameof(PullRequest) },
    EnableGitHubToken = true,
    ImportSecrets = new[] { nameof(NugetToken) })]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version to be deployed")] public readonly string Version = "0.1.2-test1";

    [Solution(SuppressBuildProjectCheck = true)] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [CI] readonly GitHubActions GitHubActions;

    [Parameter] [Secret] readonly string NugetToken;
    const string ChangelogFileName = "CHANGELOG.md";

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath TargetProjectDirectory => SourceDirectory / "CommandLine";
    AbsolutePath PackageDirectory => TargetProjectDirectory / "nupkg";

    string MainVersion
    {
        get
        {
            var split = Version?.Split("-", 2);
            return split?[0];
        }
    }

    Target Clean => _ => _
        .Description("✨ Clean")
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(PackageDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Description("🧱 Restore")
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

    Target Pack => _ => _
        .Description("📦 NuGet Pack")
        .DependsOn(Compile)
        .Requires(() => Version)
        .Produces(PackageDirectory / "*.nupkg")
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(TargetProjectDirectory)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetProperty("PackageVersion", Version));

        });

    Target ChangelogVerification => _ => _
        .Description("👀 Changelog Verification")
        .Requires(() => Version)
        .Executes(() =>
        {
            Assert.FileExists(RootDirectory / ChangelogFileName);
            Assert.True(
                File.ReadLines(RootDirectory / ChangelogFileName).Any(line => line.StartsWith($"## [{MainVersion}")),
                $"There is no entry for version {Version} in {ChangelogFileName}");
        });

    public Target Push => _ => _
        .Description("📢 NuGet Push")
        .DependsOn(Pack, UnitTests, IntegrationTests, ChangelogVerification)
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
            GitTag tag = await github.Git.Tag.Create(
                owner,
                name,
                new NewTag
                {
                    Tag = $"v{Version}",
                    Object = GitHubActions.Sha,
                    Type = TaggedType.Commit,
                    Tagger = new Committer(GitHubActions.Actor, $"{GitHubActions.Actor}@users.noreply.github.com", DateTimeOffset.UtcNow),
                    Message = "Package published in NuGet.org"
                });

            await github.Git.Reference.Create(
                owner,
                name,
                new NewReference($"refs/tags/v{Version}", tag.Object.Sha));
        });

    Target PullRequest => _ => _
        .Description("🏷 Pull Request")
        .Requires(() => GitHubActions)
        .Triggers(UnitTests, IntegrationTests)
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
                var pullRequestFiles = await github.PullRequest.Files(owner, name, GitHubActions.PullRequestNumber.Value);
                try
                {
                    Assert.True(pullRequestFiles.Any(x => x.FileName == ChangelogFileName));
                }
                catch
                {
                    foreach (string fileName in pullRequestFiles.Select(x => x.FileName))
                    {
                        Log.Information("PR File: {FileName}", fileName);
                    }

                    throw;
                }
            }
        });

    Target PublishNuGet => _ => _
        .Description("Publish NuGet")
        .Triggers(Push);
}
