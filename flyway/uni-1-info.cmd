@echo off
setlocal
cd /d "%~dp0"
set "LOG=%~dp0uni-info-log.txt"
set "FW=flyway"
where flyway >nul 2>nul
if errorlevel 1 set "FW=%TEMP%\flyway-cli-13.6.0\flyway-13.6.0\flyway.cmd"
set "LOC=-locations=filesystem:./sql,filesystem:./sql-demo"
echo ==== %date% %time% ==== > "%LOG%"
echo Flyway: %FW% >> "%LOG%"
call "%FW%" -configFiles=flyway.conf %LOC% info >> "%LOG%" 2>&1
echo FIM codigo %errorlevel% >> "%LOG%"
