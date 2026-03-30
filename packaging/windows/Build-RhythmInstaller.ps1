param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Version = "1.0.0"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$publishDirectory = Join-Path $root "artifacts\publish\$RuntimeIdentifier"
$outputDirectory = Join-Path $root "artifacts\installer"
$issPath = Join-Path $PSScriptRoot "Rhythm.iss"
$innoCompilerPath = "D:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$setupPath = Join-Path $outputDirectory "Rhythm-Setup-$Version-$RuntimeIdentifier.exe"

New-Item -ItemType Directory -Force -Path $publishDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

Push-Location $root
try {
    dotnet publish .\src\Rhythm.App\Rhythm.App.csproj `
        -c $Configuration `
        -r $RuntimeIdentifier `
        --self-contained false `
        -p:PublishSingleFile=false `
        -o $publishDirectory
}
finally {
    Pop-Location
}

if (Test-Path $setupPath) {
    Remove-Item -LiteralPath $setupPath -Force
}

& $innoCompilerPath `
    "/DMyAppVersion=$Version" `
    "/DMyAppSourceDir=$publishDirectory" `
    "/DMyAppOutputDir=$outputDirectory" `
    $issPath

Write-Host "Installer created at $setupPath"
