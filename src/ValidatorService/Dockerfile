FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Expose the required ports
EXPOSE 5001
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src/ValidatorService

# Copy project file and restore dependencies (optimized for Docker caching)
COPY ["ValidatorService.csproj", "./"]
RUN dotnet restore "./ValidatorService.csproj"

# Copy everything and build the application
COPY . .
RUN dotnet publish "./ValidatorService.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "ValidatorService.dll"]