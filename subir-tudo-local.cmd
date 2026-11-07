@echo off
rem Sobe localmente os 3 servicos do Horario Professor, cada um numa janela.
rem Compilacao gravada em C:\projkinix\api-horarioprofessor\_uni-logs
set "LOGS=C:\projkinix\api-horarioprofessor\_uni-logs"
if not exist "%LOGS%" mkdir "%LOGS%"
start "API 5000" cmd /k "cd /d C:\projkinix\api-horarioprofessor && echo Compilando API 5000... && dotnet build > %LOGS%\build-api.txt 2>&1 && echo Compilou. Subindo API 5000... && dotnet run --no-build --urls http://localhost:5000 || (echo. & echo ERRO na compilacao de API 5000. Veja %LOGS%\build-api.txt & type %LOGS%\build-api.txt | findstr /i "error")"
timeout /t 3 /nobreak >nul
start "CENARIOS 5010" cmd /k "cd /d C:\projkinix\api-cenarios && echo Compilando CENARIOS 5010... && dotnet build > %LOGS%\build-cenarios.txt 2>&1 && echo Compilou. Subindo CENARIOS 5010... && dotnet run --no-build --urls http://localhost:5010 || (echo. & echo ERRO na compilacao de CENARIOS 5010. Veja %LOGS%\build-cenarios.txt & type %LOGS%\build-cenarios.txt | findstr /i "error")"
timeout /t 3 /nobreak >nul
start "SITE 5051" cmd /k "cd /d C:\projkinix\HorarioProfessor2025_v2 && echo Compilando SITE 5051... && dotnet build > %LOGS%\build-site.txt 2>&1 && echo Compilou. Subindo SITE 5051... && dotnet run --no-build --urls http://localhost:5051 || (echo. & echo ERRO na compilacao de SITE 5051. Veja %LOGS%\build-site.txt & type %LOGS%\build-site.txt | findstr /i "error")"
echo.
echo Tres janelas abertas: API 5000, CENARIOS 5010 e SITE 5051.
echo Quando aparecer 'Now listening on' nas tres, abra http://localhost:5051
pause
