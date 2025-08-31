# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["InventoryService.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copiar build
COPY --from=build /app/publish .

# Rodar serviço apenas após SQL Server estar disponível
ENTRYPOINT ["dotnet", "InventoryService.dll"]
