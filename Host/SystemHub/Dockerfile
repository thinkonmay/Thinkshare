FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443




FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["SystemHub/SystemHub.csproj", "SystemHub/"]
COPY ["SharedHost/setting/prod/appsettings.json", "SystemHub/"]
RUN dotnet restore "SystemHub/SystemHub.csproj"


# copy setting from setting prod folder to workdir
COPY . .
WORKDIR "/src/SystemHub"
RUN dotnet build "SystemHub.csproj" -c Release -o /app/build





FROM build AS publish
RUN dotnet publish "SystemHub.csproj" -c Release -o /app/publish






# production and development image both base on base image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SystemHub.dll"]