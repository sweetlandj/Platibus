@echo off

echo. 
echo ========================================
echo               Unit Tests                
echo ========================================
echo.

pushd Source\Platibus.UnitTests
docker-compose up -d
timeout /t 10
dotnet test --filter Category!=Explicit
docker-compose down
popd

echo. 
echo ========================================
echo            Integration Tests            
echo ========================================
echo. 

pushd Source\Platibus.IntegrationTests
docker-compose up -d
timeout /t 10
dotnet test --filter Category!=Explicit
docker-compose down
popd