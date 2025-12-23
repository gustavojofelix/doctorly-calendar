using Calendar.Application.Abstractions;
using Calendar.Application.Services;
using Calendar.Infrastructure.Notifications;
using Calendar.Infrastructure.Persistence;
using Calendar.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string sqliteConnectionString
    )
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(sqliteConnectionString));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<INotificationService, ConsoleNotificationService>();

        services.AddScoped<EventService>();
        return services;
    }
}
