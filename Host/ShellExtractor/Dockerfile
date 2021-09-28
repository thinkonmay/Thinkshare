#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ShellExtractor/ShellExtractor.csproj", "ShellExtractor/"]
RUN dotnet restore "ShellExtractor/ShellExtractor.csproj"
COPY . .
WORKDIR "/src/ShellExtractor"
RUN dotnet build "ShellExtractor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShellExtractor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShellExtractor.dll"]