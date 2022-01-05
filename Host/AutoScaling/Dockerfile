FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80



FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["AutoScaling/AutoScaling.csproj", "AutoScaling/"]
COPY ["SharedHost/setting/dev/appsettings.json", "AutoScaling/"]
RUN dotnet restore "AutoScaling/AutoScaling.csproj"


# copy setting from setting dev folder to workdir
COPY . .
WORKDIR "/src/AutoScaling"
RUN dotnet build "AutoScaling.csproj" -c Release -o /app/build




FROM build AS publish
RUN dotnet publish "AutoScaling.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publishprod /app/publish .
ENTRYPOINT ["dotnet", "AutoScaling.dll"]