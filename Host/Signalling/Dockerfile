#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
<<<<<<< Updated upstream
COPY ["Signalling/Signalling.csproj", "Signalling/"]
RUN dotnet restore "Signalling/Signalling.csproj"
COPY . .
WORKDIR "/src/Signalling"
=======
COPY ["Signalling.csproj", ""]
RUN dotnet restore "Signalling.csproj"
COPY . .
WORKDIR "/src/."
>>>>>>> Stashed changes
RUN dotnet build "Signalling.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Signalling.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Signalling.dll"]