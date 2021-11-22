using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DMUCalendarSync.Services.Models;
using DMUCalendarSync.Services.Models.JsonModels;
using HtmlAgilityPack;

namespace DMUCalendarSync.Services;

public interface IDmuCalendar
{
    public Task<bool> SignIn(string username, string password);
    public Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate);
    public UserInfo? GetCurrentUser();
}

public class DmuCalendar : IDmuCalendar
{
    private readonly HttpClient _client;
    private UserInfo? _currentUser;

    public DmuCalendar()
    {
        _client = new HttpClient();
        _currentUser = null;
    }

    public async Task<bool> SignIn(string username, string password)
    {
        // Perform sign-in
        var loginSuccess = await SignInViaSsoPage(username, password);
        if (!loginSuccess) return loginSuccess;

        // Fetch user info
        var currentTime = (DateTimeOffset) DateTime.Now;
        var unixTime = currentTime.ToUnixTimeSeconds();
        var infoUri = new UriBuilder("https://my.dmu.ac.uk/campusm/sso/state")
        {
            Query = $"_={unixTime}"
        };
        var userInfoRequest = await _client.GetAsync(infoUri.ToString());
        var userInfoResponse = await userInfoRequest.Content.ReadFromJsonAsync<CampusmUserInfo>();

        // Set user member variable
        _currentUser = new UserInfo
        {
            UserId = int.Parse(userInfoResponse.PersonId),
            FirstName = userInfoResponse.Firstname,
            Surname = userInfoResponse.Surname,
            Email = userInfoResponse.Email
        };
        return userInfoResponse.AuthUserInfo != null;
    }

    public async Task<CampusmCalendar?> GetCalendar(DateTime startDate, DateTime endDate)
    {
        const string studentCalendarBaseUrl = "https://my.dmu.ac.uk/campusm/sso/cal2/student_timetable";
        var studentCalendar = new UriBuilder(studentCalendarBaseUrl)
        {
            Query = $"start={startDate.ToUniversalTime():O}" +
                    $"&end={endDate.ToUniversalTime():O}"
        };
        var calendarRequest = await _client.GetAsync(studentCalendar.ToString());
        var calendarResponse = await calendarRequest.Content.ReadFromJsonAsync<CampusmCalendar>();
        return calendarResponse;
    }

    public UserInfo? GetCurrentUser()
    {
        return _currentUser;
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

    private async Task<bool> SignInViaSsoPage(string username, string password)
    {
        // Construct form fields for sign-in page
        var postContents = new Dictionary<string, string>
        {
            {"option", "credential"},
            {"Ecom_User_ID", username},
            {"Ecom_Password", password}
        };
        var formContents = new FormUrlEncodedContent(postContents);

        // Login via URL by CampusM page.
        const string studentLogonUrl =
            "https://my.dmu.ac.uk/cmauth/login/7269" +
            "?platform=web&orgCode=891&redirect=%2Fcampusm%2Fhome%23select-profile%2F7269";
        await _client.GetAsync(studentLogonUrl);
        var loginRequest =
            await _client.PostAsync("https://idpedir.dmu.ac.uk/nidp/saml2/sso?sid=0&sid=0&uiDestination=contentDiv",
                formContents);

        // Get response from login
        var loginResponse = await loginRequest.Content.ReadAsStreamAsync();
        var loginReturnUrl = ParseReturnUrlFromHtml(loginResponse);

        // Login failed for some reason at this point.
        if (loginReturnUrl == null) return false;

        // Parse redirect url from response
        var ssoResponse = await _client.GetAsync(loginReturnUrl);
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
            await _client.PostAsync(samlResponse.ResponseUrl, new FormUrlEncodedContent(samlForm));
        return myDmuSsoRequest
            .RequestMessage
            .RequestUri
            .ToString().StartsWith("https://my.dmu.ac.uk/campusm/home");
    }
}