using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMUCalendarSync.Services
{
    public static class GoogleCalendarClient
    {
        public static async Task<CalendarService> ConfigureClient()
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
                new[] { CalendarService.Scope.Calendar },
                "user",
                CancellationToken.None,
                new FileDataStore("DCS.GCal")
            ).WaitAsync(CancellationToken.None);

            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "DMUCalendarSync"
            });
        }
    }
}
