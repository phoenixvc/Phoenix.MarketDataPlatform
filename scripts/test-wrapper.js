#!/usr/bin/env node
// Script to run tests after commit and generate coverage reports
const { execSync, spawnSync } = require("child_process");
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

    // Generate the report using the most recent coverage file
    const reportDir = path.join(
      "tests",
      "Phoenix.MarketData.Infrastructure.Tests",
      "CoverageReport",
    );

    // Clear existing coverage reports
    if (fs.existsSync(reportDir)) {
      console.log("Clearing existing coverage reports...");
      fs.rmSync(reportDir, { recursive: true, force: true });
    }

    // Create fresh directory
    console.log("Creating new coverage report directory...");
    fs.mkdirSync(reportDir, { recursive: true });

    execSync(
      `reportgenerator "-reports:tests/**/TestResults/**/coverage.cobertura.xml" "-targetdir:${reportDir}" "-reporttypes:Html"`,
      {
        stdio: "inherit",
        shell: true,
      },
    );

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
