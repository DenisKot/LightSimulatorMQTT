# LightSimulatorMQTT

## Home Assistant configuration.yaml:
```
# MQTT Light Configuration
mqtt:
  - light:
      name: "Simulated Light Bulb"
      command_topic: "home/lightbulb/set"
      state_topic: "home/lightbulb/state"
      payload_on: "ON"
      payload_off: "OFF"
      qos: 0
      retain: true
```
