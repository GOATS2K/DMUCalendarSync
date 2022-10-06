using System.Net;
using Azure.Identity;
using Microsoft.Graph;

namespace DMUCalendarSync.Services;

public interface IOutlookCalendarService
{
}

public class OutlookCalendarService : IOutlookCalendarService
{
    private GraphServiceClient _graphClient;
    private const string ClientId = "8d26a113-ee6a-4220-b833-5a375f9ff946";

    public void CreateGraphClient()
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
        return await _graphClient.Me.Request().GetAsync();
    }
    
    

}