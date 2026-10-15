# One-click start for Windows: checks prerequisites, then runs the Aspire AppHost.
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

foreach ($tool in @(
    @{ Name = 'dotnet'; Hint = 'install .NET 10 SDK: https://dotnet.microsoft.com/download' },
    @{ Name = 'node';   Hint = 'install Node.js 22.12+: https://nodejs.org' },
    @{ Name = 'docker'; Hint = 'install Docker Desktop and make sure it is running' })) {
    if (-not (Get-Command $tool.Name -ErrorAction SilentlyContinue)) {
        throw "Missing prerequisite: $($tool.Name) - $($tool.Hint)"
    }
}

docker info *> $null
if ($LASTEXITCODE -ne 0) { throw 'Docker is installed but not running. Start Docker Desktop and try again.' }

dotnet run --project src/SlotBooking.AppHost/SlotBooking.AppHost.csproj
