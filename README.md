# HomeDashboard - Docker Setup

This repository contains a HomeDashboard ASP.NET Core web application with Docker support for both x86_64 and ARM64 architectures.

## Docker Commands Summary

### Building Images

#### Standard Build (x86_64)
```bash
docker build -t homedashboard-web .
```
Builds the Docker image for x86_64 architecture using the default Docker builder.

#### ARM64 Build (for Raspberry Pi 5)
```bash
# Setup Docker buildx for multi-platform builds
docker buildx create --use

# Build ARM64 image
docker buildx build --platform linux/arm64 -t homedashboard-web:arm64 --load .
```
Builds the Docker image specifically for ARM64 architecture, optimized for Raspberry Pi 5 and other ARM64 devices.

### Running Containers

#### Run on x86_64 systems
```bash
docker run -p 8080:8080 homedashboard-web
```

#### Run with mounted config file
```bash
docker run -p 8080:8080 -v /path/to/config.json:/config.json homedashboard-web
```

Mounts a local configuration file to override default settings.

### Image Management

#### List built images
```bash
docker images | findstr homedashboard
```
Shows all built HomeDashboard images with their tags and sizes.

## Architecture Support

- **x86_64**: Standard desktop/server architecture
- **ARM64**: Raspberry Pi 5 and other ARM64-based devices

## Application Details

- **Framework**: ASP.NET Core 9.0
- **Architecture**: Web application with MQTT integration
- **Ports**: Exposes port 8080 for web access
- **Base Images**: Uses Microsoft's official .NET 9.0 images

## Configuration

The application supports multiple configuration sources that are merged in order:

1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific settings
3. `/config.json` - Optional mounted configuration file (container deployments)

### Mounted Config File

When running in containers, you can mount a JSON configuration file to `/config.json` to override any settings:

```json
{
  "AllowedHosts": "*",

  "MqttConfig": {
    "ServerHostName": "mosquitto-mqtts",
    "Topics": {
      "Dimmer": "zigbee2mqtt/KonyhaLedDimmer",
      "DimmerSet": "zigbee2mqtt/KonyhaLedDimmer/set",
      "PowerSupplySet": "zigbee2mqtt/NappaliLedTápegység/set"
    }
  }
}

```

This allows for flexible configuration in containerized environments without rebuilding the image.</content>
<parameter name="filePath">c:\Users\Tibi\source\repos\HomeDashboard\README.md