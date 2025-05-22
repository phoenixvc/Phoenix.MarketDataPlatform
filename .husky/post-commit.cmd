@echo off
echo Running post-commit tests...
npm run postcommit || exit /b 0

echo Generating HTML coverage report...
powershell -ExecutionPolicy Bypass -File tests/Phoenix.MarketData.Infrastructure.Tests/Generate-CoverageReport.ps1 -RunTestsOnly:$false || exit /b 0

echo Post-commit process completed
exit /b 0