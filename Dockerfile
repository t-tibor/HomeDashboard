# Use the official .NET 9.0 ASP.NET runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 9.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["HomeDashboard.Web/HomeDashboard.Web.csproj", "HomeDashboard.Web/"]
RUN dotnet restore "HomeDashboard.Web/HomeDashboard.Web.csproj"

# Copy the rest of the source code
COPY HomeDashboard.Web/ HomeDashboard.Web/

# Set the working directory to the project folder
WORKDIR "/src/HomeDashboard.Web"

# Build the application
RUN dotnet build "HomeDashboard.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HomeDashboard.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the base image for the final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HomeDashboard.Web.dll"]