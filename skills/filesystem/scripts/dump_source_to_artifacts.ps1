# dump_source_to_artifacts.ps1
# Tool: dump_source_to_artifacts
# Usage: .\scripts\dump_source_to_artifacts.ps1 -root_path "C:\Users\org.tooensure\Documents\Tooensure\Projects\ProjectName" -output_name "Copilot_Context"
# Result: Creates a folder with individual .txt files for each source file.

param(
    [Parameter(Mandatory = $true)]
    [string]$root_path,

    [string]$sub_path = "",

    [string]$output_name = "Project_Artifacts",

    [int]$max_file_kb = 500
)

$ErrorActionPreference = "Stop"

# 1. Setup Directories
if (-not (Test-Path $root_path)) {
    Write-Error "root_path does not exist: $root_path"
    exit 1
}

$scanRoot = if ($sub_path -ne "") { Join-Path $root_path $sub_path } else { $root_path }
$exportRoot = Join-Path $root_path ".pmcro\artifacts\$output_name"

if (Test-Path $exportRoot) {
    Remove-Item -Path $exportRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $exportRoot -Force | Out-Null

# 2. Configuration
$allowedExtensions = @(
    ".cs", ".csproj", ".sln", ".json", ".yaml", ".yml",
    ".xml", ".proto", ".md", ".ps1", ".sh", ".txt", 
    ".gitignore", ".editorconfig", ".razor", ".html", ".css", ".ts", ".js"
)

$excludedDirs = @("bin", "obj", ".git", ".vs", ".idea", "node_modules", "dist", "build")

function Is-ExcludedPath {
    param([string]$fullPath)
    $parts = $fullPath.Split([IO.Path]::DirectorySeparatorChar)
    foreach ($part in $parts) {
        if ($excludedDirs -contains $part) { return $true }
    }
    return $false
}

# 3. Processing
$maxBytes = $max_file_kb * 1024
$filesProcessed = 0

Write-Host "Exporting source to individual artifacts in: $exportRoot" -ForegroundColor Cyan

Get-ChildItem -Path $scanRoot -Recurse -File | ForEach-Object {
    $file = $_
    $isAllowed = $allowedExtensions -contains $file.Extension.ToLower()
    $isNotExcluded = -not (Is-ExcludedPath $file.FullName)
    $withinSize = $file.Length -le $maxBytes

    if ($isAllowed -and $isNotExcluded -and $withinSize) {
        # Calculate Relative Path and safe output name
        $relPath = [IO.Path]::GetRelativePath($root_path, $file.FullName)
        
        # Flatten path into filename (e.g. src\App\Program.cs -> src_App_Program.cs.txt)
        # This prevents deep nested folders in your artifact export while keeping context
        $safeFileName = $relPath.Replace("\", "_").Replace("/", "_") + ".txt"
        $targetPath = Join-Path $exportRoot $safeFileName

        # Read content and add a Header for the AI
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $header = @"
// ═══════════════════════════════════════════════════════════════
// ARTIFACT IDENTITY: $relPath
// SOURCE TYPE     : $($file.Extension)
// MODIFIED        : $($file.LastWriteTimeUtc.ToString("o"))
// ═══════════════════════════════════════════════════════════════

$content
"@
        $header | Out-File -FilePath $targetPath -Encoding UTF8 -Force
        
        $filesProcessed++
        Write-Host " [✓] $relPath" -ForegroundColor Gray
    }
}

Write-Host "`nDone! Processed $filesProcessed files." -ForegroundColor Green
Write-Host "Upload the contents of $exportRoot to your Copilot Project." -ForegroundColor White