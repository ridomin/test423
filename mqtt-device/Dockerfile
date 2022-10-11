#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["device-template/mqtt-device/mqtt-device.csproj", "device-template/mqtt-device/"]
RUN dotnet restore "device-template/mqtt-device/mqtt-device.csproj"
COPY . .
WORKDIR "/src/device-template/mqtt-device"
RUN dotnet build "mqtt-device.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mqtt-device.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mqtt-device.dll"]