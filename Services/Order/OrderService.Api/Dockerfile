# OrderService API Dockerfile
# Path: Services/Order/OrderService.Api/Dockerfile
# Build from the solution root: docker build -f Services\Order\OrderService.Api\Dockerfile -t orderservice-api .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Order/OrderService.Api/OrderService.Api.csproj", "Services/Order/OrderService.Api/"]
COPY ["Services/Order/OrderService.Application/OrderService.Application.csproj", "Services/Order/OrderService.Application/"]
COPY ["Services/Order/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "Services/Order/OrderService.Infrastructure/"]
COPY ["Services/Order/OrderService.Domain/OrderService.Domain.csproj", "Services/Order/OrderService.Domain/"]

RUN dotnet restore "Services/Order/OrderService.Api/OrderService.Api.csproj"

COPY . .

WORKDIR "/src/Services/Order/OrderService.Api"
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]

