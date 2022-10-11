using dtmi_com_example_devicetemplate;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace mqtt_device;
public class ClientFactory
{
    static string ComputeDeviceKey(string masterKey, string deviceId) =>
        Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));

    internal static string? NuGetPackageVersion { get; set; }
    internal static ConnectionSettings computedSettings = new()
        ;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public ClientFactory(IConfiguration configuration, ILogger<ClientFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;

    }

    public async Task<Idevicetemplate> CreateDeviceTemplateClientAsync(CancellationToken cancellationToken = default)
    {
        Idevicetemplate devicetemplateClient;
        string connectionString = _configuration.GetConnectionString("cs");
        var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));
        _logger.LogWarning("Connecting to .. {cs}", cs);
        if (connectionString.Contains("IdScope") || connectionString.Contains("SharedAccessKey"))
        {
            if (cs.IdScope != null && _configuration["masterKey"] != null)
            {
                var deviceId = Environment.MachineName;
                var masterKey = _configuration.GetValue<string>("masterKey");
                var deviceKey = ComputeDeviceKey(masterKey, deviceId);
                var newCs = $"IdScope={cs.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={cs.SasMinutes}";
                devicetemplateClient =  await CreateHubClientAsync(newCs, cancellationToken);
            }
            else
            {
                devicetemplateClient =  await CreateHubClientAsync(connectionString, cancellationToken);
            }
        }
        else
        {
            devicetemplateClient =  await CreateBrokerClientAsync(connectionString, cancellationToken);
        }
        _logger.LogWarning("Connected to {settings}", ClientFactory.computedSettings);
        return devicetemplateClient;
    }

    private static async Task<dtmi_com_example_devicetemplate.mqtt.devicetemplate> CreateBrokerClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = new ConnectionSettings(connectionString) { ModelId = Idevicetemplate.ModelId };
        var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, cancellationToken);
        var client = new dtmi_com_example_devicetemplate.mqtt.devicetemplate(mqtt);
        computedSettings = cs;
        NuGetPackageVersion = BrokerClientFactory.NuGetPackageVersion;
        return client;
    }

    private static async Task<dtmi_com_example_devicetemplate.hub.devicetemplate> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = new ConnectionSettings(connectionString) { ModelId = Idevicetemplate.ModelId };
        var hub = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs, cancellationToken);
        var client = new dtmi_com_example_devicetemplate.hub.devicetemplate(hub);
        computedSettings = HubDpsFactory.ComputedSettings!;
        NuGetPackageVersion = HubDpsFactory.NuGetPackageVersion;
        return client;
    }
}