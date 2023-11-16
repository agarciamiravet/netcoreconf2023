#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NetCoreConfmad2023.Front/NetCoreConfmad2023.Front.csproj", "NetCoreConfmad2023.Front/"]
COPY ["NetCoreConfMad2023-Observability/NetCoreConfMad2023.Observability.csproj", "NetCoreConfMad2023-Observability/"]
RUN dotnet restore "NetCoreConfmad2023.Front/NetCoreConfmad2023.Front.csproj"
COPY . .
WORKDIR "/src/NetCoreConfmad2023.Front"
RUN dotnet build "NetCoreConfmad2023.Front.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreConfmad2023.Front.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreConfmad2023.Front.dll"]