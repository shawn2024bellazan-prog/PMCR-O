# dump_project_source.ps1
# Tool: dump_project_source
# Usage: .\scripts\dump_project_source.ps1 -root_path "A:\source\ProjectName" [-sub_path "src"] [-max_file_kb 500]
# Artifacts written to .pmcro/artifacts/dump_project_source-<timestamp>.json

param(
    [Parameter(Mandatory = $true)]
    [string]$root_path,

    [string]$sub_path = "",

    [int]$max_file_kb = 500
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

$maxBytes    = $max_file_kb * 1024
$skipped     = 0

$files = @(Get-ChildItem -Path $scanRoot -Recurse -File |
    Where-Object {
        $allowed = $allowedExtensions -contains $_.Extension.ToLower()
        $notExcluded = -not (Is-ExcludedPath $_.FullName)
        $withinSize = $_.Length -le $maxBytes
        if ($allowed -and $notExcluded -and -not $withinSize) { $script:skipped++ }
        $allowed -and $notExcluded -and $withinSize
    } |
    Sort-Object FullName |
    ForEach-Object {
        $rel     = [IO.Path]::GetRelativePath($root_path, $_.FullName)
        $content = Get-Content $_.FullName -Raw -Encoding UTF8 -ErrorAction SilentlyContinue
        [PSCustomObject]@{
            relative_path = $rel
            size_bytes    = $_.Length
            last_modified = $_.LastWriteTimeUtc.ToString("o")
            content       = $content
        }
    })

$result = [PSCustomObject]@{
    root          = $root_path
    sub_path      = if ($sub_path -ne "") { $sub_path } else { $null }
    file_count    = $files.Count
    skipped_count = $skipped
    files         = $files
}

Write-PmcroArtifact -ToolName "dump_project_source" -Data $result | Out-Null
$result | ConvertTo-Json -Depth 20
