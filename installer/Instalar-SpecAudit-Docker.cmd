@echo off
chcp 65001 >nul
echo ============================================================================
echo   SpecAudit - Instalador / Inicializador Docker (Background Container)
echo   Autor: Carlos Wagner G. Bittencourt
echo ============================================================================
echo.
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERRO] O Docker Desktop nao esta em execucao ou nao foi encontrado.
    echo Inicie o Docker Desktop e execute este instalador novamente.
    pause
    exit /b 1
)

echo Construindo e iniciando o container SpecAudit em segundo plano...
pushd "%~dp0.."
docker compose up -d --build
if errorlevel 1 (
    popd
    echo [ERRO] Falha ao iniciar o container SpecAudit.
    pause
    exit /b 1
)
popd

echo.
echo SpecAudit em execucao no Docker em http://127.0.0.1:5187 !
echo - Pasta de projetos mapeada (somente leitura): /projetos
echo - Pasta de relatorios mapeada (externa):       /relatorios
start "" "http://127.0.0.1:5187"
pause
