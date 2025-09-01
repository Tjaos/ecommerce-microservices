# -------------- BUILD --------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar csproj e restaurar
COPY ["SalesService.csproj", "./"]
RUN dotnet restore

# Copiar o restante
COPY . .
RUN dotnet publish -c Release -o /app/publish

# -------------- RUNTIME --------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copiar publish
COPY --from=build /app/publish .

# Definir variáveis de ambiente
ENV SQLSERVER_HOST=sqlserver \
    SQLSERVER_PORT=1433 \
    SQLSERVER_USER=sa \
    SQLSERVER_PASSWORD=Str0ngP@ssword!

# Entrypoint: primeiro espera o SQL, depois roda o serviço
ENTRYPOINT ["dotnet", "SalesService.dll"]
