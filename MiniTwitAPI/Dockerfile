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
ENV ConnectionStrings__DefaultConnection="Server=tcp:34.38.158.201,1433;Database=minitwit-db;User Id=admin;Password=FJ251499;TrustServerCertificate=False;Encrypt=True;"

# Start the API
ENTRYPOINT ["dotnet", "MiniTwitAPI.dll"]
