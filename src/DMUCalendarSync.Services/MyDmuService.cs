using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DMUCalendarSync.Services.Models;
using DMUCalendarSync.Services.Models.JsonModels;
using HtmlAgilityPack;
using RestSharp;

namespace DMUCalendarSync.Services;

public interface IDmuCalendarService
{
    public Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate);
}

public class MyDmuService : IDmuCalendarService
{
    // assume that we have a fully configured client
    private readonly RestClient _restClient;

    public MyDmuService(string username, string password)
    {
        var options = new RestClientOptions("https://my.dmu.ac.uk/campusm/sso");
        _restClient = new RestClient(options)
        {
            Authenticator = new DmuAuthenticator(username, password)
        };
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

        var restRequest = new RestRequest(studentCalendarBaseUrl, Method.Get)
            .AddQueryParameter("start", $"{startDate.ToUniversalTime():O}")
            .AddQueryParameter("end", $"{endDate.ToUniversalTime():O}");

        var calendarResponse = await _restClient.GetAsync<CampusmCalendar>(restRequest);
        return calendarResponse;
    }
}