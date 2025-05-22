#!/usr/bin/env node
// Script to run tests after commit and generate coverage reports
const { execSync, spawnSync } = require("child_process");
const { execSync } = require("child_process");
const path = require("path");
const fs = require("fs");

try {
  // Run the tests
  console.log("Running tests on the solution...");
  execSync("dotnet test Phoenix.MarketDataPlatform.sln", { stdio: "inherit" });
  console.log("Tests completed successfully");

  // Generate the HTML coverage report
  console.log("Generating HTML coverage report...");

  try {
    // Check if reportgenerator is already installed
    try {
      execSync("reportgenerator -version", { stdio: "pipe" });
      console.log("ReportGenerator is already installed");
    } catch (error) {
      console.log("Installing ReportGenerator tool...");
      execSync("dotnet tool install -g dotnet-reportgenerator-globaltool", {
        stdio: "inherit",
        shell: true,
      });
    }

    // Generate the report using coverage files from ALL test projects
    // Use a central location that's not tied to a specific test project
    const reportDir = path.join("coverage-report");
    
    // Clear existing coverage reports
    if (fs.existsSync(reportDir)) {
      console.log("Clearing existing coverage reports...");
      fs.rmSync(reportDir, { recursive: true, force: true });
    }

    // Create fresh directory
    console.log("Creating new coverage report directory...");
    fs.mkdirSync(reportDir, { recursive: true });

    // Create a temporary settings file to completely disable source control
    const settingsPath = path.join("coverage-settings.xml");
    fs.writeFileSync(
      settingsPath,
      `<?xml version="1.0" encoding="utf-8"?>
      <ReportGeneratorSettings>
        <Settings>
          <UseSourceLink>False</UseSourceLink>
          <UseHistoricCoverageFile>False</UseHistoricCoverageFile>
          <DisableSourceControlIntegration>True</DisableSourceControlIntegration>
        </Settings>
      </ReportGeneratorSettings>`,
    );

    // Execute reportgenerator with extreme settings to disable GitHub lookups
    // CRITICAL FIX: Process the coverage files to remove GitHub URLs
    console.log("Pre-processing coverage files to remove GitHub URLs...");
    processAllCoverageFiles();

    // Now run ReportGenerator on the processed files
    execSync(
      `reportgenerator ` +
        `"-reports:tests/**/TestResults/**/coverage.cobertura.xml" ` +
      `"-reports:tests/**/TestResults/**/processed.coverage.cobertura.xml" ` +
        `"-targetdir:${reportDir}" ` +
        `"-reporttypes:Html" ` +
        `"-sourcedirs:${process.cwd()}" ` +
        `"-verbosity:Error" ` +
        `"-settings:${settingsPath}" ` +
        `"-tag:noupdate"`,
      `"-verbosity:Warning"`,
      {
        stdio: "inherit",
        shell: true,
      },
      }
    );

    // Clean up settings file
    fs.unlinkSync(settingsPath);

    // Open the report in browser (Windows-specific)
    console.log(`Coverage report generated at: ${reportDir}`);
    console.log(`Opening report in browser...`);

    execSync(`start "${reportDir}\\index.html"`, {
      stdio: "inherit",
      shell: true,
    });
  } catch (reportError) {
    console.error("Error generating coverage reports:", reportError.message);
  }
} catch (error) {
  console.error("Tests failed");
}

// Helper function to process all coverage files and remove GitHub URLs
function processAllCoverageFiles() {
  // Find all coverage files
  const findCoverageCmd = `dir /s /b tests\\*coverage.cobertura.xml`;
  const coverageFiles = execSync(findCoverageCmd, { encoding: 'utf8', shell: true })
    .split('\r\n')
    .filter(file => file.trim() !== '');
  
  console.log(`Found ${coverageFiles.length} coverage files to process`);
  
  // Process each file
  coverageFiles.forEach(filePath => {
    if (!filePath || !fs.existsSync(filePath)) return;
    
    // Read the coverage file
    const content = fs.readFileSync(filePath, 'utf8');
    
    // Replace any GitHub URLs with local paths