param(
    [ValidateSet('info', 'validate', 'migrate')]
    [string]$Command = 'info'
)

$ErrorActionPreference = 'Stop'

$configPath = Join-Path $PSScriptRoot 'flyway.conf'
$migrationsPath = Join-Path $PSScriptRoot 'sql'

$flywayExecutable = Get-Command flyway -ErrorAction SilentlyContinue
if ($flywayExecutable) {
    $flywayPath = $flywayExecutable.Source
}
else {
    $flywayPath = Join-Path $env:TEMP 'flyway-cli-13.6.0\flyway-13.6.0\flyway.cmd'
}

if (-not (Test-Path -LiteralPath $flywayPath)) {
    throw 'Flyway não encontrado. Instale-o no PATH ou baixe a distribuição portátil oficial.'
}

& $flywayPath `
    "-configFiles=$configPath" `
    "-locations=filesystem:$migrationsPath" `
    $Command

exit $LASTEXITCODE
