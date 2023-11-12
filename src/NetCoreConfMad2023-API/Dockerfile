#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NetCoreConfMad2023-API/NetCoreConfMad2023.API.csproj", "NetCoreConfMad2023-API/"]
COPY ["NetCoreConfMad2023-Observability/NetCoreConfMad2023.Observability.csproj", "NetCoreConfMad2023-Observability/"]
RUN dotnet restore "NetCoreConfMad2023-API/NetCoreConfMad2023.API.csproj"
COPY . .
WORKDIR "/src/NetCoreConfMad2023-API"
RUN dotnet build "NetCoreConfMad2023.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreConfMad2023.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreConfMad2023.API.dll"]