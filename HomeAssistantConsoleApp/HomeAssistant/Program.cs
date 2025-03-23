using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;

class Program
{
    private static IMqttClient mqttClient;
    private static bool isLightOn = false;
    private const string mqttBroker = "192.168.0.194"; // Change to your broker's IP
    private const string topicCommand = "home/lightbulb/set";
    private const string topicState = "home/lightbulb/state";
    static async Task Main()
    {
        var factory = new MqttClientFactory();
        mqttClient = factory.CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttBroker)
            .WithCredentials("denys", "YOUR_PASSWORD") // Add if required
            .WithCleanSession()
            .Build();
        mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
        mqttClient.ConnectedAsync += async e =>
        {
            Console.WriteLine("Connected to MQTT broker.");
            await mqttClient.SubscribeAsync(topicCommand);
        };
        await mqttClient.ConnectAsync(options);
        Console.WriteLine("Light bulb simulation started. Waiting for commands...");
        // Keep the app running
        await Task.Delay(-1);
    }
    private static async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        Console.WriteLine($"Received command: {payload}");
        if (payload.Equals("ON", StringComparison.OrdinalIgnoreCase))
        {
            isLightOn = true;
        }
        else if (payload.Equals("OFF", StringComparison.OrdinalIgnoreCase))
        {
            isLightOn = false;
        }
        Console.WriteLine($"Light is now {(isLightOn ? "ON" : "OFF")}");
        await PublishState();
    }
    private static async Task PublishState()
    {
        if (mqttClient != null && mqttClient.IsConnected)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topicState)
                .WithPayload(isLightOn ? "ON" : "OFF")
                .Build();
            await mqttClient.PublishAsync(message);
            Console.WriteLine("Published state: " + (isLightOn ? "ON" : "OFF"));
        }
    }
}