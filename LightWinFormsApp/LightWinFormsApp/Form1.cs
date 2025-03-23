using MQTTnet;
using System.Text;

namespace LightWinFormsApp
{
    public partial class Form1 : Form
    {
        private static IMqttClient mqttClient;
        private static bool isLightOn = false;
        private const string mqttBroker = "192.168.0.194"; // Change to your broker's IP
        private const string topicCommand = "home/lightbulb/set";
        private const string topicState = "home/lightbulb/state";

        public Form1()
        {
            InitializeComponent();

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
            mqttClient.ConnectAsync(options);
        }

        private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Console.WriteLine($"Received command: {payload}");
            if (payload.Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                isLightOn = true;
                SetLightState(isLightOn);
            }
            else if (payload.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            {
                isLightOn = false;
                SetLightState(isLightOn);
            }
            Console.WriteLine($"Light is now {(isLightOn ? "ON" : "OFF")}");
            await PublishState();
        }
        private async Task PublishState()
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

        private void SetLightState(bool isOn)
        {
            string imagePath = Path.Combine(Application.StartupPath, "Content", isOn ? "on.png" : "off.png");
            this.Invoke((MethodInvoker)delegate
            {
                this.pictureBox1.Image = Image.FromFile(imagePath);
            });
        }
    }
}
