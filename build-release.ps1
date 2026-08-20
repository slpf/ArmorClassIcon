[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectPath = Join-Path $PSScriptRoot "src\ArmorClassIcon.csproj"
$modInfoPath = Join-Path $PSScriptRoot "src\ModInfo.cs"
$bepInExPath = Join-Path $PSScriptRoot "build\BepInEx"
$pluginPath = Join-Path $bepInExPath "plugins\ArmorClassIcon.dll"
$distributionPath = Join-Path $PSScriptRoot "distrib"

if (!(Test-Path -LiteralPath $projectPath -PathType Leaf))
{
    throw "Project file was not found: $projectPath"
}

if (!(Test-Path -LiteralPath $modInfoPath -PathType Leaf))
{
    throw "ModInfo file was not found: $modInfoPath"
}

$modInfo = Get-Content -LiteralPath $modInfoPath -Raw
$versionMatch = [regex]::Match(
    $modInfo,
    'public\s+const\s+string\s+Version\s*=\s*"(?<version>[^"]+)"\s*;')

if (!$versionMatch.Success)
{
    throw "Unable to read the mod version from $modInfoPath"
}

$version = $versionMatch.Groups["version"].Value.Trim()

if ([string]::IsNullOrWhiteSpace($version) -or
    $version.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0)
{
    throw "Invalid mod version: '$version'"
}

Get-Command dotnet -ErrorAction Stop | Out-Null
& dotnet build $projectPath --configuration Release

if ($LASTEXITCODE -ne 0)
{
    throw "Release build failed with exit code $LASTEXITCODE"
}

if (!(Test-Path -LiteralPath $pluginPath -PathType Leaf))
{
    throw "Build output was not found: $pluginPath"
}

New-Item -ItemType Directory -Path $distributionPath -Force | Out-Null
$archivePath = Join-Path $distributionPath "ArmorClassIcon-$version.zip"
$archiveBasePath = Split-Path -Parent $bepInExPath
$archiveBasePrefix = [System.IO.Path]::GetFullPath($archiveBasePath).TrimEnd('\', '/') +
    [System.IO.Path]::DirectorySeparatorChar
[System.IO.FileInfo[]]$files = @(Get-Item -LiteralPath $pluginPath)

if ($files.Count -eq 0)
{
    throw "Build output contains no files: $bepInExPath"
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$archiveStream = [System.IO.File]::Open(
    $archivePath,
    [System.IO.FileMode]::Create,
    [System.IO.FileAccess]::Write,
    [System.IO.FileShare]::None)
$archive = $null

try
{
    $archive = [System.IO.Compression.ZipArchive]::new(
        $archiveStream,
        [System.IO.Compression.ZipArchiveMode]::Create,
        $false)

    foreach ($file in $files)
    {
        [string]$fullPath = [System.IO.Path]::GetFullPath($file.FullName)
        [string]$entryPath = $fullPath.Substring($archiveBasePrefix.Length).Replace('\', '/')

        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive,
            $fullPath,
            $entryPath,
            [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally
{
    if ($null -ne $archive)
    {
        $archive.Dispose()
    }

    $archiveStream.Dispose()
}

if (!(Test-Path -LiteralPath $archivePath -PathType Leaf))
{
    throw "Archive was not created: $archivePath"
}

Write-Host "Created $archivePath"
