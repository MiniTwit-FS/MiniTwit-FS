# Use the official .NET SDK as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Expose ports (matches the ones in docker-compose)
EXPOSE 8080
EXPOSE 8081

# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY MiniTwitAPI/MiniTwitAPI.csproj ./
RUN dotnet restore "MiniTwitAPI.csproj"

# Copy the entire project and build it
COPY . .  
RUN dotnet publish "MiniTwitAPI/MiniTwitAPI.csproj" -c Release -o /app/publish

# Use the runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=prod
ENV ConnectionStrings__DefaultConnection="Host=10.132.0.16;Port=5432;Database=minitwitdb;Username=postgres;Password=FJ251499;SslMode=Prefer;"
ENV ENVIRONMENT=prod
ENV DB_NAME=postgres
ENV DB_USER=postgres
ENV DB_PASS=FJ251499
ENV INSTANCE_CONNECTION_NAME=sinuous-origin-452313-p2:europe-west1:minitwitdb
ENV DB_PORT=5432
ENV INSTANCE_HOST=10.132.0.16
ENV DB_ROOT_CERT=certs/server-ca.pem
ENV DB_CERT=certs/client-cert.pem
ENV DB_KEY=certs/client-key.pem
ENV PRIVATE_IP=TRUE


# Start the API
ENTRYPOINT ["dotnet", "MiniTwitAPI.dll"]
