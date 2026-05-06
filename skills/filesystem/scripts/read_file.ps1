# read_file.ps1
# Tool: read_file
# Usage: .\scripts\read_file.ps1 -file_path "A:\source\ProjectName\src\Program.cs"
# Artifacts written to .pmcro/artifacts/read_file-<timestamp>.json

param(
    [Parameter(Mandatory = $true)]
    [string]$file_path
)

$ErrorActionPreference = "Stop"

$_scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
. (Join-Path $_scriptDir "_artifact_writer.ps1")

if (-not (Test-Path $file_path)) {
    Write-Error "file_path does not exist: $file_path"
    exit 1
}

$item    = Get-Item $file_path
$content = Get-Content $file_path -Raw -Encoding UTF8
$lines   = ($content -split "`n").Count

$result = [PSCustomObject]@{
    file_path     = $file_path
    file_name     = $item.Name
    extension     = $item.Extension
    size_bytes    = $item.Length
    last_modified = $item.LastWriteTimeUtc.ToString("o")
    line_count    = $lines
    content       = $content
}

Write-PmcroArtifact -ToolName "read_file" -Data $result | Out-Null
$result | ConvertTo-Json -Depth 10
