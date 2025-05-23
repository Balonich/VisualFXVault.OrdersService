#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 7070
EXPOSE 7071

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["APILayer/APILayer.csproj", "APILayer/"]
RUN dotnet restore "./APILayer/APILayer.csproj"
COPY . .
WORKDIR "/src/APILayer"
RUN dotnet build "./APILayer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./APILayer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV USERS_SERVICE_HOST=http://localhost
ENV USERS_SERVICE_PORT=9090
ENV MONGO_HOST=localhost
ENV MONGO_PORT=27017
ENV MONGO_DB=VisualFXVault_Orders
ENTRYPOINT ["dotnet", "APILayer.dll"]