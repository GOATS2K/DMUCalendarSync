using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DMUCalendarSync.Database;
using DMUCalendarSync.Database.Models;
using DMUCalendarSync.Services.Models;
using DMUCalendarSync.Services.Models.JsonModels;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Authenticators;

namespace DMUCalendarSync.Services;

public class DmuAuthenticator : IAuthenticator
{
    private readonly DcsDbContext _context;
    private readonly string _password;
    private readonly string _username;


    public DmuAuthenticator(string username, string password, DcsDbContext context)
    {
        _username = username.ToLower();
        _password = password;
        _context = context;
    }

    public async ValueTask Authenticate(RestClient client, RestRequest request)
    {
        var cookieContainer = await ConfigureClient();

        // check that the cookies we need were allocated
        var myDmuCookies = cookieContainer.GetCookies(new Uri("https://my.dmu.ac.uk"));
        if (!myDmuCookies.Any())
            throw new ApplicationException("Invalid credentials, we didn't get the expected MyDMU cookies.");

        client.CookieContainer.Add(cookieContainer.GetAllCookies());
    }

    public async Task<CookieContainer> ConfigureClient()
    {
        // fetch user info from cache
        var user = _context.MyDmuUsers.FirstOrDefault(x => x.Username == _username);
        if (user != null)
        {
            var cookieSet = await GetCookieSet(user);
            if (cookieSet != null) return cookieSet;
        }

        // prepare httpclient for cookie fetching
        var cookieContainer = new CookieContainer();
        var client = new HttpClient(new HttpClientHandler
        {
            CookieContainer = cookieContainer
        });

        // Perform sign-in
        var loginSuccess = await SignInViaSsoPage(client);
        if (!loginSuccess) return cookieContainer;

        // Fetch user info
        var currentTime = (DateTimeOffset) DateTime.Now;
        var unixTime = currentTime.ToUnixTimeSeconds();
        var infoUri = new UriBuilder("https://my.dmu.ac.uk/campusm/sso/state")
        {
            Query = $"_={unixTime}"
        };
        var userInfoRequest = await client.GetAsync(infoUri.ToString());
        var userInfoResponse = await userInfoRequest.Content.ReadFromJsonAsync<CampusmUserInfo>();

        if (userInfoResponse == null) return cookieContainer;

        // create user if it didn't already exist
        if (user == null)
        {
            user = new MyDmuUser
            {
                Username = _username,
                Password = _password,
                FirstName = userInfoResponse.Firstname,
                Surname = userInfoResponse.Surname
            };
            _context.MyDmuUsers.Add(user);
            await _context.SaveChangesAsync();
        }

        await WriteCookieSet(user, cookieContainer);
        return cookieContainer;
    }

    private string? ParseReturnUrlFromHtml(Stream stream)
    {
        var responseDocument = new HtmlDocument();
        responseDocument.Load(stream);
        var scriptTag = responseDocument
            .DocumentNode
            .SelectSingleNode("//script")
            .InnerHtml;
        var redirectUrl = Regex.Match(scriptTag, @"(https\:\/\/.*)(?=\'\;)");
        if (redirectUrl.Captures.Count == 0) return null;
        return redirectUrl.Captures.Single().ToString();
    }

    private SamlResponse ParseResponseFromHtml(Stream stream)
    {
        var samlResponse = new SamlResponse();
        var responseDocument = new HtmlDocument();
        responseDocument.Load(stream);
        var formNode = responseDocument.DocumentNode.SelectSingleNode("//form");
        samlResponse.ResponseUrl = formNode.Attributes
            .Single(f => f.Name == "action")
            .Value;
        samlResponse.ResponseDocument = formNode.SelectSingleNode("//form/input[1]")
            .Attributes
            .Single(f => f.Name == "value")
            .Value;
        samlResponse.RelayState = formNode.SelectSingleNode("//form/input[2]")
            .Attributes
            .Single(f => f.Name == "value")
            .Value;
        return samlResponse;
    }

    private async Task<bool> SignInViaSsoPage(HttpClient client)
    {
        // Construct form fields for sign-in page
        var postContents = new Dictionary<string, string>
        {
            {"option", "credential"},
            {"Ecom_User_ID", _username},
            {"Ecom_Password", _password}
        };
        var formContents = new FormUrlEncodedContent(postContents);

        // Login via URL by CampusM page.
        const string studentLogonUrl =
            "https://my.dmu.ac.uk/cmauth/login/7269" +
            "?platform=web&orgCode=891&redirect=%2Fcampusm%2Fhome%23select-profile%2F7269";

        await client.GetAsync(studentLogonUrl);

        var loginRequest =
            await client.PostAsync("https://idpedir.dmu.ac.uk/nidp/saml2/sso?sid=0&sid=0&uiDestination=contentDiv",
                formContents);

        // Get response from login
        var loginResponse = await loginRequest.Content.ReadAsStreamAsync();
        var loginReturnUrl = ParseReturnUrlFromHtml(loginResponse);

        // Login failed for some reason at this point.
        if (loginReturnUrl == null) return false;

        // Parse redirect url from response
        var ssoResponse = await client.GetAsync(loginReturnUrl);
        var ssoResponseDocument = await ssoResponse.Content.ReadAsStreamAsync();

        // Get SAML Response and RelayState from incoming document
        var samlResponse = ParseResponseFromHtml(ssoResponseDocument);

        // Send SAML response back to target server
        var samlForm = new Dictionary<string, string>
        {
            {"SAMLResponse", samlResponse.ResponseDocument},
            {"RelayState", samlResponse.RelayState}
        };
        var myDmuSsoRequest =
            await client.PostAsync(samlResponse.ResponseUrl, new FormUrlEncodedContent(samlForm));


        if (myDmuSsoRequest == null) return false;
        return myDmuSsoRequest
            .RequestMessage!
            .RequestUri!
            .ToString().StartsWith("https://my.dmu.ac.uk/campusm/home");
    }

    private async Task WriteCookieSet(MyDmuUser user, CookieContainer cookieContainer)
    {
        var cookieSet = new MyDmuCookieSet
        {
            MyDmuUser = user,
            Cookies = new List<MyDmuCookie>()
        };

        foreach (Cookie cookie in cookieContainer.GetCookies(new Uri("https://my.dmu.ac.uk")))
            cookieSet.Cookies.Add(new MyDmuCookie
            {
                MyDmuUser = user,
                Domain = cookie.Domain,
                Name = cookie.Name,
                Value = cookie.Value,
                ExpiryTime = cookie.Expires
            });

        cookieSet.EarliestCookieExpiry = cookieSet.Cookies.Min(c => c.ExpiryTime);
        _context.MyDmuCookieSets.Add(cookieSet);
        await _context.SaveChangesAsync();
    }

    private async Task<CookieContainer?> GetCookieSet(MyDmuUser user)
    {
        var cookieSet = await _context.MyDmuCookieSets
            .Include(c => c.Cookies)
            .FirstOrDefaultAsync(cs => cs.MyDmuUser.Id == user.Id && cs.EarliestCookieExpiry > DateTime.UtcNow);

        if (cookieSet != null)
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookie in cookieSet.Cookies)
                cookieContainer.Add(new Cookie
                {
                    Domain = cookie.Domain,
                    Expires = cookie.ExpiryTime,
                    Name = cookie.Name,
                    Value = cookie.Value
                });
            return cookieContainer;
        }

        return null;
    }
}