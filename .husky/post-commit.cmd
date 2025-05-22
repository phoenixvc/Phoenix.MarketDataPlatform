@echo off
echo Running post-commit tests...
npm run postcommit || exit /b 0

echo Generating coverage reports...
dotnet tool install -g dotnet-reportgenerator-globaltool --skip-duplicate
reportgenerator "-reports:tests/**/TestResults/**/coverage.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:Html" || exit /b 0

echo Coverage report generated in coverage-report directory
exit /b 0