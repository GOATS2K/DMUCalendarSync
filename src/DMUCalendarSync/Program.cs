using DMUCalendarSync.Services;

namespace DMUCalendarSync;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var c = new DmuCalendar();
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