#Requires -Version 5.1
<#
.SYNOPSIS
    Runs PromptTests with code coverage and generates an HTML report.

.DESCRIPTION
    Collects coverage using coverlet, then uses ReportGenerator (installed as a
    .NET global tool if not already present) to produce an HTML report.

.PARAMETER Configuration
    Build configuration – Debug (default) or Release.

.PARAMETER ReportDir
    Directory where the HTML report is written.
    Defaults to tests/PromptTests/coverage-report.

.PARAMETER OpenReport
    When specified, opens the HTML report in the default browser after generation.

.EXAMPLE
    .\coverage.ps1
    .\coverage.ps1 -Configuration Release -OpenReport
#>
param(
    [string] $Configuration = 'Debug',
    [string] $ReportDir     = (Join-Path $PSScriptRoot 'tests\PromptTests\coverage-report'),
    [switch] $OpenReport
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root        = $PSScriptRoot
$testProject = Join-Path $root 'tests\PromptTests\PromptTests.csproj'
$coberturaDir = Join-Path $root 'tests\PromptTests\coverage'

# ── 1. Clean previous coverage artefacts ──────────────────────────────────────
if (Test-Path $coberturaDir) { Remove-Item $coberturaDir -Recurse -Force }
if (Test-Path $ReportDir)    { Remove-Item $ReportDir    -Recurse -Force }

New-Item -ItemType Directory -Force -Path $coberturaDir | Out-Null
New-Item -ItemType Directory -Force -Path $ReportDir    | Out-Null

# ── 2. Run tests with coverlet data collector ─────────────────────────────────
Write-Host ''
Write-Host '==> Running tests with coverage collection...' -ForegroundColor Cyan

dotnet test $testProject `
    --configuration $Configuration `
    --collect:"XPlat Code Coverage" `
    --results-directory $coberturaDir `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet test failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# ── 3. Locate the generated coverage.cobertura.xml ────────────────────────────
$coberturaFile = Get-ChildItem -Path $coberturaDir -Filter 'coverage.cobertura.xml' -Recurse |
                 Select-Object -First 1

if (-not $coberturaFile) {
    Write-Error 'Could not find coverage.cobertura.xml. Check that coverlet.collector is installed.'
    exit 1
}

Write-Host "==> Coverage data: $($coberturaFile.FullName)" -ForegroundColor Green

# ── 4. Ensure ReportGenerator is available ────────────────────────────────────
$rg = Get-Command 'reportgenerator' -ErrorAction SilentlyContinue
if (-not $rg) {
    Write-Host '==> Installing ReportGenerator global tool...' -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
    if ($LASTEXITCODE -ne 0) {
        Write-Error 'Failed to install ReportGenerator.'
        exit $LASTEXITCODE
    }
    # Refresh PATH so the newly installed tool is found
    $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH', 'User') + ';' + $env:PATH
}

# ── 5. Generate HTML report ───────────────────────────────────────────────────
Write-Host ''
Write-Host '==> Generating HTML report...' -ForegroundColor Cyan

reportgenerator `
    "-reports:$($coberturaFile.FullName)" `
    "-targetdir:$ReportDir" `
    '-reporttypes:Html;TextSummary' `
    '-assemblyfilters:+interactiveCLI'

if ($LASTEXITCODE -ne 0) {
    Write-Error "ReportGenerator failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# ── 6. Print text summary ─────────────────────────────────────────────────────
$summary = Join-Path $ReportDir 'Summary.txt'
if (Test-Path $summary) {
    Write-Host ''
    Write-Host '==> Coverage summary:' -ForegroundColor Cyan
    Get-Content $summary | Write-Host
}

Write-Host ''
Write-Host "==> HTML report written to: $ReportDir" -ForegroundColor Green

# ── 7. Optionally open the report ─────────────────────────────────────────────
if ($OpenReport) {
    $index = Join-Path $ReportDir 'index.html'
    if (Test-Path $index) {
        Start-Process $index
    }
}
