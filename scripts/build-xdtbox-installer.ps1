[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.12",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "artifacts\publish\XDTBox"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$stagingRoot = Join-Path $repoRoot "artifacts\staging\xdtbox-installer"
$stagedPublishDir = Join-Path $stagingRoot "publish\XDTBox"
$stagedInstallerDir = Join-Path $stagingRoot "installer"
$projectPath = Join-Path $repoRoot "XdtDeviceBridge.App\XdtDeviceBridge.App.csproj"
$innoScript = Join-Path $repoRoot "installer\XDTBox.iss"
$versionInfoVersion = "$Version.0.0"
$setupFileName = "XDTBox_Setup_$Version.exe"

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

function Replace-BuildArtifactDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationPath
    )

    Assert-PathInsideRepository -Path $SourcePath -RepositoryRoot $repoRoot
    Assert-PathInsideRepository -Path $DestinationPath -RepositoryRoot $repoRoot

    if (-not (Test-Path -LiteralPath $SourcePath)) {
        throw "Staging-Artefaktordner wurde nicht gefunden: $SourcePath"
    }

    $destinationParent = Split-Path -Parent $DestinationPath
    New-Item -ItemType Directory -Force -Path $destinationParent | Out-Null

    $backupPath = "$DestinationPath.previous-$([DateTime]::UtcNow.ToString('yyyyMMddHHmmssfff'))"
    Assert-PathInsideRepository -Path $backupPath -RepositoryRoot $repoRoot

    $hasBackup = $false
    try {
        if (Test-Path -LiteralPath $DestinationPath) {
            Move-Item -LiteralPath $DestinationPath -Destination $backupPath
            $hasBackup = $true
        }

        Move-Item -LiteralPath $SourcePath -Destination $DestinationPath

        if ($hasBackup -and (Test-Path -LiteralPath $backupPath)) {
            Remove-Item -LiteralPath $backupPath -Recurse -Force
        }
    }
    catch {
        if ((-not (Test-Path -LiteralPath $DestinationPath)) -and $hasBackup -and (Test-Path -LiteralPath $backupPath)) {
            Move-Item -LiteralPath $backupPath -Destination $DestinationPath
        }

        throw
    }
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

function Assert-BuiltInDeviceAssetsExist {
    $deviceAssetsDir = Join-Path $repoRoot "XdtDeviceBridge.App\Assets\Devices"
    $deviceInfoAssetsDir = Join-Path $repoRoot "XdtDeviceBridge.App\Assets\DeviceInfo"
    $requiredBuiltInDeviceAssets = @(
        "Topcon_CV5000_freigestellt.png",
        "device-document-attachment-default.png",
        "device-manual-document-selection-default.png",
        "device-nidek-ar360-default.png",
        "device-nidek-ark1s-default.png",
        "device-nidek-lm7-default.png",
        "device-nidek-nt530p-default.png",
        "device-nidek-rt2100-serial-default.png",
        "device-nidek-rt3100-serial-default.png",
        "device-nidek-rt5100-serial-default.png",
        "device-nidek-rt6100-default.png",
        "device-topcon-cl300-default.png",
        "device-topcon-ct1p-default.png",
        "device-topcon-ct800a-default.png",
        "device-topcon-kr1-default.png",
        "device-topcon-kr800-default.png",
        "device-topcon-solos-default.png",
        "device-topcon-trk2p-default.png"
    )

    $missingAssets = foreach ($asset in $requiredBuiltInDeviceAssets) {
        $assetPath = Join-Path $deviceAssetsDir $asset
        if (-not (Test-Path -LiteralPath $assetPath)) {
            $assetPath
        }
    }

    if ($missingAssets) {
        $list = $missingAssets -join [Environment]::NewLine
        throw "Offizielle BuiltIn-Geraetebilder fehlen im App-Asset-Ordner:$([Environment]::NewLine)$list"
    }

    $deviceInfoCatalogPath = Join-Path $deviceInfoAssetsDir "device-technical-profiles.de.json"
    if (-not (Test-Path -LiteralPath $deviceInfoCatalogPath)) {
        throw "Offizielle BuiltIn-Geraete-Steckbriefe fehlen im App-Asset-Ordner:$([Environment]::NewLine)$deviceInfoCatalogPath"
    }
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

try {
    Reset-BuildArtifactDirectory -Path $stagingRoot
    New-Item -ItemType Directory -Force -Path $stagedPublishDir | Out-Null
    New-Item -ItemType Directory -Force -Path $stagedInstallerDir | Out-Null
    Assert-BuiltInDeviceAssetsExist

    dotnet publish $projectPath `
        -c $Configuration `
        -r $Runtime `
        --self-contained true `
        -p:PublishSingleFile=false `
        -p:Version=$Version `
        -p:AssemblyVersion=$versionInfoVersion `
        -p:FileVersion=$versionInfoVersion `
        -p:InformationalVersion=$Version `
        -o $stagedPublishDir

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish ist fehlgeschlagen. Finale Publish-/Installer-Artefakte wurden nicht ersetzt."
    }

    Assert-CustomerPublishIsClean -Path $stagedPublishDir

    if ($SkipInstaller) {
        Replace-BuildArtifactDirectory -SourcePath $stagedPublishDir -DestinationPath $publishDir
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
        throw "Inno Setup 6 Compiler wurde nicht gefunden. Bitte Inno Setup 6 installieren oder ISCC_EXE auf ISCC.exe setzen. Validierte Publish-Ausgabe liegt vorlaeufig unter: $stagedPublishDir"
    }

    & $iscc "/DMyPublishDir=$stagedPublishDir" "/DMyInstallerOutputDir=$stagedInstallerDir" "/DMyAppVersion=$Version" "/DMyVersionInfoVersion=$versionInfoVersion" $innoScript

    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup Build ist fehlgeschlagen. Finale Publish-/Installer-Artefakte wurden nicht ersetzt."
    }

    $stagedSetupFile = Join-Path $stagedInstallerDir $setupFileName
    if (-not (Test-Path -LiteralPath $stagedSetupFile)) {
        throw "Installer wurde nicht gefunden: $stagedSetupFile"
    }

    Replace-BuildArtifactDirectory -SourcePath $stagedPublishDir -DestinationPath $publishDir
    Replace-BuildArtifactDirectory -SourcePath $stagedInstallerDir -DestinationPath $installerDir

    $setupFile = Join-Path $installerDir $setupFileName
    Write-Host "Publish fertig: $publishDir"
    Write-Host "Installer fertig: $setupFile"
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force
    }
}
