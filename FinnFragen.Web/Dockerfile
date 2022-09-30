#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
ENV PATH="$PATH:/root/.dotnet/tools"
COPY ["FinnFragen.Web/FinnFragen.Web.csproj", "FinnFragen.Web/"]
RUN dotnet restore "FinnFragen.Web/FinnFragen.Web.csproj"
COPY . .
WORKDIR "/src/FinnFragen.Web"
RUN libman restore
RUN dotnet build "FinnFragen.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinnFragen.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinnFragen.Web.dll"]