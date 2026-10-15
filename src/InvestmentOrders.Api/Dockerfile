# Build context is the repo root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/InvestmentOrders.SharedKernel/InvestmentOrders.SharedKernel.csproj src/InvestmentOrders.SharedKernel/
COPY src/InvestmentOrders.Domain/InvestmentOrders.Domain.csproj src/InvestmentOrders.Domain/
COPY src/InvestmentOrders.Infrastructure/InvestmentOrders.Infrastructure.csproj src/InvestmentOrders.Infrastructure/
COPY src/InvestmentOrders.Application/InvestmentOrders.Application.csproj src/InvestmentOrders.Application/
COPY src/InvestmentOrders.Api/InvestmentOrders.Api.csproj src/InvestmentOrders.Api/
RUN dotnet restore src/InvestmentOrders.Api/InvestmentOrders.Api.csproj

COPY src/ src/
RUN dotnet publish src/InvestmentOrders.Api/InvestmentOrders.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "InvestmentOrders.Api.dll"]
