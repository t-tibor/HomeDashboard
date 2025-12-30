# Power supply MQTT state messages:

## Power supply on:

Topic: zigbee2mqtt/NappaliLedTápegység
QoS: 0
``` json
{"identify":null,"linkquality":63,"power_on_behavior":"previous","state":"ON","update":{"installed_version":587765297,"latest_version":587765297,"state":"idle"}}
```

## Power supply off
Topic: zigbee2mqtt/NappaliLedTápegység QoS: 0
``` json
{"identify":null,"linkquality":70,"power_on_behavior":"previous","state":"OFF","update":{"installed_version":587765297,"latest_version":587765297,"state":"idle"}}
```

# Dimmer state messages

## Dimmer on
Topic: zigbee2mqtt/KonyhaLedDimmer
QoS: 0
```json
{"brightness":26,"color_mode":"color_temp","color_temp":327,"do_not_disturb":false,"effect":null,"linkquality":70,"state":"ON"}
```