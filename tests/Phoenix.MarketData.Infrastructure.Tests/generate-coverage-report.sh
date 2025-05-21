#!/bin/bash

# Ensure ReportGenerator tool is installed
if ! command -v reportgenerator &> /dev/null; then
    echo "Installing ReportGenerator tool..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Set working directory to the project directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

# Run the tests with coverage
echo "Running tests with coverage collection..."
dotnet test .

# Generate HTML report
echo "Generating HTML coverage report..."
TEST_RESULTS_DIR="$SCRIPT_DIR/TestResults"
REPORT_DIR="$SCRIPT_DIR/CoverageReport"

# Create the report directory if it doesn't exist
mkdir -p "$REPORT_DIR"

# Use wildcard pattern for coverage files
echo "Searching for coverage files..."
COVERAGE_PATTERN="$TEST_RESULTS_DIR/**/coverage.cobertura.xml"

# Generate the report using the wildcard pattern
reportgenerator "-reports:$COVERAGE_PATTERN" "-targetdir:$REPORT_DIR" -reporttypes:Html

# Open the report in the default browser
INDEX_PATH="$REPORT_DIR/index.html"
if [ -f "$INDEX_PATH" ]; then
    echo "Opening coverage report in browser..."
    if [[ "$OSTYPE" == "darwin"* ]]; then
        open "$INDEX_PATH"
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        if command -v xdg-open &> /dev/null; then
            xdg-open "$INDEX_PATH"
        else
            echo "Please open the report manually: $INDEX_PATH"
        fi
    else
        echo "Please open the report manually: $INDEX_PATH"
    fi
else
    echo "Error: Report was not generated correctly."
fi

echo "Coverage report generated at: $REPORT_DIR"