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
    "🐛 Unit Tests"
    Check { dotnet test ./UnitTests/ --no-build --logger:"console;verbosity=normal" }
}

Task Pack -Depends Build {
    "📦 NuGet Pack"
    Check { dotnet pack /p:PackageVersion="$version" --no-build }
}

Task Install -Depends Pack {
    "🛠 Install tool"
    dotnet tool uninstall scrap --tool-path install
    Check { dotnet tool install scrap --tool-path install --add-source ./CommandLine/nupkg/ --version '0.1.2-test1' }
}

Task ConfigureIntegrationTests -Depends Install {
    "⚙ Configure tool for integration tests"
    $installFullPath = Resolve-Path ./install
    $env:Scrap_GlobalConfigurationFolder=$installFullPath
    $jobDefsFullPath = Resolve-Path ./IntegrationTests/jobDefinitions.json

    $dbFullPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("./scrap.db")
    Check { ./install/scrap.exe config /key=Scrap:Definitions /value=$jobDefsFullPath }
    Check { ./install/scrap.exe config /key=Scrap:Database /value="Filename=$dbFullPath;Connection=shared" }
}

Task RunIntegrationTests -Depends ConfigureIntegrationTests {
    "🌍 Install and start web server for integration tests"
    dotnet tool uninstall dotnet-serve --tool-path install
    Check { dotnet tool install dotnet-serve --tool-path install }
    $wwwPath = Resolve-Path ./IntegrationTests/www/
    $serverproc = Start-Process "./install/dotnet-serve.exe" -ArgumentList "--directory $wwwPath --port 8080" -PassThru -WorkingDirectory .
    "🐛 Integration Tests"
    try
    {
        Check {
            dotnet test `
                ./IntegrationTests/ `
                --no-build `
                --logger:"console;verbosity=normal" `
                --environment Scrap_GlobalConfigurationFolder="$env:Scrap_GlobalConfigurationFolder"
        }
    }
    finally {
        $serverproc.Kill()
    }
}

Task Push -Depends Pack,UnitTests {
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

Task Publish -Depends TagCommit

Task IntegrationTests -Depends RunIntegrationTests {
    Check { dotnet tool uninstall dotnet-serve --tool-path install }
    Check { dotnet tool uninstall scrap --tool-path install }
    del $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("./scrap.db")
}