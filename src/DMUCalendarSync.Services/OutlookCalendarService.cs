using System.Net;
using Azure.Identity;
using Microsoft.Graph;

namespace DMUCalendarSync.Services;

public interface IOutlookCalendarService
{
    public void SignIn();
    public Task<User> GetUserInfo();
}

// This will not work with a DMU account as we don't have permissions to read a users calendar.
// On hold for now.
public class OutlookCalendarService : IOutlookCalendarService
{
    private GraphServiceClient? _graphClient;
    private User? _user;
    private const string ClientId = "8d26a113-ee6a-4220-b833-5a375f9ff946";

    public void SignIn()
    {
        var scopes = new[] { 
            "User.Read",
            "User.ReadBasic.All",
        };

        // Multi-tenant apps can use "common",
        // single-tenant apps must use the tenant ID from the Azure portal
        var tenantId = "common";
        
        // using Azure.Identity;
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = tenantId,
            ClientId = ClientId,
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            RedirectUri = new Uri("http://localhost"),
        };

        // https://learn.microsoft.com/dotnet/api/azure.identity.interactivebrowsercredential
        var interactiveCredential = new InteractiveBrowserCredential(options);
        _graphClient = new GraphServiceClient(interactiveCredential, scopes);
    }

    public async Task<User> GetUserInfo()
    {
        return _user ?? (_user = await _graphClient.Me.Request().GetAsync());
    }

    public async Task<IUserCalendarsCollectionPage> GetCalendarList()
    {
        return await _graphClient!.Me.Calendars.Request().GetAsync();
    }

    public async Task<Calendar> GetCalendar(string calendarId)
    {
        return await _graphClient!.Me.Calendars[calendarId].Request().GetAsync();
    }
}