using DMUCalendarSync.Database;
using DMUCalendarSync.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DMUCalendarSync;

internal static class Program
{
    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ApplicationArguments>();
        services.AddScoped<ICalendarManager, CalendarManager>();
        services.AddScoped<IMyDmuService, MyDmuService>();
        services.AddDbContext<DcsDbContext>();
        services.AddLogging(opt =>
        {
            opt.AddSimpleConsole(logger =>
            {
                logger.UseUtcTimestamp = true;
                logger.TimestampFormat = "[yyyy-mm-dd HH:mm:ss] ";
            });
            opt.AddFilter("Microsoft", LogLevel.Warning);
            opt.AddFilter("System", LogLevel.Warning);
            opt.AddConsole();
        });
        // services.AddHttpClient<IDmuCalendarClient, DmuCalendarClient>();
        return services;
    }

    private static async Task Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("usage: <dmu_user dmu_password google_app_client_id google_app_client_secret>");
            Environment.Exit(1);
        }

        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();
        var appArguments = serviceProvider.GetRequiredService<ApplicationArguments>();

        appArguments.DmuUsername = args[0];
        appArguments.DmuPassword = args[1];
        appArguments.GoogleAppClientId = args[2];
        appArguments.GoogleAppClientSecret = args[3];

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DcsDbContext>();
            await db.Database.EnsureCreatedAsync();
            // await db.Database.MigrateAsync();
        }

        var calendarManager = serviceProvider.GetRequiredService<ICalendarManager>();

        while (true)
        {
            await calendarManager.SyncToGoogleCalendar();
            Thread.Sleep(1 * 3600 * 1000);
        }
    }
}