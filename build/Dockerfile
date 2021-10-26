FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["build/Build.csproj", "build/"]
RUN dotnet restore "build\Build.csproj"
COPY . .
WORKDIR "/src/build"
RUN dotnet build "Build.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Build.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Build.dll"]
