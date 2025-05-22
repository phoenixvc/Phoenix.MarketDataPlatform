#!/usr/bin/env node
// Script to run tests after commit
const { execSync } = require("child_process");

try {
  // Run the tests
  console.log("Running tests on the solution...");
  execSync("dotnet test Phoenix.MarketDataPlatform.sln", { stdio: "inherit" });
  console.log("Tests completed successfully");
} catch (error) {
  console.error("Tests failed");
  // We don't exit with error since this is post-commit
  // and can't prevent the commit anymore
}
