FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443





FROM mcr.microsoft.com/dotnet/sdk:5.0 AS BuildProd
WORKDIR /src

COPY ["MetricCollector/MetricCollector.csproj", "MetricCollector/"]
COPY ["setting/prod/appsettings.json", "MetricCollector/"]
RUN dotnet restore "MetricCollector/MetricCollector.csproj"


# copy setting from setting prod folder to workdir
COPY . .
WORKDIR "/src/MetricCollector"
RUN dotnet build "MetricCollector.csproj" -c Release -o /app/build






FROM mcr.microsoft.com/dotnet/sdk:5.0 AS BuildDev
WORKDIR /src

COPY ["MetricCollector/MetricCollector.csproj", "MetricCollector/"]
COPY ["setting/dev/appsettings.json", "MetricCollector/"]
RUN dotnet restore "MetricCollector/MetricCollector.csproj"


# copy setting from setting dev folder to workdir
COPY . .
WORKDIR "/src/MetricCollector"
RUN dotnet build "MetricCollector.csproj" -c Release -o /app/build




FROM BuildDev AS PublishDev
RUN dotnet publish "MetricCollector.csproj" -c Release -o /app/publish

FROM BuildProd AS PublishProd
RUN dotnet publish "MetricCollector.csproj" -c Release -o /app/publish






# production and development image both base on base image
FROM base AS dev
WORKDIR /app
COPY --from=PublishDev /app/publish .
ENTRYPOINT ["dotnet", "MetricCollector.dll"]

FROM base AS prod
WORKDIR /app
COPY --from=PublishProd /app/publish .
ENTRYPOINT ["dotnet", "MetricCollector.dll"]