@echo off
echo Running post-commit tests...
npm run postcommit || exit /b 0
exit /b 0