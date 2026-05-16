# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CutBookApi.csproj", "."]
RUN dotnet restore "CutBookApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CutBookApi.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "CutBookApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image setup
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CutBookApi.dll"]