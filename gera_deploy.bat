@echo off
chcp 65001 >nul
setlocal
REM ===================================================================
REM  GERA O PACOTE LINUX - API HORARIO PROFESSOR
REM  Pacote (caminho padrao do dotnet publish):
REM     bin\Release\net8.0\linux-x64\publish\
REM  Destino no droplet: /var/www/horarioprofessor-api
REM  Modelos do servidor (service, config, passo a passo): servidor-linux\
REM  Uso sem pausa: gera_deploy.bat /auto
REM ===================================================================
cd /d "%~dp0"
set "PACOTE=bin\Release\net8.0\linux-x64\publish"

echo.
echo [1/2] Limpando pacote anterior...
if exist "%PACOTE%" rmdir /s /q "%PACOTE%"

echo.
echo [2/2] Publicando para Linux (linux-x64 self-contained)...
dotnet publish "API-HorarioProfessor.csproj" -c Release -r linux-x64 --self-contained true
if errorlevel 1 goto erro
if not exist "%PACOTE%\API-HorarioProfessor" goto erro
if exist "%PACOTE%\appsettings.Development.json" goto erro

echo.
echo ===================================================================
echo  PRONTO! Pacote em:
echo     %~dp0%PACOTE%
echo.
echo  FileZilla: envie o CONTEUDO dessa pasta para /var/www/horarioprofessor-api/
echo  (sobrescrever; a config de producao ja vai no pacote)
echo  Depois, no console: systemctl restart horarioprofessor-api
echo  Primeira vez? Veja servidor-linux\PASSO-A-PASSO.md
echo ===================================================================
if not "%1"=="/auto" pause
exit /b 0

:erro
echo.
echo *** ERRO - o pacote NAO foi gerado. Veja a mensagem acima. ***
if not "%1"=="/auto" pause
exit /b 1
