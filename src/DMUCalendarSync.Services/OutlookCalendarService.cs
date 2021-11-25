using System.Net;

namespace DMUCalendarSync.Services;

public interface IOutlookCalendarService
{
}

public class OutlookCalendarService : IOutlookCalendarService
{
    private readonly HttpClient _client;

    public OutlookCalendarService(HttpClient client)
    {
        _client = client;
    }
}