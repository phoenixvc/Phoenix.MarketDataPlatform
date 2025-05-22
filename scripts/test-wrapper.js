#!/usr/bin/env node
// Script to run tests after commit and generate coverage reports
const { execSync } = require("child_process");
const path = require("path");

try {
  // Run the tests
  console.log("Running tests on the solution...");
  execSync("dotnet test Phoenix.MarketDataPlatform.sln", { stdio: "inherit" });
  console.log("Tests completed successfully");

  // Generate the HTML coverage report
  console.log("Generating HTML coverage report...");

  try {
    // Install reportgenerator if needed (will skip if already installed)
    execSync(
      "dotnet tool install -g dotnet-reportgenerator-globaltool --skip-duplicate",
      {
        stdio: "inherit",
        shell: true,
      },
    );

    // Generate the report using the most recent coverage file
    const reportDir = path.join(
      "tests",
      "Phoenix.MarketData.Infrastructure.Tests",
      "CoverageReport",
    );

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
