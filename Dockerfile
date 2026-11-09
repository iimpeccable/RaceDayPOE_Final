# ASP.NET Core Web MVC Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RacingDays.Web/RacingDays.Web.csproj", "RacingDays.Web/"]
RUN dotnet restore "RacingDays.Web/RacingDays.Web.csproj"
COPY . .
WORKDIR "/src/RacingDays.Web"
RUN dotnet build "RacingDays.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RacingDays.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RacingDays.Web.dll"]