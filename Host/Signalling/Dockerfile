FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443



FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["Signalling/Signalling.csproj", "Signalling/"]
COPY ["SharedHost/setting/prod/appsettings.json", "Signalling/"]
RUN dotnet restore "Signalling/Signalling.csproj"


# copy setting from setting prod folder to workdir
COPY . .
WORKDIR "/src/Signalling"
RUN dotnet build "Signalling.csproj" -c Release -o /app/build




FROM build AS publish
RUN dotnet publish "Signalling.csproj" -c Release -o /app/publish





# production and development image both base on base image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Signalling.dll"]