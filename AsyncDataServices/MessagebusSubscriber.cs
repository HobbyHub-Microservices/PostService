﻿using System.Text;
using PostService.EventProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PostService.AsyncDataServices;

public class MessagebusSubscriber : BackgroundService
{
      private readonly IConfiguration _configuration;
    private readonly IEventProcessor _eventProcessor;
    private IConnection _connection;
    private IModel _channel;
    private string _queueName;


    public MessagebusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
    {
        _configuration = configuration;
        _eventProcessor = eventProcessor;
        IntitializeRabbitMQ();
    }

    private void IntitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"],
            Port = int.Parse(_configuration["RabbitMQPort"] ?? string.Empty),
            ClientProvidedName = "PostCommandService",
            UserName = _configuration["RabbitMQUsername"],
            Password = _configuration["RabbitMQPassword"]
        };
            _connection = factory.CreateConnection();
            Console.WriteLine("--> RabbitMQ connection is established.");
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: "amq.topic", type: ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare(exchange: "post.topic", type: ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare(exchange: "user.command.topic", type: ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare(exchange: "hobby.command.topic", type: ExchangeType.Topic, durable: true);
            _queueName = _channel.QueueDeclare().QueueName;
            
            
            _channel.QueueBind(queue: _queueName, exchange: "amq.topic", routingKey: "KK.EVENT.*.HobbyHub.SUCCESS.#");
            _channel.QueueBind(queue: _queueName, exchange: "amq.topic", routingKey: "KK.EVENT.*.HobbyHub.ERROR.#");
            
            _channel.QueueBind(queue: _queueName, exchange: "hobby.command.topic", routingKey: "hobby.topic.*");
            _channel.QueueBind(queue: _queueName, exchange: "user.command.topic", routingKey: "user.topic.*");
            
            // // Binding to the user.topic exchange
            // _channel.ExchangeDeclare(exchange: "user.topic", type: ExchangeType.Topic, durable: false);
            // _channel.QueueBind(queue: _queueName, exchange: "user.topic", routingKey: "user.add");
            
            Console.WriteLine("--> Listening on the Message Bus");
            
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutDown;

    }




    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += (ModuleHandle, ea) =>
        {
            Console.WriteLine($"--> Received message: {ea.Body}");
            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
            
            Console.WriteLine($"--> Message encoded: {notificationMessage}");
            
            _eventProcessor.ProcessEvent(notificationMessage);
        };
        
        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        
        return Task.CompletedTask;
    }

    private void RabbitMQ_ConnectionShutDown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> Shutting down the RabbitMQ connection");
    }

    public override void Dispose()
    {
        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }
    }
}