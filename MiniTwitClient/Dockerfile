# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY MiniTwitClient.csproj ./
RUN dotnet restore "MiniTwitClient.csproj"

# Copy everything else and build
COPY . .  
RUN dotnet publish "MiniTwitClient.csproj" -c Release -o /app/publish

# Runtime Stage: Use Nginx to serve static files
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default Nginx static content
RUN rm -rf ./*

# Copy Blazor WASM published files
COPY --from=build /app/publish/wwwroot .

# Expose ports
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
