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
TEST_EXIT_CODE=$?

# Check if tests ran successfully
if [ $TEST_EXIT_CODE -ne 0 ]; then
    echo "Error: Tests failed with exit code $TEST_EXIT_CODE"
    echo "Coverage report will not be generated."
    exit $TEST_EXIT_CODE
fi

# Generate HTML report
echo "Generating HTML coverage report..."
TEST_RESULTS_DIR="$SCRIPT_DIR/TestResults"
REPORT_DIR="$SCRIPT_DIR/CoverageReport"

# Check if test results directory exists
if [ ! -d "$TEST_RESULTS_DIR" ]; then
    echo "Error: Test results directory not found: $TEST_RESULTS_DIR"
    echo "Tests may have run without generating coverage data."
    exit 1
fi

# Create the report directory if it doesn't exist
mkdir -p "$REPORT_DIR"

# Find the most recent coverage file
echo "Searching for coverage files..."
LATEST_COVERAGE_FILE=""
LATEST_TIMESTAMP=0

# Find the most recent coverage file
for file in $(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" 2>/dev/null); do
    if [ -f "$file" ]; then
        FILE_TIMESTAMP=$(stat -c %Y "$file" 2>/dev/null || stat -f %m "$file" 2>/dev/null)
        if [ -n "$FILE_TIMESTAMP" ] && [ "$FILE_TIMESTAMP" -gt "$LATEST_TIMESTAMP" ]; then
            LATEST_TIMESTAMP=$FILE_TIMESTAMP
            LATEST_COVERAGE_FILE=$file
        fi
    fi
done

# Check if any coverage files were found
if [ -z "$LATEST_COVERAGE_FILE" ]; then
    echo "Error: No coverage files found at $TEST_RESULTS_DIR"
    exit 1
fi

echo "Using coverage file: $LATEST_COVERAGE_FILE"

# Generate the report using the specific file
reportgenerator "-reports:$LATEST_COVERAGE_FILE" "-targetdir:$REPORT_DIR" -reporttypes:Html
REPORT_EXIT_CODE=$?

# Check if report generation was successful
if [ $REPORT_EXIT_CODE -ne 0 ]; then
    echo "Error: Report generation failed with exit code $REPORT_EXIT_CODE"
    exit $REPORT_EXIT_CODE
fi

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
    exit 1
fi

echo "Coverage report generated at: $REPORT_DIR"