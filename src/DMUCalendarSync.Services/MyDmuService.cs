using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DMUCalendarSync.Database;
using DMUCalendarSync.Services.Models;
using DMUCalendarSync.Services.Models.JsonModels;
using HtmlAgilityPack;
using RestSharp;

namespace DMUCalendarSync.Services;

public interface IMyDmuService
{
    public Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate);
    public Task<CampusmUserInfo?> GetUser();
    public void SetCredentials(string username, string password);
}

public class MyDmuService : IMyDmuService
{
    // assume that we have a fully configured client
    private RestClient _restClient;
    private readonly DcsDbContext _context;
    
    public MyDmuService(DcsDbContext context)
    {
        _context = context;
        var options = new RestClientOptions("https://my.dmu.ac.uk/campusm/sso");
        _restClient = new RestClient(options);
    }

    public void SetCredentials(string username, string password)
    {
        _restClient.Authenticator = new DmuAuthenticator(username, password, _context);
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