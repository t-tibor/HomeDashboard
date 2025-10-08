using MQTTnet;
using MQTTnet.Client;

namespace HomeDashboard.Web.MQTT;

public interface IMqttConnector
{
	Task<IDisposable> SubscribeAsync(Action<MqttClientSubscribeOptionsBuilder> subscribeConfig,
		Func<MqttApplicationMessageReceivedEventArgs, Task> action,
		CancellationToken token = default);

	Task Publish(Func<MqttApplicationMessageBuilder, MqttApplicationMessage> action,
		CancellationToken cancellationToken = default);
}
