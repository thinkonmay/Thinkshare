FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443




FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["Conductor/Conductor.csproj", "Conductor/"]
COPY ["SharedHost/setting/prod/appsettings.json", "Conductor/"]
RUN dotnet restore "Conductor/Conductor.csproj"


# copy setting from setting prod folder to workdir
COPY . .
WORKDIR "/src/Conductor"
RUN dotnet build "Conductor.csproj" -c Release -o /app/build







FROM build AS publish
RUN dotnet publish "Conductor.csproj" -c Release -o /app/publish





FROM base AS final 
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conductor.dll"]