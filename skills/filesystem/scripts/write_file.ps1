# write_file.ps1
# Tool: write_file
# Usage: .\scripts\write_file.ps1 -file_path "A:\source\ProjectName\src\X.cs" -content "..." [-overwrite $true]
# Artifacts written to .pmcro/artifacts/write_file-<timestamp>.json
# WARNING: This is a write operation. Invoke only after Checker phase approval.

param(
    [Parameter(Mandatory = $true)]
    [string]$file_path,

    [Parameter(Mandatory = $true)]
    [string]$content,

    [bool]$overwrite = $true
)

$ErrorActionPreference = "Stop"

$_scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
. (Join-Path $_scriptDir "_artifact_writer.ps1")

$existed      = Test-Path $file_path
$createdDirs  = [System.Collections.Generic.List[string]]::new()

if ($existed -and -not $overwrite) {
    Write-Error "File already exists and overwrite is false: $file_path"
    exit 1
}

# Create parent directories if needed
$parentDir = Split-Path $file_path -Parent
if (-not (Test-Path $parentDir)) {
    $created = New-Item -ItemType Directory -Path $parentDir -Force
    # Collect all newly created dirs
    $dir = $parentDir
    while ($dir -and -not (Test-Path (Split-Path $dir -Parent) -PathType Container -ErrorAction SilentlyContinue) -eq $false) {
        break
    }
    $createdDirs.Add($parentDir) | Out-Null
}

# Write content (UTF-8 without BOM)
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($file_path, $content, $utf8NoBom)

$item = Get-Item $file_path

$result = [PSCustomObject]@{
    file_path    = $file_path
    size_bytes   = $item.Length
    written      = $true
    overwritten  = $existed -and $overwrite
    created_dirs = $createdDirs.ToArray()
}

Write-PmcroArtifact -ToolName "write_file" -Data $result | Out-Null
$result | ConvertTo-Json -Depth 10
