# Use the official .NET SDK as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Expose ports (matches the ones in docker-compose)
EXPOSE 8080
EXPOSE 8081

# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["MinitwitApi/MinitwitApi.csproj MinitwitApi/"]
RUN dotnet restore "MinitwitApi.csproj"

# Copy the entire project and build it
COPY . .
RUN dotnet publish "MinitwitApi.csproj" -c Release -o /app/publish

# Use the runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=prod
ENV ConnectionStrings__DefaultConnection="Server=minitwitdb,1433;Database=MinitwitDev;User Id=sa;Password=Lukas175!;TrustServerCertificate=True;"

# Start the API
ENTRYPOINT ["dotnet", "MinitwitApi.dll"]
