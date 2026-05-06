# list_tree.ps1
# Tool: list_tree
# Usage: .\scripts\list_tree.ps1 -root_path "A:\source\ProjectName" [-max_depth 3]
# Artifacts written to .pmcro/artifacts/list_tree-<timestamp>.json

param(
    [Parameter(Mandatory = $true)]
    [string]$root_path,

    [int]$max_depth = 0   # 0 = unlimited
)

$ErrorActionPreference = "Stop"

$_scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
. (Join-Path $_scriptDir "_artifact_writer.ps1")

if (-not (Test-Path $root_path)) {
    Write-Error "root_path does not exist: $root_path"
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

$tree = Get-ChildItem -Path $root_path -Recurse |
    Where-Object {
        $rel   = [IO.Path]::GetRelativePath($root_path, $_.FullName)
        $depth = ($rel.Split([IO.Path]::DirectorySeparatorChar)).Count
        $withinDepth = ($max_depth -eq 0) -or ($depth -le $max_depth)
        $withinDepth -and -not (Is-ExcludedPath $_.FullName)
    } |
    ForEach-Object {
        $rel    = [IO.Path]::GetRelativePath($root_path, $_.FullName)
        $prefix = if ($_.PSIsContainer) { "[DIR]  " } else { "[FILE] " }
        "$prefix$rel"
    }

$treeArr = @($tree)

$result = [PSCustomObject]@{
    root        = $root_path
    entry_count = $treeArr.Count
    max_depth   = if ($max_depth -eq 0) { $null } else { $max_depth }
    tree        = $treeArr
}

Write-PmcroArtifact -ToolName "list_tree" -Data $result | Out-Null
$result | ConvertTo-Json -Depth 10
