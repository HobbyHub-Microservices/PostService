using System.Text;
using System.Text.Json;
using PostService.DTOs;
using RabbitMQ.Client;

namespace PostService.AsyncDataServices;

public class MessageBusClient : IMesssageBusClient
{
    private readonly IConfiguration _config;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public MessageBusClient(IConfiguration config)
    {
        _config = config;
        var factory = new ConnectionFactory()
        {
            HostName = _config["RabbitMQHost"],
            Port = int.Parse(_config["RabbitMQPort"] ?? string.Empty),
            ClientProvidedName = "PostService"
        };
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: "post.topic", type: ExchangeType.Topic, durable: true);
            _connection.ConnectionShutdown += RabbitMq_ConnectionShutDown;
            Console.WriteLine("--> Connected to RabbitMQ");
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"--> Could not connect to message bus: {e.Message}");
        }
        
        
    }
    public void PublishNewPost(PostPublishedDTO userPublishedDto)
    {
        var message = JsonSerializer.Serialize(userPublishedDto);
        
        if (_connection.IsOpen)
        {
            Console.WriteLine($"--> Sending message to RabbitMQ: {message}");
            SendMessageToUser(message);
            SendMessageToHobby(message);
        }
        else
        {
            Console.WriteLine($"--> RabbitMQ is closed, not able to send message");
        }
    }
    
    private void SendMessageToUser(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: "post.topic",
            routingKey: "post.user",
            basicProperties: null,
            body: Encoding.UTF8.GetBytes(message));
        Console.WriteLine($"--> Sent message to RabbitMQ User: {message}");
    }
    
    private void SendMessageToHobby(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: "post.topic",
            routingKey: "post.hobby",
            basicProperties: null,
            body: Encoding.UTF8.GetBytes(message));
        Console.WriteLine($"--> Sent message to RabbitMQ Hobby: {message}");
    }

    private void Dispose()
    {
        Console.WriteLine("--> Disposing of RabbitMQ");
        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }
    }

    private static void RabbitMq_ConnectionShutDown(object? sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
    }
}