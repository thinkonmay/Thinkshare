FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["Signalling/Signalling.csproj", "Signalling/"]
RUN dotnet restore "Signalling/Signalling.csproj"
COPY . .
WORKDIR "/src/Signalling"
RUN dotnet build "Signalling.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "Signalling.csproj" -c Release -o /app/publish


#debug image
FROM build AS debug
WORKDIR /src/Signalling

RUN dotnet dev-certs https
RUN apt-get update
RUN apt-get install -y unzip
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg

RUN dotnet run


FROM base AS release
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Signalling.dll"]

