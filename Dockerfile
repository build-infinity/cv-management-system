FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CvManagementSystem.Api/CvManagementSystem.Api.csproj src/CvManagementSystem.Api/
COPY src/CvManagementSystem.Application/CvManagementSystem.Application.csproj src/CvManagementSystem.Application/
COPY src/CvManagementSystem.Infrastructure/CvManagementSystem.Infrastructure.csproj src/CvManagementSystem.Infrastructure/
COPY src/CvManagementSystem.Domain/CvManagementSystem.Domain.csproj src/CvManagementSystem.Domain/
RUN dotnet restore src/CvManagementSystem.Api/CvManagementSystem.Api.csproj

COPY src/ src/
RUN dotnet publish src/CvManagementSystem.Api/CvManagementSystem.Api.csproj \
    --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CvManagementSystem.Api.dll"]
