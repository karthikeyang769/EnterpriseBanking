using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedInfrastructure.Caching;

public static class RedisExtensions
{
    public static IServiceCollection AddCustomRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        }

        // If Redis connection is not specified or empty, fallback to distributed memory cache
        if (string.IsNullOrEmpty(redisConnectionString) || redisConnectionString.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDistributedMemoryCache();
            return services;
        }

        try
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "EnterpriseBank_";
            });
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
