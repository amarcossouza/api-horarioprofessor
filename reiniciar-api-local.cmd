@echo off
rem Reinicia a API (5000) e o site (5051) locais. O Cenarios (5010) continua rodando.
set "LOGS=C:\projkinix\api-horarioprofessor\_uni-logs"
if not exist "%LOGS%" mkdir "%LOGS%"
echo Fechando API e site antigos...
taskkill /IM API-HorarioProfessor.exe /F >nul 2>&1
taskkill /IM HorarioProfessorMVC.exe /F >nul 2>&1
timeout /t 3 /nobreak >nul
start "API 5000" cmd /k "cd /d C:\projkinix\api-horarioprofessor && echo Compilando API 5000... && dotnet build > %LOGS%\build-api.txt 2>&1 && echo Compilou. Subindo API 5000... && dotnet run --no-build --urls http://localhost:5000 || (echo. & echo ERRO na compilacao. Veja %LOGS%\build-api.txt & type %LOGS%\build-api.txt | findstr /i "error")"
timeout /t 3 /nobreak >nul
start "SITE 5051" cmd /k "cd /d C:\projkinix\HorarioProfessor2025_v2 && echo Subindo SITE 5051... && dotnet run --urls http://localhost:5051"
