dotnet --version | Out-Host
Write-Host "Restoring NuGet packages" 
& dotnet restore | Out-Host
if ($LastExitCode -ne 0)
{
   Write-Host "Restored failed"
   Return $LastExitCode
}
Write-Host "Building solution..."
& dotnet build -c Release | Out-Host
if ($LastExitCode -ne 0)
{
   Write-Host "Build failed"
   Return $LastExitCode
}
Write-Host "Creating NuGet packages"
& dotnet pack .\YoutubeNetDumper\YoutubeNetDumper.csproj -o ..\packages | Out-Host
if ($LastExitCode -ne 0)
{
   Write-Host "Created NuGet packages failed"
   Exit $LastExitCode
}
Exit 0