FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["services/auth-service/AuthService.csproj", "services/auth-service/"]
RUN dotnet restore "services/auth-service/AuthService.csproj"
COPY services/auth-service/ services/auth-service/
WORKDIR "/src/services/auth-service"
RUN dotnet build "AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]