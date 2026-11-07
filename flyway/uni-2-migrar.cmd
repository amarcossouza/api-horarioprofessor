@echo off
setlocal
cd /d "%~dp0"
set "LOG=%~dp0uni-migrar-log.txt"
set "FW=flyway"
where flyway >nul 2>nul
if errorlevel 1 set "FW=%TEMP%\flyway-cli-13.6.0\flyway-13.6.0\flyway.cmd"
set "LOC=-locations=filesystem:./sql,filesystem:./sql-demo"
echo ==== %date% %time% ==== > "%LOG%"
echo Flyway: %FW% >> "%LOG%"
echo ---- INFO ANTES ---- >> "%LOG%"
call "%FW%" -configFiles=flyway.conf %LOC% info >> "%LOG%" 2>&1
echo ---- VALIDATE ---- >> "%LOG%"
call "%FW%" -configFiles=flyway.conf %LOC% -ignoreMigrationPatterns=*:pending validate >> "%LOG%" 2>&1
if errorlevel 1 goto erro
echo ---- MIGRATE ---- >> "%LOG%"
call "%FW%" -configFiles=flyway.conf %LOC% migrate >> "%LOG%" 2>&1
if errorlevel 1 goto erro
echo ---- INFO DEPOIS ---- >> "%LOG%"
call "%FW%" -configFiles=flyway.conf %LOC% info >> "%LOG%" 2>&1
echo FIM OK >> "%LOG%"
goto :eof
:erro
echo FIM COM ERRO %errorlevel% >> "%LOG%"
