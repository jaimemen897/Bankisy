# Imagen base para la construcción de la aplicación .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TFG.Controllers/TFG.Controllers.csproj", "TFG.Controllers/"]
COPY ["TFG.Services/TFG.Services.csproj", "TFG.Services/"]
COPY ["TFG.Context/TFG.Context.csproj", "TFG.Context/"]
RUN dotnet restore "TFG.Controllers/TFG.Controllers.csproj"
COPY . .
WORKDIR "/src/TFG.Controllers"
RUN dotnet dev-certs https --trust
RUN dotnet build "TFG.Controllers.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar la aplicación .NET
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TFG.Controllers.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=angular-build /app/dist/out ./wwwroot
ENV ASPNETCORE_ENVIRONMENT=Production

COPY ["TFG.Controllers/cert/certificate.pfx", "/https/certificate.pfx"]

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:5196;http://+:5195
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="password"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/certificate.pfx"

ENTRYPOINT ["dotnet", "TFG.Controllers.dll"]