using DMUCalendarSync.Services;
using DMUCalendarSync.Services.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DMUCalendarSync;

internal static class Program
{
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddHttpClient<IDmuCalendarService, DmuCalendarService>();
        services.AddHttpClient<IGoogleCalendarService, GoogleCalendarService>();
        services.AddHttpClient<IOutlookCalendarService, OutlookCalendarService>();
        return services;
    }
    private static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();
        
        var c = serviceProvider.GetRequiredService<IDmuCalendarService>();
        var username = Environment.GetEnvironmentVariable("DC_USER");
        var password = Environment.GetEnvironmentVariable("DC_PASS");

        if (username != null && password != null)
        {
            await c.SignIn(username, password);
            Console.WriteLine(c.GetCurrentUser()?.Email);
            var cal = await c.GetCalendar(DateTime.Today, DateTime.Today.AddDays(7));
        }
    }
}