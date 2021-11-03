FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443





FROM mcr.microsoft.com/dotnet/sdk:5.0 AS BuildProd
WORKDIR /src

COPY ["Conductor/Conductor.csproj", "Conductor/"]
COPY ["setting/prod/appsettings.json", "Conductor/"]
RUN dotnet restore "Conductor/Conductor.csproj"


# copy setting from setting prod folder to workdir
COPY . .
COPY ["setting/prod/app-setting.json", "Conductor/"]
WORKDIR "/src/Conductor"
RUN dotnet build "Conductor.csproj" -c Release -o /app/build






FROM mcr.microsoft.com/dotnet/sdk:5.0 AS BuildDev
WORKDIR /src

COPY ["Conductor/Conductor.csproj", "Conductor/"]
COPY ["setting/dev/appsettings.json", "Conductor/"]
RUN dotnet restore "Conductor/Conductor.csproj"


# copy setting from setting dev folder to workdir
COPY . .
COPY ["setting/dev/app-setting.json", "Conductor/"]
WORKDIR "/src/Conductor"
RUN dotnet build "Conductor.csproj" -c Release -o /app/build




FROM BuildDev AS PublishDev
RUN dotnet publish "Conductor.csproj" -c Release -o /app/publish

FROM BuildProd AS PublishProd
RUN dotnet publish "Conductor.csproj" -c Release -o /app/publish






# production and development image both base on base image
FROM base AS dev
WORKDIR /app
COPY --from=PublishDev /app/publish .
ENTRYPOINT ["dotnet", "Conductor.dll"]

FROM base AS prod
WORKDIR /app
COPY --from=PublishProd /app/publish .
ENTRYPOINT ["dotnet", "Conductor.dll"]