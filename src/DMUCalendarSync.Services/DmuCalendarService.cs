using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DMUCalendarSync.Services.Models;
using DMUCalendarSync.Services.Models.JsonModels;
using HtmlAgilityPack;

namespace DMUCalendarSync.Services;

public interface IDmuCalendarService
{
    public Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate);
}

public class DmuCalendarService : IDmuCalendarService
{
    // assume that we have a fully configured client
    private readonly IDmuCalendarClient _dmuClient;

    public DmuCalendarService(IDmuCalendarClient client)
    {
        _dmuClient = client;
    }

    public async Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate)
    {
        const string studentCalendarBaseUrl = "https://my.dmu.ac.uk/campusm/sso/cal2/student_timetable";
        var studentCalendar = new UriBuilder(studentCalendarBaseUrl)
        {
            Query = $"start={startDate.ToUniversalTime():O}" +
                    $"&end={endDate.ToUniversalTime():O}"
        };
        var calendarRequest = await _dmuClient.GetHttpClient().GetAsync(studentCalendar.ToString());
        var calendarResponse = await calendarRequest.Content.ReadFromJsonAsync<CampusmCalendar>();
        return calendarResponse;
    }
}