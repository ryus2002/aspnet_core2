@echo off
echo ===== Building Solution =====
dotnet build MicroservicesEcommerce.sln -c Release

IF ERRORLEVEL 1 (
    echo Build failed, aborting tests!
    exit /b %ERRORLEVEL%
)

echo.
echo ===== Running Unit Tests =====
echo Running ProductService Tests...
dotnet test tests\ProductService.Tests\ProductService.Tests.csproj -c Release
echo.

echo Running AuthService Tests...
dotnet test tests\AuthService.Tests\AuthService.Tests.csproj -c Release
echo.

echo Running OrderService Tests...
dotnet test tests\OrderService.Tests\OrderService.Tests.csproj -c Release
echo.

echo ===== Running Integration Tests =====
echo Note: Integration tests require Docker to be running
echo If you want to skip integration tests, press Ctrl+C now
timeout /t 5
dotnet test tests\ProductService.IntegrationTests\ProductService.IntegrationTests.csproj -c Release

echo.
echo ===== All Done! =====