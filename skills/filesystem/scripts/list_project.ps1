# list_project.ps1
# Tool: list_project
# Usage: .\scripts\list_project.ps1 -root_path "A:\source\ProjectName" [-sub_path "src"]
# Artifacts written to .pmcro/artifacts/list_project-<timestamp>.json

param(
    [Parameter(Mandatory = $true)]
    [string]$root_path,

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

$allowedExtensions = @(
    ".cs", ".csproj", ".sln",
    ".json", ".yaml", ".yml",
    ".xml", ".proto", ".gradle",
    ".md", ".ps1", ".sh",
    ".txt", ".env", ".config",
    ".dockerfile", ".gitignore", ".editorconfig", ".razor", ".html", ".css", ".ts", ".js"
)

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

$files = @(Get-ChildItem -Path $scanRoot -Recurse -File |
    Where-Object {
        ($allowedExtensions -contains $_.Extension.ToLower()) -and
        (-not (Is-ExcludedPath $_.FullName))
    } |
    ForEach-Object {
        [IO.Path]::GetRelativePath($root_path, $_.FullName)
    } |
    Sort-Object)

$result = [PSCustomObject]@{
    root       = $root_path
    sub_path   = if ($sub_path -ne "") { $sub_path } else { $null }
    file_count = $files.Count
    files      = $files
}

Write-PmcroArtifact -ToolName "list_project" -Data $result | Out-Null
$result | ConvertTo-Json -Depth 10
