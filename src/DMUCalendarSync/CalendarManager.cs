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
        private readonly IMyDmuService _myDmuService;

        public CalendarManager(IMyDmuService myDmuService)
        {
            _myDmuService = myDmuService;
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
            var calendarClient = await GoogleCalendarClient.ConfigureClient();
            var gcal = new GoogleCalendarService(calendarClient);
            return gcal.GetDCSCalendar();
        }
    }
}
