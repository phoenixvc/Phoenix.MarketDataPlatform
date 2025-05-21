#!/usr/bin/env pwsh

# Ensure ReportGenerator tool is installed
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "Installing ReportGenerator tool..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Set working directory to the project directory
$scriptPath = $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptPath

# Run the tests with coverage
Write-Host "Running tests with coverage collection..."
dotnet test $projectDir

# Generate HTML report
Write-Host "Generating HTML coverage report..."
$testResultsDir = Join-Path $projectDir "TestResults"
$reportDir = Join-Path $projectDir "CoverageReport"

# Create the report directory if it doesn't exist
if (-not (Test-Path $reportDir)) {
    New-Item -Path $reportDir -ItemType Directory -Force | Out-Null
}

# Find the coverage files and use a proper wildcard pattern
$coverageFilesPattern = Join-Path $testResultsDir "**\coverage.cobertura.xml"
Write-Host "Searching for coverage files with pattern: $coverageFilesPattern"

# Generate the report using a wildcard pattern instead of specific files
reportgenerator "-reports:$coverageFilesPattern" "-targetdir:$reportDir" -reporttypes:Html

# Open the report in the default browser
$indexPath = Join-Path $reportDir "index.html"
if (Test-Path $indexPath) {
    Write-Host "Opening coverage report in browser..."
    Start-Process $indexPath
} else {
    Write-Error "Report was not generated correctly."
}

Write-Host "Coverage report generated at: $reportDir"