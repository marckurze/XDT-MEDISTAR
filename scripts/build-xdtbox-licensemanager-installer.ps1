[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.11",
    [switch]$SkipInstaller,
    [switch]$SkipStartValidation
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "artifacts\publish\XDTBox Lizenzmanager"
$installerDir = Join-Path $repoRoot "artifacts\Lizenzmanager Setup"
$stagingRoot = Join-Path $repoRoot "artifacts\staging\xdtbox-licensemanager-installer"
$stagedPublishDir = Join-Path $stagingRoot "publish\XDTBox Lizenzmanager"
$stagedInstallerDir = Join-Path $stagingRoot "Lizenzmanager Setup"
$projectPath = Join-Path $repoRoot "XdtBox.LicenseManager\XdtBox.LicenseManager.csproj"
$innoScript = Join-Path $repoRoot "installer\XDTBox.LicenseManager.iss"
$versionInfoVersion = "$Version.0.0"
$setupFileName = "XDTBox_Lizenzmanager_Setup_$Version.exe"

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
        throw "Sicherheitsabbruch: Build-Artefaktpfad liegt außerhalb des Repositorys: $resolvedPath"
    }
}

function Reset-BuildArtifactDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)

    Assert-PathInsideRepository -Path $Path -RepositoryRoot $repoRoot
    if (Test-Path -LiteralPath $Path) {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Replace-BuildArtifactDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$DestinationPath
    )

    Assert-PathInsideRepository -Path $SourcePath -RepositoryRoot $repoRoot
    Assert-PathInsideRepository -Path $DestinationPath -RepositoryRoot $repoRoot

    if (-not (Test-Path -LiteralPath $SourcePath)) {
        throw "Staging-Artefaktordner wurde nicht gefunden: $SourcePath"
    }

    $destinationParent = Split-Path -Parent $DestinationPath
    New-Item -ItemType Directory -Force -Path $destinationParent | Out-Null

    $backupPath = "$DestinationPath.previous-$([DateTime]::UtcNow.ToString('yyyyMMddHHmmssfff'))"
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
    param([Parameter(Mandatory = $true)][IO.FileInfo]$File)

    $textExtensions = @(".json", ".config", ".xml", ".txt", ".md", ".ini", ".log", ".ps1", ".cmd", ".bat", ".xaml")
    foreach ($extension in $textExtensions) {
        if ([string]::Equals($extension, $File.Extension, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Assert-LicenseManagerPublishIsClean {
    param([Parameter(Mandatory = $true)][string]$Path)

    $forbiddenNamePatterns = @(
        "XdtDeviceBridge.App.exe",
        "XdtDeviceBridge.App.dll",
        "XdtBox.LicenseIssuer.exe",
        "*.Tests.*",
        "*.pem",
        "*.key",
        "*.xdtboxlic",
        "*.xdtbox-licensemanager-backup",
        "license-manager-customers.json",
        "license-history.json",
        "license-manager-settings.json",
        "license-customer.json",
        "license-customer-data.json",
        "xdtbox_private*",
        "*private*key*"
    )

    $violations = New-Object System.Collections.Generic.List[string]
    foreach ($pattern in $forbiddenNamePatterns) {
        foreach ($item in Get-ChildItem -Path $Path -Recurse -Force -Filter $pattern -ErrorAction SilentlyContinue) {
            $violations.Add($item.FullName)
        }
    }

    $forbiddenTextTokens = @(
        "BEGIN PRIVATE KEY",
        "license-manager-customers.json",
        "license-history.json",
        "license-manager-settings.json",
        "C:\Users\MarcK",
        "C:\XDTBox\RT3100RS232"
    )

    foreach ($file in Get-ChildItem -Path $Path -Recurse -Force -File -ErrorAction SilentlyContinue) {
        if (-not (Test-IsTextFileForPublishValidation -File $file)) {
            continue
        }

        $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction SilentlyContinue
        foreach ($token in $forbiddenTextTokens) {
            if ($content -and $content.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
                $violations.Add("$($file.FullName) enthält '$token'")
            }
        }
    }

    if ($violations.Count -gt 0) {
        throw "LicenseManager-Publish enthält verbotene Schlüssel-/Kunden-/Backupdaten:$([Environment]::NewLine)$($violations -join [Environment]::NewLine)"
    }
}

function Remove-LicenseIssuerExecutableFromPublish {
    param([Parameter(Mandatory = $true)][string]$Path)

    # XdtBox.LicenseManager reuses issuer classes through the project reference. The DLL is required,
    # but the separate command-line issuer executable must not be shipped in the LicenseManager setup.
    $issuerExe = Join-Path $Path "XdtBox.LicenseIssuer.exe"
    if (Test-Path -LiteralPath $issuerExe) {
        Remove-Item -LiteralPath $issuerExe -Force
        Write-Host "Nicht benoetigte LicenseIssuer-Exe aus LicenseManager-Publish entfernt: $issuerExe"
    }
}

function Assert-LicenseManagerStartsFromPublish {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ($SkipStartValidation) {
        Write-Host "App-Start-Validierung wurde durch -SkipStartValidation übersprungen."
        return
    }

    $exe = Join-Path $Path "XdtBox.LicenseManager.exe"
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "LicenseManager-Exe wurde im Publish nicht gefunden: $exe"
    }

    $process = Start-Process -FilePath $exe -WorkingDirectory $Path -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 2
    if ($process.HasExited -and $process.ExitCode -ne 0) {
        throw "XDTBox Lizenzmanager startete aus dem Publish nicht erfolgreich. ExitCode: $($process.ExitCode)"
    }

    if (-not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Wait-Process -Id $process.Id -Timeout 5 -ErrorAction SilentlyContinue
    }

    Write-Host "App-Start aus Publish erfolgreich geprüft: $exe"
}

try {
    Reset-BuildArtifactDirectory -Path $stagingRoot
    New-Item -ItemType Directory -Force -Path $stagedPublishDir | Out-Null
    New-Item -ItemType Directory -Force -Path $stagedInstallerDir | Out-Null

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
        throw "dotnet publish ist fehlgeschlagen. Finale LicenseManager-Artefakte wurden nicht ersetzt."
    }

    Remove-LicenseIssuerExecutableFromPublish -Path $stagedPublishDir
    Assert-LicenseManagerPublishIsClean -Path $stagedPublishDir
    Assert-LicenseManagerStartsFromPublish -Path $stagedPublishDir

    if ($SkipInstaller) {
        Replace-BuildArtifactDirectory -SourcePath $stagedPublishDir -DestinationPath $publishDir
        Write-Host "Publish fertig: $publishDir"
        Write-Host "Installer-Build wurde durch -SkipInstaller übersprungen."
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
        throw "Inno Setup 6 Compiler wurde nicht gefunden. Bitte Inno Setup 6 installieren oder ISCC_EXE auf ISCC.exe setzen. Validierte Publish-Ausgabe liegt vorläufig unter: $stagedPublishDir"
    }

    & $iscc "/DMyPublishDir=$stagedPublishDir" "/DMyInstallerOutputDir=$stagedInstallerDir" "/DMyAppVersion=$Version" "/DMyVersionInfoVersion=$versionInfoVersion" $innoScript

    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup Build ist fehlgeschlagen. Finale LicenseManager-Artefakte wurden nicht ersetzt."
    }

    $stagedSetupFile = Join-Path $stagedInstallerDir $setupFileName
    if (-not (Test-Path -LiteralPath $stagedSetupFile)) {
        throw "Installer wurde nicht gefunden: $stagedSetupFile"
    }

    Replace-BuildArtifactDirectory -SourcePath $stagedPublishDir -DestinationPath $publishDir
    Replace-BuildArtifactDirectory -SourcePath $stagedInstallerDir -DestinationPath $installerDir

    $setupFile = Join-Path $installerDir $setupFileName
    $setupInfo = Get-Item -LiteralPath $setupFile
    Write-Host "Publish fertig: $publishDir"
    Write-Host "Installer fertig: $setupFile"
    Write-Host "Installer Größe: $($setupInfo.Length) Bytes"
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force
    }
}
