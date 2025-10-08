using HomeDashboard.Web.MQTT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace HomeDashboard.Web.Pages;

public class IndexModel : PageModel
{
	private readonly ILogger<IndexModel> _logger;
	private readonly IMqttConnector _mqttConnector;

	public IndexModel(ILogger<IndexModel> logger, IMqttConnector mqttConnector)
	{
		_logger = logger;
		_mqttConnector = mqttConnector;
	}

	public Task<IActionResult> OnGetAsync()
	{
		return Task.FromResult<IActionResult>(Page());
	}

	public async Task<IActionResult> OnPostAsync(string mode)
	{
		if (string.IsNullOrEmpty(mode))
		{
			return Page();
		}

		switch (mode)
		{
			case "on":
				await TurnOn(255);
				break;
			case "mild":
				await TurnOn(26);
				break;
			case "off":
				await TurnOff();
				break;
			default:
				return NotFound("Invalid mode");
		}

		return Page();
	}

	private async Task TurnOn(ushort targetBrightness)
	{

		// switch on the led power supply
		await _mqttConnector.Publish(x => x
			.WithTopic("zigbee2mqtt/KitchenLedLightSwitch/set")
			.WithPayload("{ \"state\": \"ON\"}")
			.Build()
		);

		using var responseCts = new CancellationTokenSource();
		// subscribe to the led controller topic
		using var subs = await _mqttConnector.SubscribeAsync(
			conf => conf.WithTopicFilter("zigbee2mqtt/LedController"),
			rcvEvent =>
			{
				// check the incoming message, and cancel the timeout if the brightness is the one we want
				var raw = rcvEvent.ApplicationMessage;
				var msg = System.Text.Encoding.UTF8.GetString(raw.PayloadSegment);
				// deserialize the message string into a JObject
				if (msg == null)
				{
					return Task.CompletedTask;
				}
				var jmsg = JObject.Parse(msg);
				if (jmsg.TryGetValue("brightness", out var brightnessStr) &&
					int.TryParse(brightnessStr.Value<string>(), out var brightness) && 
					brightness == targetBrightness)
				{
					responseCts.Cancel();
				}

				return Task.CompletedTask;
			}, responseCts.Token);

		using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(responseCts.Token, timeoutCts.Token);

		// periodically send out a OFF command to the led controller until we get a response or the timeout is reached
		try
		{
			while (!cts.Token.IsCancellationRequested)
			{
				await _mqttConnector.Publish(x => x
					.WithTopic("zigbee2mqtt/LedController/set")
					.WithPayload($"{{\"state\": \"ON\", \"brightness\": {targetBrightness}}}")
					.Build(), cts.Token);

				await Task.Delay(300, cts.Token);
			}
		}
		catch (TaskCanceledException)
		{
			// timeout or brightness reached
		}
	}

	public async Task TurnOff()
	{
		await _mqttConnector.Publish(x => x
			.WithTopic("zigbee2mqtt/LedController/set")
			.WithPayload("{\"state\": \"OFF\", \"brightness\": 0}")
			.Build()
		);

		await Task.Delay(2000);

		await _mqttConnector.Publish(x => x
			.WithTopic("zigbee2mqtt/KitchenLedLightSwitch/set")
			.WithPayload("{ \"state\": \"OFF\"}")
			.Build()
		);
	}
}
