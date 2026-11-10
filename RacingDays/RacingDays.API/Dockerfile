FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSEC 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RacingDays.API.csproj", "./"]
RUN dotnet restore "RacingDays.API.csproj"
COPY . .
RUN dotnet build "RacingDays.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RacingDays.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RacingDays.API.dll"]
