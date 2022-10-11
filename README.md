# Device Template

This repo can be used as a template repo to create new MQTT devices using [MQTTnet.Extensions.MultiCloud](https://github.com/iotmodels/MQTTnet.Extensions.MultiCloud)

## What is in this template?

- A dotnet 6.0 worker project
- With references to MQTTnet MultiCloud extensions
- A sample device template model
  - The C# interface representing the device model
  - Implementations for Azure IoT Hub and MQTT Broker
  - Sample Device implementation
  - Default launch settings for different endpoints
  - Sample CA certificate to connect to [mosquitto-local](https://github.com/ridomin/mosquitto-local)

### Device Model

The device implements:
- `sdkInfo` as a reported property of type `string`
- `interval` as a desired property of type `int`
- `temp` as a telemetry of type `double` (uses random values)
- `echo` a command that receives a `string` and returns another `string`

## How to run

The dotnet project contains a `launchSettings.json.template` with some pre-configured profiles. To avoid publishing this file with credentials, this file it's git.ignored. Run `./init-template.sh` to rename this file, then use `dotnet run --launch-profile <profile>`.

### MQTT Broker
To connect to a local MQTT broker like mosquitto, you can use the [mosquitto-local](https://github.com/ridomin/mosquitto-local) Docker image.

```
docker run -it --rm -p 8080:8080 -p 1883:1883 -p 8883:8883 -p 8884:8884 -p 8443:8443  ridomin/mosquitto-local:dev
```

### Azure IoT services

To use Azure IoT Hub, create a device identity and provide the device connection string
To use DPS, or Central, provide IdScope and enrollment details.

## Interact with the device

- When targeting a MQTT broker, you can use [pnp-mqtt](https://iotmodels.github.io/iotux-mqtt/) and connect to the broker configured

- When targeting Azure IoT Hub, you can use IoT Explorer
- When targeting Azure IoT Central, create a device template based on the DTDL sample model