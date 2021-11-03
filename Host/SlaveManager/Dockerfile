FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443





FROM mcr.microsoft.com/dotnet/sdk:5.0 AS BuildProd
WORKDIR /src

COPY ["SlaveManager/SlaveManager.csproj", "SlaveManager/"]
COPY ["setting/prod/appsettings.json", "SlaveManager/"]
RUN dotnet restore "SlaveManager/SlaveManager.csproj"


# copy setting from setting prod folder to workdir
COPY . .
WORKDIR "/src/SlaveManager"
RUN dotnet build "SlaveManager.csproj" -c Release -o /app/build






FROM mcr.microsoft.com/dotnet/sdk:5.0 AS BuildDev
WORKDIR /src

COPY ["SlaveManager/SlaveManager.csproj", "SlaveManager/"]
COPY ["setting/dev/appsettings.json", "SlaveManager/"]
RUN dotnet restore "SlaveManager/SlaveManager.csproj"


# copy setting from setting dev folder to workdir
COPY . .
WORKDIR "/src/SlaveManager"
RUN dotnet build "SlaveManager.csproj" -c Release -o /app/build




FROM BuildDev AS PublishDev
RUN dotnet publish "SlaveManager.csproj" -c Release -o /app/publish

FROM BuildProd AS PublishProd
RUN dotnet publish "SlaveManager.csproj" -c Release -o /app/publish






# production and development image both base on base image
FROM base AS dev
WORKDIR /app
COPY --from=PublishDev /app/publish .
ENTRYPOINT ["dotnet", "SlaveManager.dll"]

FROM base AS prod
WORKDIR /app
COPY --from=PublishProd /app/publish .
ENTRYPOINT ["dotnet", "SlaveManager.dll"]