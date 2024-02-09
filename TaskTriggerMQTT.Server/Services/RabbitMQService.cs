using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace TaskTriggerMQTT.Server.Services
{
    public class RabbitMQService
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "taskQueue";
        private readonly string _username = "guest";
        private readonly string _password = "guest";

        public void SendMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
                
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);

            Console.WriteLine($" [x] Sent {message}");
        }

        public void SendTestMessage()
        {
            var message = "Test Message";
            SendMessage(message);
            Console.WriteLine($" [x] Sent {message}");
        }


    }
}
