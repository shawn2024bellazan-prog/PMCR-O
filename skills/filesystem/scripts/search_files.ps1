# search_files.ps1
# Tool: search_files
# Usage: .\scripts\search_files.ps1 -root_path "A:\source\ProjectName" -pattern "*.cs" [-sub_path "src"]
# Artifacts written to .pmcro/artifacts/search_files-<timestamp>.json

param(
    [Parameter(Mandatory = $true)]
    [string]$root_path,

    [Parameter(Mandatory = $true)]
    [string]$pattern,

    [string]$sub_path = ""
)

$ErrorActionPreference = "Stop"

$_scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
. (Join-Path $_scriptDir "_artifact_writer.ps1")

if (-not (Test-Path $root_path)) {
    Write-Error "root_path does not exist: $root_path"
    exit 1
}

$scanRoot = if ($sub_path -ne "") {
    Join-Path $root_path $sub_path
} else {
    $root_path
}

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

$matched = @(Get-ChildItem -Path $scanRoot -Recurse -File -Filter $pattern |
    Where-Object { -not (Is-ExcludedPath $_.FullName) } |
    ForEach-Object {
        [IO.Path]::GetRelativePath($root_path, $_.FullName)
    } |
    Sort-Object)

$result = [PSCustomObject]@{
    root        = $root_path
    pattern     = $pattern
    sub_path    = if ($sub_path -ne "") { $sub_path } else { $null }
    match_count = $matched.Count
    matches     = $matched
}

Write-PmcroArtifact -ToolName "search_files" -Data $result | Out-Null
$result | ConvertTo-Json -Depth 10
