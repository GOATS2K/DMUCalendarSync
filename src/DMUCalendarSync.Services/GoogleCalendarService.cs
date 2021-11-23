using System.Net;

namespace DMUCalendarSync.Services;

public interface IGoogleCalendarService
{
}

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly HttpClient _client;

    public GoogleCalendarService(HttpClient client)
    {
        _client = client;
    }
}