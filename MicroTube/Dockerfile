FROM jrottenberg/ffmpeg AS ffmpgeg
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MicroTube.csproj", "MicroTube/"]
RUN dotnet restore "MicroTube/MicroTube.csproj"
COPY . MicroTube/
WORKDIR "/src/MicroTube"
RUN dotnet build "MicroTube.csproj" -c Release -o /app/build

WORKDIR /app/build
ENTRYPOINT dotnet MicroTube.dll
#FROM build AS publish
#RUN dotnet publish "MicroTube.csproj" -c Release -o /app/publish /p:UseAppHost=false