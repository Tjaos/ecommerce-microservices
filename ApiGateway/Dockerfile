# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia csproj e restaura pacotes
COPY ["ApiGateway.csproj", "./"]
RUN dotnet restore 

# Copia todo o código
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

#Força o kestrel a escutar na porta 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
