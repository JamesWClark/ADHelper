@echo off

REM Set the paths
set EXE_PATH=%~dp0..\bin\publish\ADHelper.exe
set CONFIG_PATH=%~dp0config.xml
set CSV_PATH=%~dp0users.csv

REM Run the ADHelper executable with the specified config, CSV files, and task
"%EXE_PATH%" -config "%CONFIG_PATH%" -csv "%CSV_PATH%" -task create_users

REM Pause to keep the command prompt open
pause