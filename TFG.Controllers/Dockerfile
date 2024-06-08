# Imagen base para la construcción de la aplicación Angular
FROM node:22.2.0 AS angular-build
WORKDIR /app
COPY TFG.Controllers/ClientApp/package.json TFG.Controllers/ClientApp/package-lock.json ./
RUN npm ci
COPY TFG.Controllers/ClientApp/ .
ENV NODE_OPTIONS="--max_old_space_size=2048"
RUN npm run build --prod --source-map=false --output-path=./dist/out

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
ENTRYPOINT ["dotnet", "TFG.Controllers.dll"]