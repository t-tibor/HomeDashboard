using HomeDashboard.Web.MQTT;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace HomeDashboard.Web.Services;

public class LightControlService : ILightControlService, IDisposable
{
    private readonly IMqttConnector _mqttConnector;
    private readonly MqttConfig _mqttConfig;
    private readonly ILogger<LightControlService> _logger;

    private Task? _backgroundTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public LightControlService(
        IMqttConnector mqttConnector,
        IOptions<MqttConfig> mqttConfig,
        ILogger<LightControlService> logger)
    {
        _mqttConnector = mqttConnector;
        _mqttConfig = mqttConfig.Value;
        _logger = logger;
    }

    private bool IsBackgroundTaskRunning =>
        _backgroundTask != null && !_backgroundTask.IsCompleted;

    public async Task StartTurnOnAsync(ushort targetBrightness)
    {
        if (IsBackgroundTaskRunning)
        {
            _logger.LogWarning("Cannot start turn on task - background task is already running");
            return;
        }

        if (_backgroundTask != null)
        {
            try
            {
                await _backgroundTask; // wait for any previous task to complete
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in previous background task");
            }
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _backgroundTask = TurnOnJobAsync(targetBrightness, _cancellationTokenSource.Token);
    }

    public async Task StartTurnOffAsync()
    {
        if (IsBackgroundTaskRunning)
        {
            _logger.LogWarning("Cannot start turn off task - background task is already running");
            return;
        }

        if (_backgroundTask != null)
        {
            try
            {
                await _backgroundTask; // wait for any previous task to complete
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in previous background task");
            }
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _backgroundTask = TurnOffJobAsync(_cancellationTokenSource.Token);
    }

    private async Task TurnOnJobAsync(ushort targetBrightness, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting turn on job with brightness {Brightness}", targetBrightness);

            // switch on the led power supply
            await _mqttConnector.Publish(x => x
                .WithTopic(_mqttConfig.Topics.PowerSupplySet)
                .WithPayload("{ \"state\": \"ON\"}")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build(),
                cancellationToken
            );

            using var responseCts = new CancellationTokenSource();
            // subscribe to the led controller topic
            using var subs = await _mqttConnector.SubscribeAsync(
                conf => conf.WithTopicFilter(_mqttConfig.Topics.Dimmer),
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
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseCts.Token, timeoutCts.Token);

            // periodically send out a ON command to the led controller until we get a response or the timeout is reached
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await _mqttConnector.Publish(x => x
                        .WithTopic(_mqttConfig.Topics.DimmerSet)
                        .WithPayload($"{{\"state\": \"ON\", \"brightness\": {targetBrightness}}}")
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                        .Build(), cts.Token);

                    await Task.Delay(300, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // timeout or brightness reached
                _logger.LogInformation("Turn on job completed - brightness {Brightness} reached or timed out", targetBrightness);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in turn on job");
            throw;
        }
    }

    private async Task TurnOffJobAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting turn off job");

            await _mqttConnector.Publish(x => x
                .WithTopic(_mqttConfig.Topics.DimmerSet)
                .WithPayload("{\"state\": \"OFF\", \"brightness\": 0}")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build(),
                cancellationToken
            );

            await Task.Delay(2000, cancellationToken);

            await _mqttConnector.Publish(x => x
                .WithTopic(_mqttConfig.Topics.PowerSupplySet)
                .WithPayload("{ \"state\": \"OFF\"}")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build(),
                cancellationToken
            );

            _logger.LogInformation("Turn off job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in turn off job");
            throw;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}