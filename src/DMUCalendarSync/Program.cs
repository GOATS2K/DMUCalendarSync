using DMUCalendarSync.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DMUCalendarSync;

internal static class Program
{
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddHttpClient<IDmuCalendarClient, DmuCalendarClient>();
        services.AddScoped<ICalendarManager, CalendarManager>();
        return services;
    }
    
    private static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();

        var calendarManager = serviceProvider.GetRequiredService<ICalendarManager>();
        var dmuCal = await calendarManager.GetDmuCalendar();

        // await GetDmuCalendar(serviceProvider);
    }
}