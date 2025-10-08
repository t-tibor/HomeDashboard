namespace HomeDashboard.Web.MQTT;

public static class BuilderExtensions
{
	public static IHostApplicationBuilder AddMqttInfrastructure(this IHostApplicationBuilder builder)
	{
		builder.Services.Configure<MqttConfig>(builder.Configuration.GetSection(nameof(MqttConfig)));
		builder.Services.AddSingleton<IMqttConnector, MqttConnector>();
		return builder;
	}
}
