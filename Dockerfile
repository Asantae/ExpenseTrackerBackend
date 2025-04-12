# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install sqlite3 CLI (needed to execute init.sql)
RUN apt-get update && apt-get install -y sqlite3

# Create the directory for SQLite based on your connection string in appsettings.json
RUN mkdir -p /home/asantae && chmod -R 777 /home/asantae

# Copy your initialization script
COPY init.sql /app/init.sql

# Copy the entrypoint script and ensure it is executable
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

# Copy published output from build
COPY --from=build /app/publish .

# Expose the container’s port and set the ASP.NET URLs
EXPOSE 80
ENV ASPNETCORE_URLS=http://0.0.0.0:80

ENTRYPOINT ["/app/docker-entrypoint.sh"]
