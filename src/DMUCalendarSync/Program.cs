using DMUCalendarSync.Services;

namespace DMUCalendarSync;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var c = new DmuCalendar();
        Console.WriteLine("Username:");
        var username = Console.ReadLine();
        
        Console.WriteLine("Password:");
        var password = Console.ReadLine();

        if (username != null && password != null)
        {
            await c.SignIn(username, password);
            Console.WriteLine(c.GetCurrentUser()?.Email);
        }
    }
}