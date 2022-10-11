using dtmi_com_example_devicetemplate;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

namespace mqtt_device;

public class Device : BackgroundService
{
    private Idevicetemplate? client;

    private const int default_interval = 5;

    private readonly ILogger<Device> _logger;
    private readonly ClientFactory _clientFactory;

    public Device(ILogger<Device> logger, ClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client = await _clientFactory.CreateDeviceTemplateClientAsync(stoppingToken);
        
        client.Property_interval.OnMessage = Property_interval_UpdateHandler;
        client.Command_echo.OnMessage = Cmd_echo_Handler;

        await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);
        await client.Property_sdkInfo.SendMessageAsync(ClientFactory.NuGetPackageVersion, stoppingToken);

        double lastTemp = 21;
        while (!stoppingToken.IsCancellationRequested)
        {
            lastTemp = GenerateSensorReading(lastTemp, 12, 45);
            await client!.Telemetry_temp.SendMessageAsync(lastTemp, stoppingToken);
            _logger.LogInformation("Waiting {interval} s to send telemetry", client.Property_interval.Value);
            await Task.Delay(client.Property_interval.Value * 1000, stoppingToken);
        }
    }

    private async Task<Ack<int>> Property_interval_UpdateHandler(int p)
    {
        ArgumentNullException.ThrowIfNull(client);
        _logger.LogInformation("New prop 'interval' received: {p}", p.ToString());
        var ack = new Ack<int>();
        if (p > 0)
        {
            client.Property_interval.Value = p;
            ack.Description = "desired notification accepted";
            ack.Status = 200;
            ack.Version = client.Property_interval.Version!.Value;
            ack.Value = p;
        }
        else
        {
            ack.Description = "negative values not accepted";
            ack.Status = 405;
            ack.Version = client.Property_interval.Version!.Value;
            ack.Value = client.Property_interval.Value;
        };
        return await Task.FromResult(ack);
    }

    private async Task<string> Cmd_echo_Handler(string req)
    {
        _logger.LogInformation("Command echo received: {req}", req);
        return await Task.FromResult(req + req);
    }

    private readonly Random random = new();

    private double GenerateSensorReading(double currentValue, double min, double max)
    {
        double percentage = 15;
        double value = currentValue * (1 + (percentage / 100 * (2 * random.NextDouble() - 1)));
        value = Math.Max(value, min);
        value = Math.Min(value, max);
        return value;
    }
}
