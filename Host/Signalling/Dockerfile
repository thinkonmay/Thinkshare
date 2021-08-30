FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

#debug image
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS debug

RUN mkdir /work/
WORKDIR /work/

COPY ./Signalling/Signalling.csproj /work/Signalling.csproj
RUN dotnet restore


COPY . /work/
RUN mkdir /out/
RUN dotnet publish "Signalling.csproj" --no-restore --output /out/ --configuration Release


RUN apt-get update
RUN apt-get install -y unzip
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg

RUN dotnet run


#production image
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

COPY Signalling/Signalling.csproj /app/Signalling.csproj
RUN dotnet restore

COPY . .
RUN dotnet publish Signalling.csproj -c release -o /app --no-restore

FROM base as release
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Signalling.dll"]




