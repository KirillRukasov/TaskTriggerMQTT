using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace TaskTriggerMQTT.Server.Services
{
    public class RabbitMQService
    {
        private readonly string _hostname = "localhost";
        private readonly int _port = 1883; 
        private readonly string _topic = "mqtt_Queue"; 

        public async Task SendMessageAsync(string message)
        {
            var factory = new MqttFactory();
            using var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_hostname, _port)
                .Build();

            await mqttClient.ConnectAsync(options, CancellationToken.None);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(_topic) 
                .WithPayload(Encoding.UTF8.GetBytes(message))
                .Build();

            await mqttClient.PublishAsync(mqttMessage, CancellationToken.None);

            Console.WriteLine($" [x] Sent {message}");
        }

        public async Task SendTestMessageAsync()
        {
            var message = "Test Message";
            await SendMessageAsync(message);
            Console.WriteLine($" [x] Sent {message}");
        }
    }
}
