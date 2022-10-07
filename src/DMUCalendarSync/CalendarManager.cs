using DMUCalendarSync.Services;
using DMUCalendarSync.Services.Models;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMUCalendarSync
{
    public interface ICalendarManager
    {
        public Task<CampusmCalendar?> GetDmuCalendar();
        public Task<Calendar> GetGoogleCalendar();

    }

    class CalendarManager : ICalendarManager
    {
        private readonly IDmuCalendarClient _dmuCalendarClient;
        public CalendarManager(IDmuCalendarClient dmuCalendarClient)
        {
            _dmuCalendarClient = dmuCalendarClient;
        }

        public async Task<CampusmCalendar?> GetDmuCalendar()
        {
            var username = Environment.GetEnvironmentVariable("DCS_DMU_USER");
            var password = Environment.GetEnvironmentVariable("DCS_DMU_PASS");

            if (username != null && password != null)
            {
                await _dmuCalendarClient.ConfigureClient(username, password);
                if (_dmuCalendarClient.GetCurrentUser != null)
                {
                    var calendarService = new DmuCalendarService(_dmuCalendarClient);
                    return await calendarService.GetCalendar(DateTime.Today, DateTime.Today.AddDays(7));
                }
                return null;
            }
            return null;
        }

        public async Task<Calendar> GetGoogleCalendar()
        {
            var calendarClient = await GoogleCalendarClient.ConfigureClient();
            var gcal = new GoogleCalendarService(calendarClient);
            return gcal.GetDCSCalendar();
        }
    }
}
