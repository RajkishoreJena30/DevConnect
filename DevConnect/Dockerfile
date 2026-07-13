# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching)
COPY ["DevConnect.csproj", "./"]
RUN dotnet restore "DevConnect.csproj"

# Copy the rest and publish
COPY . .
RUN dotnet publish "DevConnect.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kestrel listens on 8080 inside the container (no HTTPS cert needed in the container)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DevConnect.dll"]
