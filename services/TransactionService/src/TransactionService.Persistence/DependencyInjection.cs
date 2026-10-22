using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Domain.Repositories;
using TransactionService.Persistence.Repositories;

namespace TransactionService.Persistence;

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

        services.AddDbContext<TransactionDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(TransactionDbContext).Assembly.FullName)));

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
