#!/usr/bin/env pwsh
param(
    [switch]$RunTestsOnly = $true
)

# Set working directory to the project directory
$scriptPath = $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptPath

# Run the tests with coverage if requested
if ($RunTestsOnly) {
    Write-Host "Running tests with coverage collection..."
    dotnet test $projectDir
    $testExitCode = $LASTEXITCODE

    # Check if tests succeeded before continuing
    if ($testExitCode -ne 0) {
        Write-Error "Tests failed with exit code $testExitCode. Report generation aborted."
        exit $testExitCode
    }
}
else {
    Write-Host "Skipping tests (already run in post-commit hook)..."
}

# Generate HTML report
Write-Host "Generating HTML coverage report..."
$testResultsDir = Join-Path $projectDir "TestResults"
$reportDir = Join-Path $projectDir "CoverageReport"

# Check if test results directory exists and contains coverage files
if (-not (Test-Path $testResultsDir)) {
    Write-Error "Test results directory not found: $testResultsDir"
    Write-Error "Tests may have run without generating coverage data."
    exit 1
}

# Create the report directory if it doesn't exist
if (-not (Test-Path $reportDir)) {
    New-Item -Path $reportDir -ItemType Directory -Force | Out-Null
}

# Find the coverage files using a wildcard pattern
$coverageFilesPattern = Join-Path $testResultsDir "**\coverage.cobertura.xml"
$coverageFiles = Get-ChildItem -Path $coverageFilesPattern -Recurse -ErrorAction SilentlyContinue

if (-not $coverageFiles -or $coverageFiles.Count -eq 0) {
    Write-Error "No coverage files found with pattern: $coverageFilesPattern"
    exit 1
}

Write-Host "Found $($coverageFiles.Count) coverage files"

# Generate the report
reportgenerator "-reports:$coverageFilesPattern" "-targetdir:$reportDir" -reporttypes:Html
$reportExitCode = $LASTEXITCODE

if ($reportExitCode -ne 0) {
    Write-Error "Report generation failed with exit code $reportExitCode"
    exit $reportExitCode
}

# Open the report in the default browser
$indexPath = Join-Path $reportDir "index.html"
if (Test-Path $indexPath) {
    Write-Host "Opening coverage report in browser..."
    Start-Process $indexPath
}
else {
    Write-Error "Report was not generated correctly."
    exit 1
}

Write-Host "Coverage report generated at: $reportDir"
