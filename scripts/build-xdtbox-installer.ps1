[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "artifacts\publish\XDTBox"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$projectPath = Join-Path $repoRoot "XdtDeviceBridge.App\XdtDeviceBridge.App.csproj"
$innoScript = Join-Path $repoRoot "installer\XDTBox.iss"

New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
New-Item -ItemType Directory -Force -Path $installerDir | Out-Null

dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:Version=$Version `
    -p:AssemblyVersion=1.0.0.0 `
    -p:FileVersion=1.0.0.0 `
    -p:InformationalVersion=$Version `
    -o $publishDir

$forbiddenPatterns = @(
    "XdtBox.LicenseManager*",
    "XdtBox.LicenseIssuer*",
    "*.Tests.*",
    "*.pem",
    "*.key",
    "license-history.json",
    "license-manager-settings.json"
)

$forbidden = foreach ($pattern in $forbiddenPatterns) {
    Get-ChildItem -Path $publishDir -Recurse -Force -Filter $pattern -ErrorAction SilentlyContinue
}

if ($forbidden) {
    $list = ($forbidden | Select-Object -ExpandProperty FullName) -join [Environment]::NewLine
    throw "Publish-Ausgabe enthaelt Dateien, die nicht in den Kundeninstaller duerfen:$([Environment]::NewLine)$list"
}

if ($SkipInstaller) {
    Write-Host "Publish fertig: $publishDir"
    Write-Host "Installer-Build wurde durch -SkipInstaller uebersprungen."
    return
}

$iscc = $env:ISCC_EXE
if ([string]::IsNullOrWhiteSpace($iscc)) {
    $cmd = Get-Command iscc.exe -ErrorAction SilentlyContinue
    if ($cmd) {
        $iscc = $cmd.Source
    }
}

if ([string]::IsNullOrWhiteSpace($iscc)) {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) {
            $iscc = $candidate
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($iscc) -or -not (Test-Path -LiteralPath $iscc)) {
    throw "Inno Setup 6 Compiler wurde nicht gefunden. Bitte Inno Setup 6 installieren oder ISCC_EXE auf ISCC.exe setzen. Publish-Ausgabe liegt unter: $publishDir"
}

& $iscc $innoScript

$setupFile = Join-Path $installerDir "XDTBox_Setup_1.0.exe"
if (-not (Test-Path -LiteralPath $setupFile)) {
    throw "Installer wurde nicht gefunden: $setupFile"
}

Write-Host "Publish fertig: $publishDir"
Write-Host "Installer fertig: $setupFile"
