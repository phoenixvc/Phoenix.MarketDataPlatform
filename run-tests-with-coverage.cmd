@echo off
dotnet test --settings tests/Phoenix.MarketData.Infrastructure.Tests/coverage.runsettings
cd tests/Phoenix.MarketData.Infrastructure.Tests
powershell -ExecutionPolicy Bypass -File .\Generate-CoverageReport.ps1