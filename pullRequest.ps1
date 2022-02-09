dotnet restore
dotnet build /p:Version='0.1.2-test1' /p:AssemblyVersion='0.1.2' /p:FileVersion='0.1.2' /p:InformationalVersion='0.1.2-test1' --no-restore
dotnet pack /p:PackageVersion='0.1.2-test1' --no-build
dotnet tool uninstall scrap --global
dotnet tool install scrap --global --add-source ./CommandLine/nupkg/ --version '0.1.2-test1'
$jobDefsFullPath = Resolve-Path ./Tests/jobDefinitions.json
if (Test-Path ./scrap.db)
{
    Remove-Item -Force ./scrap.db
}

$dbFullPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("./scrap.db")
scrap config /key=Scrap:Definitions /value=$jobDefsFullPath
scrap config /key=Scrap:Database /value="Filename=$dbFullPath;Connection=shared"
dotnet tool install dotnet-serve --global
$wwwPath = Resolve-Path .\Tests\www\
$serverproc = Start-Process "dotnet" -ArgumentList "serve --directory $wwwPath --port 8080" -WindowStyle Hidden -PassThru -WorkingDirectory .
dotnet test --no-build --logger:"console;verbosity=normal"
$serverproc.Kill()
