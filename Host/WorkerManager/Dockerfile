FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443





FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["WorkerManager/WorkerManager.csproj", "WorkerManager/"]
RUN dotnet restore "WorkerManager/WorkerManager.csproj"


# copy setting from setting prod folder to workdir
COPY . .
WORKDIR "/src/WorkerManager"
RUN dotnet build "WorkerManager.csproj" -c Release -o /app/build




FROM build AS publish 
RUN dotnet publish "WorkerManager.csproj" -c Release -o /app/publish





FROM base AS prod
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerManager.dll"]