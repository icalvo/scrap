properties {
    $split = "$version" -split "-",2
    $mainversion = $split[0]
}

function Check() {
    param (
        [ScriptBlock]$block
    )
    
    $block.Invoke();
    if ($lastexitcode -ne 0)
    {
        throw "Command-line program failed: $($block.ToString())"
    }
}

FormatTaskName ""

task default -Depends TestParams

Task Init {
    Assert ($version -ne $null) '$version should not be null'
    Assert ($actor -ne $null) '$actor should not be null'
    Set-Location "$PSScriptRoot/src"
}

Task Clean -Depends Init {
    "✨ Clean"
    Check { dotnet clean }
    if (Test-Path ./scrap.db)
    {
        Remove-Item -Force ./scrap.db
    }
}

Task Build -Depends Clean {
    "🏭 Restore dependencies"
    Check { dotnet restore }
    "🧱 Build"
    Check { dotnet build /p:Version="$version" /p:AssemblyVersion="$mainversion" /p:FileVersion="$mainversion" /p:InformationalVersion="$version" --no-restore }
}

Task UnitTests -Depends Build {
    "🐛 Test"
    # Check { dotnet test --no-build --logger:"console;verbosity=normal" }
}

Task Pack -Depends Build {
    "📦 NuGet Pack"
    Check { dotnet pack /p:PackageVersion="$version" --no-build }
}

Task Install -Depends Pack {
    "🛠 Install tool"
    Check { dotnet tool uninstall scrap --global }
    Check { dotnet tool install scrap --global --add-source ./CommandLine/nupkg/ --version '0.1.2-test1' }
}

Task ConfigureIntegrationTests -Depends Install {
    "⚙ Configure tool for integration tests"
    $jobDefsFullPath = Resolve-Path ./Tests/jobDefinitions.json

    $dbFullPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("./scrap.db")
    Check { scrap config /key=Scrap:Definitions /value=$jobDefsFullPath }
    Check { scrap config /key=Scrap:Database /value="Filename=$dbFullPath;Connection=shared" }
}

Task IntegrationTests -Depends ConfigureIntegrationTests {
    "🌍 Install and start web server for integration tests"
    Check { dotnet tool install dotnet-serve --global }
    $wwwPath = Resolve-Path ./Tests/www/
    $serverproc = Start-Process "dotnet" -ArgumentList "serve --directory $wwwPath --port 8080" -PassThru -WorkingDirectory .
    "🐛 Test"
    Check { dotnet test --no-build --logger:"console;verbosity=normal" }
    $serverproc.Kill()
}

Task Push -Depends Pack {
    "📢 NuGet Push"
    Check { dotnet nuget push ./CommandLine/nupkg/scrap.*.nupkg -k $env:NUGET_AUTH_TOKEN -s https://api.nuget.org/v3/index.json }
}

Task TagCommit -Depends Push {
    "🏷 Tag commit and push"
    Check { git config --global user.email "$actor@users.noreply.github.com" }
    Check { git config --global user.name "$actor" }
    Check { git tag v$version }
    Check { git push origin --tags }
}

task Publish -Depends TagCommit

