using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace DMUCalendarSync.Services;

public class GoogleCalendarService
{
    private CalendarService _calendarService;

    public async Task SignIn()
    {
        var clientId = Environment.GetEnvironmentVariable("DCS_GAPP_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("DCS_GAPP_CLIENT_SECRET");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new ApplicationException("Unable to sign in to Google. Missing Client ID and secret.");
        }
        
        var userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets()
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            new[] {CalendarService.Scope.Calendar},
            "user",
            CancellationToken.None,
            new FileDataStore("DCS.GCal")
        ).WaitAsync(CancellationToken.None);

        _calendarService = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = userCredential,
            ApplicationName = "DMUCalendarSync"
        });
    }

    public CalendarList GetCalendarList()
    {
        var calendars = _calendarService.CalendarList.List().Execute();
        if (!calendars.Items.Any())
        {
            throw new ApplicationException("No calendars found.");
        }

        return calendars;
    }

    public Calendar GetDCSCalendar()
    {
        var targetCalendarEntry = GetCalendarList().Items.SingleOrDefault(c => c.Summary == "DMUCalendarSync");
        if (targetCalendarEntry == null)
        {
            var calendarData = new Calendar()
            {
                Summary = "DMUCalendarSync",
                Location = "Leicester, United Kingdom",
                Description = "This calendar is managed by DMUCalendarSync. " +
                              "Any changes done will be overwritten by DCS.",
                TimeZone = "Europe/London",
            };

            return _calendarService.Calendars.Insert(calendarData).Execute();
        }

        return _calendarService.Calendars.Get(targetCalendarEntry.Id).Execute();
    } 
}