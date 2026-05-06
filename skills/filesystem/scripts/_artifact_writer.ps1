# _artifact_writer.ps1
# Shared helper — dot-source this in every script using:
#   $_scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
#   . (Join-Path $_scriptDir "_artifact_writer.ps1")
# DO NOT run this file directly — it only defines a function.

param()  # Empty param block — prevents PowerShell from prompting for input

function Write-PmcroArtifact {
    <#
    .SYNOPSIS
        Writes a structured JSON artifact to .pmcro/artifacts/ and returns the file path.
    .PARAMETER ToolName
        Name of the tool producing this artifact (used as the filename prefix).
    .PARAMETER Data
        The PSCustomObject or hashtable to serialize.
    .PARAMETER ArtifactRoot
        Override the artifacts directory (default: <cwd>/.pmcro/artifacts).
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$ToolName,

        [Parameter(Mandatory = $true)]
        [object]$Data,

        [string]$ArtifactRoot = ""
    )

    if (-not $ArtifactRoot) {
        $ArtifactRoot = Join-Path (Get-Location).Path ".pmcro\artifacts"
    }

    if (-not (Test-Path $ArtifactRoot)) {
        New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null
    }

    $timestamp  = (Get-Date).ToString("yyyyMMdd_HHmmss_fff")
    $fileName   = "$ToolName-$timestamp.json"
    $filePath   = Join-Path $ArtifactRoot $fileName

    $Data | ConvertTo-Json -Depth 20 | Out-File -FilePath $filePath -Encoding UTF8 -Force

    Write-Host "[pmcro] Artifact written: $filePath" -ForegroundColor DarkGray
    return $filePath
}