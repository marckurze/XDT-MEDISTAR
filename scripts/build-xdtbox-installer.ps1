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

function Assert-PathInsideRepository {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $resolvedRoot = [IO.Path]::GetFullPath($RepositoryRoot)
    $resolvedPath = [IO.Path]::GetFullPath($Path)
    if (-not $resolvedPath.StartsWith($resolvedRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Sicherheitsabbruch: Build-Artefaktpfad liegt ausserhalb des Repositorys: $resolvedPath"
    }
}

function Reset-BuildArtifactDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    Assert-PathInsideRepository -Path $Path -RepositoryRoot $repoRoot
    if (Test-Path -LiteralPath $Path) {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Test-IsTextFileForPublishValidation {
    param(
        [Parameter(Mandatory = $true)]
        [IO.FileInfo]$File
    )

    $textExtensions = @(
        ".json",
        ".config",
        ".xml",
        ".txt",
        ".md",
        ".ini",
        ".log",
        ".ps1",
        ".cmd",
        ".bat",
        ".xaml"
    )

    foreach ($extension in $textExtensions) {
        if ([string]::Equals($extension, $File.Extension, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Assert-CustomerPublishIsClean {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $forbiddenNamePatterns = @(
        "XdtBox.LicenseManager*",
        "XdtBox.LicenseIssuer*",
        "*.Tests.*",
        "*.pem",
        "*.key",
        "*.xdtboxbackup",
        "*.gdt",
        "*.xdt",
        "*.log",
        "app-settings.json",
        "license.xdtboxlic",
        "license.json",
        "license-customer.json",
        "license-customer-data.json",
        "device-grace-periods.json",
        "device-image-overrides.json",
        "license-history.json",
        "license-manager-settings.json"
    )

    $forbiddenItems = foreach ($pattern in $forbiddenNamePatterns) {
        Get-ChildItem -Path $Path -Recurse -Force -Filter $pattern -ErrorAction SilentlyContinue
    }

    $forbiddenDirectoryNames = @(
        "profiles",
        "UserDefined",
        "interfaces",
        "template-packages",
        "templates",
        "DeviceImages",
        "Backups",
        "Backup",
        "logs",
        "TestData",
        "Fixtures"
    )

    $forbiddenDirectories = Get-ChildItem -Path $Path -Recurse -Force -Directory -ErrorAction SilentlyContinue |
        Where-Object {
            $directoryName = $_.Name
            $match = $false
            foreach ($forbiddenDirectoryName in $forbiddenDirectoryNames) {
                if ([string]::Equals($forbiddenDirectoryName, $directoryName, [StringComparison]::OrdinalIgnoreCase)) {
                    $match = $true
                    break
                }
            }

            $match
        }

    $forbiddenTextTokens = @(
        "C:\Users\MarcK",
        "MarcK",
        "C:\XDTBox\RT3100RS232",
        "Patient2Box",
        "COM4",
        "device-image-overrides.json",
        "license.xdtboxlic",
        "license-history.json",
        "license-manager-settings.json"
    )

    $textHits = New-Object System.Collections.Generic.List[string]
    foreach ($file in Get-ChildItem -Path $Path -Recurse -Force -File -ErrorAction SilentlyContinue) {
        if (-not (Test-IsTextFileForPublishValidation -File $file)) {
            continue
        }

        $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction SilentlyContinue
        foreach ($token in $forbiddenTextTokens) {
            if ($content -and $content.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
                $textHits.Add("$($file.FullName) enthaelt '$token'")
            }
        }
    }

    $violations = @()
    if ($forbiddenItems) {
        $violations += $forbiddenItems | Select-Object -ExpandProperty FullName
    }

    if ($forbiddenDirectories) {
        $violations += $forbiddenDirectories | Select-Object -ExpandProperty FullName
    }

    if ($textHits.Count -gt 0) {
        $violations += $textHits
    }

    if ($violations.Count -gt 0) {
        $list = $violations -join [Environment]::NewLine
        throw "Kundenpublish enthaelt lokale Kundendaten/Entwicklungsdaten:$([Environment]::NewLine)$list"
    }
}

Reset-BuildArtifactDirectory -Path $publishDir
Reset-BuildArtifactDirectory -Path $installerDir

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

Assert-CustomerPublishIsClean -Path $publishDir

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
