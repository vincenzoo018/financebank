@echo off
echo Fixing directory structure and permissions...

rem Create any missing directories
if not exist "Components\Shared" mkdir "Components\Shared"
if not exist "Models" mkdir "Models" 
if not exist "Data" mkdir "Data"
if not exist "Services" mkdir "Services"
if not exist "Database" mkdir "Database"

rem Reset permissions
icacls "Components\Shared" /reset /T /Q
icacls "Models" /reset /T /Q
icacls "Data" /reset /T /Q
icacls "Services" /reset /T /Q
icacls "Database" /reset /T /Q

echo Done!
pause
