using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading.Tasks;

namespace TaskTriggerMQTT.Server.Services
{
    public class RabbitMQService
    {
        private IMqttClient _mqttClient;
        private readonly string _hostname = "localhost";
        private readonly int _port = 1883; 
        private readonly string _commandsTopic = "ESP32Client/inbox";
        private readonly string _responsesTopic = "ESP32Client/outbox";

        public RabbitMQService()
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
        }

        public async Task ConnectAsync()
        {
            if (!_mqttClient.IsConnected)
            {
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_hostname, _port)
                    .Build();

                try
                {
                    await _mqttClient.ConnectAsync(options, CancellationToken.None);
                    Console.WriteLine("Connected to MQTT broker.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
                }
            }
        }

        public async Task SendMessageAsync(string message)
        {

            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(_commandsTopic) 
                .WithPayload(Encoding.UTF8.GetBytes(message))
                .Build();

            await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);

            Console.WriteLine($" [x] Sent {message}");
        }

        public async Task<string> WaitForResponseAsync(CancellationToken cancellationToken)
        {

            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            string responsePayload = null;
            var tcs = new TaskCompletionSource<string>();

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                if (topic == _responsesTopic)
                {
                    responsePayload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                    Console.WriteLine($"Received response: {responsePayload}");

                    // Поскольку мы получили ответ, можно отписаться от топика
                    await _mqttClient.UnsubscribeAsync(_responsesTopic);

                    Console.WriteLine($" [x] Received message {responsePayload}. On topic {_responsesTopic}");
                    tcs.TrySetResult(responsePayload);
                }
            };

            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_responsesTopic).Build());

            try
            {
                return await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                return "The wait for a response was interrupted";
            }
            finally
            {
                await _mqttClient.UnsubscribeAsync(_responsesTopic);
            }
        }

        public async Task SendTestMessageAsync()
        {

            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            var message = "Test Message";
            await SendMessageAsync(message);
            Console.WriteLine($" [x] Sent {message}");
        }
    }
}
