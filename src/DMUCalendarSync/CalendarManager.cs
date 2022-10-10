using System.Security.Cryptography;
using System.Text;
using DMUCalendarSync.Services;
using DMUCalendarSync.Services.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;

namespace DMUCalendarSync
{
    public interface ICalendarManager
    {
        public Task<CampusmCalendar?> GetDmuCalendar();
        public Task<Calendar> GetGoogleCalendar();
        public string GenerateHashForEvent(CalendarEvent calendarEvent);
        public Task SyncToGoogleCalendar();
    }

    class CalendarManager : ICalendarManager
    {
        private readonly IMyDmuService _myDmuService;

        public CalendarManager(IMyDmuService myDmuService)
        {
            _myDmuService = myDmuService;
        }

        public async Task SyncToGoogleCalendar()
        {
            var dmuCalendar = await GetDmuCalendar();
            if (dmuCalendar == null)
            {
                return;
            }
            
            var gCalService = await GetGoogleCalendarService();
            foreach (var calendarEvent in dmuCalendar.Events)
            {
                var eventHash = GenerateHashForEvent(calendarEvent);
                var syncedEvent = await gCalService.CreateCalendarEntry(new Event()
                {
                    Start = new EventDateTime()
                    {
                        DateTime = calendarEvent.Start
                    },
                    End = new EventDateTime()
                    {
                        DateTime = calendarEvent.End
                    },
                    Summary = calendarEvent.Desc1,
                    Location = calendarEvent.LocAdd1,
                    Description = eventHash
                });
                Console.WriteLine($"Synced event: {calendarEvent.Desc1}" +
                                  $" at {calendarEvent.Start}" +
                                  $" with GCal ID {syncedEvent.Id}");
            }
        }

        public async Task<CampusmCalendar?> GetDmuCalendar()
        {
            var username = Environment.GetEnvironmentVariable("DCS_DMU_USER");
            var password = Environment.GetEnvironmentVariable("DCS_DMU_PASS");

            if (username != null && password != null)
            {
                _myDmuService.SetCredentials(username, password);
                var user = await _myDmuService.GetUser();

                Console.WriteLine($"Getting calendar data for: {user?.Email}");
                return await _myDmuService.GetCalendar(DateTime.Today, DateTime.Today.AddDays(7));
            }
            return null;
        }

        public async Task<Calendar> GetGoogleCalendar()
        {
            var gcal = await GetGoogleCalendarService();
            return gcal.GetDCSCalendar();
        }

        private static async Task<GoogleCalendarService> GetGoogleCalendarService()
        {
            var calendarClient = await GoogleCalendarClient.ConfigureClient();
            var gcal = new GoogleCalendarService(calendarClient);
            return gcal;
        }

        public string GenerateHashForEvent(CalendarEvent calendarEvent)
        {
            var uniqueCalendarString = calendarEvent.ToString();
            
            using var hasher = SHA256.Create();
            var hashedBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(uniqueCalendarString));

            return Convert.ToHexString(hashedBytes);
        }
    }
}
