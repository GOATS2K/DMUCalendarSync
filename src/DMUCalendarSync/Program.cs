using DMUCalendarSync.Database;
using DMUCalendarSync.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace DMUCalendarSync;

internal static class Program
{
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddScoped<ICalendarManager, CalendarManager>();
        services.AddScoped<IMyDmuService, MyDmuService>();
        services.AddDbContext<DcsDbContext>();
        // services.AddHttpClient<IDmuCalendarClient, DmuCalendarClient>();
        return services;
    }
    
    private static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();

        var calendarManager = serviceProvider.GetRequiredService<ICalendarManager>();
        await calendarManager.SyncToGoogleCalendar();

        // await GetDmuCalendar(serviceProvider);
    }
}