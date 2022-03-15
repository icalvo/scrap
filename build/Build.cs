using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static System.Environment;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
[GitHubActions(
    "publish-nuget-nuke",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnWorkflowDispatchRequiredInputs = new[] { nameof(Version) },
    InvokedTargets = new []{ nameof(Push) },
    ImportSecrets = new[] { "NUGET_TOKEN"})]
[GitHubActions(
    "pull-request-nuke",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPullRequestBranches = new[] { "main" },
    InvokedTargets = new []{ nameof(IntegrationTests) })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version to be deployed")] public readonly string Version = "0.1.2-test1";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [CI] readonly GitHubActions GitHubActions;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";

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
            EnsureCleanDirectory(OutputDirectory);
            DeleteFile(SourceDirectory / "scrap.db");
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
                .EnableNoBuild()
                .SetLoggers("\"console;verbosity=normal\""));
        });


    Target IntegrationTests => _ => _
        .Description("🐛 Integration Tests")
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(o => o
                .SetProjectFile(SourceDirectory / "IntegrationTests")
                .EnableNoBuild()
                .SetLoggers("\"console;verbosity=normal\"")
                .SetProcessEnvironmentVariable(
                    "Scrap_GlobalConfigurationFolder",
                    GetEnvironmentVariable("Scrap_GlobalConfigurationFolder")));
        });

    Target Pack => _ => _
        .Description("📦 NuGet Pack")
        .DependsOn(Compile)
        .Requires(() => Version)
        .Executes(() =>
        {
            DotNetPack(s => s
                .EnableNoBuild()
                .SetProperty("PackageVersion", Version));
        });

    public Target Push => _ => _
        .Description("📢 NuGet Push")
        .DependsOn(Pack, UnitTests, IntegrationTests)
        .Triggers(TagCommit)
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetTargetPath(SourceDirectory / "CommandLine" / "nupkg" / "scrap.*.nupkg")
                .SetApiKey(GetEnvironmentVariable("NUGET_AUTH_TOKEN"))
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
            Git($"tag v{Version}");
            Git($"push origin --tags");
        });
}
