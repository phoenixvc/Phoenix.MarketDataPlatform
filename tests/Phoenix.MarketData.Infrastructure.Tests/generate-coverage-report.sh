#!/bin/bash

# Ensure ReportGenerator tool is installed
if ! command -v reportgenerator &> /dev/null; then
    echo "Installing ReportGenerator tool..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
    if [ $? -ne 0 ]; then
        echo "Error: Failed to install ReportGenerator tool"
        exit 1
    fi
fi

# Set working directory to the project directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

# Run the tests with coverage
echo "Running tests with coverage collection..."
dotnet test . --collect:"XPlat Code Coverage"
TEST_EXIT_CODE=$?
if [ $TEST_EXIT_CODE -ne 0 ]; then
    echo "Error: Tests failed with exit code $TEST_EXIT_CODE. Aborting coverage report generation."
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

# Find all coverage files
COVERAGE_FILES=( $(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" 2>/dev/null) )

if [ ${#COVERAGE_FILES[@]} -eq 0 ]; then
    echo "Error: No coverage files found at $TEST_RESULTS_DIR"
    exit 1
elif [ ${#COVERAGE_FILES[@]} -eq 1 ]; then
    LATEST_COVERAGE_FILE="${COVERAGE_FILES[0]}"
    echo "Using coverage file: $LATEST_COVERAGE_FILE"
    reportgenerator "-reports:$LATEST_COVERAGE_FILE" "-targetdir:$REPORT_DIR" -reporttypes:Html
    REPORT_EXIT_CODE=$?
else
    echo "Multiple coverage files found. Generating merged report."
    REPORTS_ARG="-reports:$(IFS=','; echo "${COVERAGE_FILES[*]}")"
    reportgenerator "$REPORTS_ARG" "-targetdir:$REPORT_DIR" -reporttypes:Html
    REPORT_EXIT_CODE=$?
fi

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