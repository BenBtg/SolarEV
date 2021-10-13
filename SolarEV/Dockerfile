FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SolarEV/SolarEV.csproj", "SolarEV/"]
RUN dotnet restore "SolarEV\SolarEV.csproj"
COPY . .
WORKDIR "/src/SolarEV"
RUN dotnet build "SolarEV.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SolarEV.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SolarEV.dll"]
