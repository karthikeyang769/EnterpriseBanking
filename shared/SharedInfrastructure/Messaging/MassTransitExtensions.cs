using System;
using System.Net.Sockets;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedInfrastructure.Messaging;

public static class MassTransitExtensions
{
    /// <summary>
    /// Checks whether RabbitMQ is reachable on the given host and port within the timeout.
    /// Returns false if the connection fails (e.g. RabbitMQ not installed locally).
    /// </summary>
    private static bool IsRabbitMqReachable(string host, ushort port, int timeoutMs = 1000)
    {
        try
        {
            using var tcp = new TcpClient();
            var result = tcp.BeginConnect(host, port, null, null);
            var connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMs));
            if (!connected) return false;
            tcp.EndConnect(result);
            return true;
        }
        catch
        {
            return false;
        }  
        
    }

    public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration, Assembly? assemblyWithConsumers = null)
    {
        var rabbitHost = configuration["RabbitMQ:Host"];
        if (string.IsNullOrEmpty(rabbitHost))
        {
            rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        }

        var rabbitPortStr = configuration["RabbitMQ:Port"];
        if (string.IsNullOrEmpty(rabbitPortStr))
        {
            rabbitPortStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672";
        }

        var rabbitUser = configuration["RabbitMQ:Username"];
        if (string.IsNullOrEmpty(rabbitUser))
        {
            rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
        }

        var rabbitPass = configuration["RabbitMQ:Password"];
        if (string.IsNullOrEmpty(rabbitPass))
        {
            rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
        }

        ushort rabbitPort = 5672;
        ushort.TryParse(rabbitPortStr, out rabbitPort);

        // Probe RabbitMQ availability before binding. Fall back to InMemory when offline.
        var rabbitAvailable = IsRabbitMqReachable(rabbitHost, rabbitPort);

        services.AddMassTransit(x =>
        {
            if (assemblyWithConsumers != null)
            {
                x.AddConsumers(assemblyWithConsumers);
            }

            if (rabbitAvailable)
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost, rabbitPort, "/", h =>
                    {
                        h.Username(rabbitUser);
                        h.Password(rabbitPass);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                // RabbitMQ not reachable — use InMemory transport for local development
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
