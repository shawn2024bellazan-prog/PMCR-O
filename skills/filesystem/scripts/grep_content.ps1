# grep_content.ps1
# Tool: grep_content
# Usage: .\scripts\grep_content.ps1 -root_path "A:\source\ProjectName" -pattern "TrailFrame" [-file_pattern "*.cs"] [-sub_path "src"]
# Artifacts written to .pmcro/artifacts/grep_content-<timestamp>.json

param(
    [Parameter(Mandatory = $true)]
    [string]$root_path,

    [Parameter(Mandatory = $true)]
    [string]$pattern,

    [string]$file_pattern   = "*",
    [string]$sub_path       = "",
    [bool]  $case_sensitive = $false,
    [int]   $max_results    = 200
)

$ErrorActionPreference = "Stop"

$_scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
. (Join-Path $_scriptDir "_artifact_writer.ps1")

if (-not (Test-Path $root_path)) {
    Write-Error "root_path does not exist: $root_path"
    exit 1
}

$scanRoot = if ($sub_path -ne "") { Join-Path $root_path $sub_path } else { $root_path }

if (-not (Test-Path $scanRoot)) {
    Write-Error "sub_path does not exist: $scanRoot"
    exit 1
}

$excludedDirs = @(
    "bin", "obj", ".git", ".vs", ".idea",
    "node_modules", "dist", "build", "artifacts", ".pmcro"
)

function Is-ExcludedPath {
    param([string]$fullPath)
    $parts = $fullPath.Split([IO.Path]::DirectorySeparatorChar)
    foreach ($part in $parts) {
        if ($excludedDirs -contains $part) { return $true }
    }
    return $false
}

$matchList  = [System.Collections.Generic.List[object]]::new()
$truncated  = $false

$regexOpts = if ($case_sensitive) {
    [System.Text.RegularExpressions.RegexOptions]::None
} else {
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
}

$files = Get-ChildItem -Path $scanRoot -Recurse -File -Filter $file_pattern |
    Where-Object { -not (Is-ExcludedPath $_.FullName) }

:fileloop foreach ($file in $files) {
    $relPath = [IO.Path]::GetRelativePath($root_path, $file.FullName)
    try {
        $lines = Get-Content $file.FullName -Encoding UTF8 -ErrorAction SilentlyContinue
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ([System.Text.RegularExpressions.Regex]::IsMatch($lines[$i], $pattern, $regexOpts)) {
                $matchList.Add([PSCustomObject]@{
                    file        = $relPath
                    line_number = $i + 1
                    line        = $lines[$i].Trim()
                }) | Out-Null
                if ($matchList.Count -ge $max_results) {
                    $truncated = $true
                    break fileloop
                }
            }
        }
    } catch {
        # Skip unreadable files silently
    }
}

$result = [PSCustomObject]@{
    root         = $root_path
    pattern      = $pattern
    file_pattern = $file_pattern
    sub_path     = if ($sub_path -ne "") { $sub_path } else { $null }
    match_count  = $matchList.Count
    truncated    = $truncated
    matches      = $matchList.ToArray()
}

Write-PmcroArtifact -ToolName "grep_content" -Data $result | Out-Null
$result | ConvertTo-Json -Depth 10
