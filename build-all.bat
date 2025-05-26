@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo Microservices E-commerce Platform - Build All Script
echo ===================================================
echo.

echo 1. Building main solution...
dotnet restore MicroservicesEcommerce.sln
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Solution restore failed!
    goto :error
)

dotnet build MicroservicesEcommerce.sln -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Solution build failed!
    goto :error
)
echo SUCCESS: Solution built successfully!
echo.

echo 2. Building shared libraries...
set SHARED_LIBS=shared\Shared.Messaging shared\Shared.Logging shared\Shared.Monitoring shared\Shared.HealthChecks

for %%s in (%SHARED_LIBS%) do (
    if exist %%s (
        echo Building %%s...
        dotnet restore %%s
        dotnet build %%s -c Release
        if !ERRORLEVEL! NEQ 0 (
            echo ERROR: %%s build failed!
            goto :error
        )
        echo SUCCESS: %%s built successfully!
    )
)
echo.

echo 3. Building microservices...
set SERVICES=services\api-gateway services\auth-service services\order-service services\payment-service services\product-service services\cart-service

for %%s in (%SERVICES%) do (
    if exist %%s (
        echo Building %%s...
        dotnet restore %%s
        dotnet build %%s -c Release
        if !ERRORLEVEL! NEQ 0 (
            echo ERROR: %%s build failed!
            goto :error
        )
        echo SUCCESS: %%s built successfully!
    )
)
echo.

echo 4. Building test projects...
set TEST_PROJECTS=tests\AuthService.Tests tests\OrderService.Tests tests\ProductService.Tests tests\ProductService.IntegrationTests

for %%t in (%TEST_PROJECTS%) do (
    if exist %%t (
        echo Building %%t...
        dotnet restore %%t
        dotnet build %%t -c Release
        if !ERRORLEVEL! NEQ 0 (
            echo ERROR: %%t build failed!
            goto :error
        )
        echo SUCCESS: %%t built successfully!
    )
)
echo.

echo All projects built successfully!
goto :end

:error
echo Build process encountered an error. Please check the error messages above.
exit /b 1

:end
echo.
echo ===================================================
echo Build process completed!
echo ===================================================
exit /b 0