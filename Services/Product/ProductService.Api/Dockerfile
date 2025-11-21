# ProductService API Dockerfile
# Path: Services/Product/ProductService.Api/Dockerfile
# Build from the solution root: docker build -f Services/Product/ProductService.Api/Dockerfile -t productservice-api .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only csproj files first (for better layer caching)
COPY ["Services/Product/ProductService.Api/ProductService.Api.csproj", "Services/Product/ProductService.Api/"]
COPY ["Services/Product/ProductService.Application/ProductService.Application.csproj", "Services/Product/ProductService.Application/"]
COPY ["Services/Product/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "Services/Product/ProductService.Infrastructure/"]
COPY ["Services/Product/ProductService.Domain/ProductService.Domain.csproj", "Services/Product/ProductService.Domain/"]

# Restore the API project (this pulls in all referenced projects)
RUN dotnet restore "Services/Product/ProductService.Api/ProductService.Api.csproj"

# Now copy the rest of the solution
COPY . .

# Publish the API project
WORKDIR "/src/Services/Product/ProductService.Api"
RUN dotnet publish "ProductService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
