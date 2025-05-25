FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081



FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PaymentIntegration.API/PaymentIntegration.API.csproj", "PaymentIntegration.API/"]
COPY ["PaymentIntegration.Application/PaymentIntegration.Application.csproj", "PaymentIntegration.Application/"]
COPY ["PaymentIntegration.Domain/PaymentIntegration.Domain.csproj", "PaymentIntegration.Domain/"]
COPY ["PaymentIntegration.Infrastructure/PaymentIntegration.Infrastructure.csproj", "PaymentIntegration.Infrastructure/"]

ARG GITHUB_PAC

RUN dotnet nuget add source "https://nuget.pkg.github.com/beyzaaydogan/index.json" -n github --username $GITHUB_USERNAME --password $GITHUB_PAC --store-password-in-clear-text

RUN dotnet restore "PaymentIntegration.API/PaymentIntegration.API.csproj"
COPY . .
WORKDIR "/src/PaymentIntegration.API"
RUN dotnet build "PaymentIntegration.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentIntegration.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentIntegration.API.dll"]
