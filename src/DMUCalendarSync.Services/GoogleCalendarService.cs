using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace DMUCalendarSync.Services;

public class GoogleCalendarService
{
    private readonly CalendarService _calendarService;

    public GoogleCalendarService(CalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    private CalendarList GetCalendarList()
    {
        var calendars = _calendarService.CalendarList.List().Execute();
        if (!calendars.Items.Any()) throw new ApplicationException("No calendars found.");

        return calendars;
    }

    public async Task<Event> UpdateEvent(Event newEvent, string oldEventId)
    {
        var calendar = GetDCSCalendar();
        return await _calendarService.Events.Update(newEvent, calendar.Id, oldEventId).ExecuteAsync();
    }

    public async Task<Events> GetEventsOnDay(DateTime date)
    {
        var calendar = GetDCSCalendar();
        var eventRequest = _calendarService.Events
            .List(calendar.Id);
        eventRequest.TimeMin = date;
        return await eventRequest.ExecuteAsync();
    }

    public async Task<Event> CreateCalendarEntry(Event calendarEvent)
    {
        var targetCalendar = GetDCSCalendar();
        var insertRequest = _calendarService.Events.Insert(calendarEvent, targetCalendar.Id);
        insertRequest.ConferenceDataVersion = 1;
        return await insertRequest.ExecuteAsync();
    }

    public Calendar GetDCSCalendar()
    {
        var targetCalendarEntry = GetCalendarList().Items.SingleOrDefault(c => c.Summary == "DMUCalendarSync");
        if (targetCalendarEntry == null)
        {
            var calendarData = new Calendar
            {
                Summary = "DMUCalendarSync",
                Location = "Leicester, United Kingdom",
                Description = "This calendar is managed by DMUCalendarSync. " +
                              "Any changes done will be overwritten by DCS.",
                TimeZone = "Europe/London"
            };

            return _calendarService.Calendars.Insert(calendarData).Execute();
        }

        return _calendarService.Calendars.Get(targetCalendarEntry.Id).Execute();
    }
}