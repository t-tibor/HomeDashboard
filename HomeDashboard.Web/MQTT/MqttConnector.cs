using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace HomeDashboard.Web.MQTT;

public class MqttConnector(IOptions<MqttConfig> configOptions) : IMqttConnector
{
    public async Task<IDisposable> SubscribeAsync(
        Action<MqttClientSubscribeOptionsBuilder> subscribeConfig,
        Func<MqttApplicationMessageReceivedEventArgs, Task> action,
        CancellationToken token = default)
    {
        var mqttFactory = new MqttFactory();

        var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(configOptions.Value.ServerHostName, configOptions.Value.Port)
        .Build();


        mqttClient.ApplicationMessageReceivedAsync += action;

        await mqttClient.ConnectAsync(mqttClientOptions, token);

        var mqttSubscribeOptionsBuilder = mqttFactory.CreateSubscribeOptionsBuilder();
        subscribeConfig(mqttSubscribeOptionsBuilder);
        var mqttSubscribeOptions = mqttSubscribeOptionsBuilder.Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, token);

        return mqttClient;
    }

    public async Task Publish(Func<MqttApplicationMessageBuilder, MqttApplicationMessage> action, CancellationToken cancellationToken = default)
    {
        var mqttFactory = new MqttFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(configOptions.Value.ServerHostName, configOptions.Value.Port)
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

        var applicationMessage = action(new MqttApplicationMessageBuilder());

        await mqttClient.PublishAsync(applicationMessage, cancellationToken);

        await mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
    }
}

public class MqttConfig
{
    public string ServerHostName { get; set; } = string.Empty;

    public int? Port { get; set; } = 1883;

    public MqttTopics Topics { get; set; } = new();
}

public class MqttTopics
{
    public string Dimmer { get; set; } = "zigbee2mqtt/LedController";
    public string DimmerSet { get; set; } = "zigbee2mqtt/LedController/set";
    public string PowerSupplySet { get; set; } = "zigbee2mqtt/KitchenLedLightSwitch/set";
}
