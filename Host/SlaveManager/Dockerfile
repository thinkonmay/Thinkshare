FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443





FROM mcr.microsoft.com/dotnet/sdk:5.0 AS buildprod
WORKDIR /src

COPY ["WorkerManager/WorkerManager.csproj", "WorkerManager/"]
COPY ["SharedHost/setting/prod/appsettings.json", "WorkerManager/"]
RUN dotnet restore "WorkerManager/WorkerManager.csproj"


# copy setting from setting prod folder to workdir
COPY . .
WORKDIR "/src/WorkerManager"
RUN dotnet build "WorkerManager.csproj" -c Release -o /app/build






FROM mcr.microsoft.com/dotnet/sdk:5.0 AS builddev
WORKDIR /src

COPY ["WorkerManager/WorkerManager.csproj", "WorkerManager/"]
COPY ["SharedHost/setting/dev/appsettings.json", "WorkerManager/"]
RUN dotnet restore "WorkerManager/WorkerManager.csproj"


# copy setting from setting dev folder to workdir
COPY . .
WORKDIR "/src/WorkerManager"
RUN dotnet build "WorkerManager.csproj" -c Release -o /app/build




FROM builddev AS publishdev
RUN dotnet publish "WorkerManager.csproj" -c Release -o /app/publish

FROM buildprod AS publishprod
RUN dotnet publish "WorkerManager.csproj" -c Release -o /app/publish






# production and development image both base on base image
FROM base AS dev
WORKDIR /app
COPY --from=publishdev /app/publish .
ENTRYPOINT ["dotnet", "WorkerManager.dll"]

FROM base AS prod
WORKDIR /app
COPY --from=publishprod /app/publish .
ENTRYPOINT ["dotnet", "WorkerManager.dll"]