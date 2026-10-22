using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CustomerService.Domain.Repositories;
using CustomerService.Persistence.Repositories;

namespace CustomerService.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                               "Host=localhost;Database=EnterpriseBankDB;Username=postgres;Password=postgres";
        }

        services.AddDbContext<CustomerDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(CustomerDbContext).Assembly.FullName)));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
