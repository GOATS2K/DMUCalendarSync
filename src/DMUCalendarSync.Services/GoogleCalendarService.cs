using System.Net;
using DMUCalendarSync.Services.Models;

namespace DMUCalendarSync.Services;

public interface IGoogleCalendarService
{
    public Task GetCalendar();
    public Task<bool> SignIn();
    public UserInfo? GetCurrentUser();

}

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly HttpClient _client;
    private readonly UserInfo? _currentUser;

    public GoogleCalendarService(HttpClient client)
    {
        _currentUser = null;
        _client = client;
    }

    public async Task GetCalendar()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SignIn()
    {
        throw new NotImplementedException();
    }

    public UserInfo? GetCurrentUser() => _currentUser;
}