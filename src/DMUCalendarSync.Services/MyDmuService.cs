using DMUCalendarSync.Database;
using DMUCalendarSync.Services.Models;
using DMUCalendarSync.Services.Models.JsonModels;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace DMUCalendarSync.Services;

public interface IMyDmuService
{
    public Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate);
    public Task<CampusmUserInfo?> GetUser();
    public void SetCredentials(string username, string password);
    public ParsedEventTitle ParseCalendarEventTitle(string eventDescription);
}

public class MyDmuService : IMyDmuService
{
    private readonly DcsDbContext _context;

    // assume that we have a fully configured client
    private readonly RestClient _restClient;

    private readonly ILogger<MyDmuService> _logger;

    public MyDmuService(DcsDbContext context, ILogger<MyDmuService> logger)
    {
        _context = context;
        var options = new RestClientOptions("https://my.dmu.ac.uk/campusm/sso");
        _restClient = new RestClient(options);
        _logger = logger;
    }

    public void SetCredentials(string username, string password)
    {
        _restClient.Authenticator = new DmuAuthenticator(username, password, _context);
    }

    public ParsedEventTitle ParseCalendarEventTitle(string eventDescription)
    {
        // example: Systems Building: Methods Seminar (IMAT3423-2022-501-S/02)
        var parsedEvent = new ParsedEventTitle();
        if (eventDescription.Contains(" (online) "))
        {
            eventDescription = eventDescription.Replace(" (online) ", "");
            parsedEvent.Online = true;
        }

        var parenthesesContents = eventDescription.Split('(', ')')[1];
        eventDescription = eventDescription.Replace(parenthesesContents, "");

        var splitContents = parenthesesContents.Split("-");

        parsedEvent.ModuleId = splitContents[0];
        parsedEvent.SessionCode = string.Join("-", splitContents[2..]);
        parsedEvent.ModuleName = eventDescription.Replace("()", "").Trim();

        return parsedEvent;
    }

    public async Task<CampusmUserInfo?> GetUser()
    {
        var currentTime = (DateTimeOffset)DateTime.Now;
        var unixTime = currentTime.ToUnixTimeSeconds();
        var restRequest = new RestRequest("/state")
            .AddQueryParameter("_", unixTime);

        var userInfoResponse = await _restClient.GetAsync<CampusmUserInfo>(restRequest);
        return userInfoResponse;
    }

    public async Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate)
    {
        const string studentCalendarBaseUrl = "/cal2/student_timetable";

        CampusmCalendar? calendar = null;
        try
        {
            var restRequest = new RestRequest(studentCalendarBaseUrl)
            .AddQueryParameter("start", $"{startDate.ToUniversalTime():O}")
            .AddQueryParameter("end", $"{endDate.ToUniversalTime():O}");
            calendar = await _restClient.GetAsync<CampusmCalendar>(restRequest);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to get calendar: {message}", ex.Message);
        }

        return calendar;
    }
}