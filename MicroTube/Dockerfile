FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN apt update
RUN apt -y install ffmpeg
WORKDIR /src
COPY ["MicroTube.csproj", "MicroTube/"]
RUN dotnet restore "MicroTube/MicroTube.csproj"
COPY . MicroTube/
WORKDIR "/src/MicroTube"
RUN dotnet build "MicroTube.csproj" -c Release -o /app/build
RUN apt install ca-certificates
WORKDIR /app/build
ENTRYPOINT update-ca-certificates & dotnet MicroTube.dll