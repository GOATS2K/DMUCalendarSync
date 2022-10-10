using DMUCalendarSync.Services.Models.JsonModels;
using DMUCalendarSync.Services.Models;
using HtmlAgilityPack;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace DMUCalendarSync.Services
{
    public class DmuAuthenticator : IAuthenticator
    {
        public readonly string _username;
        public readonly string _password;

        public DmuAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public async Task<CookieContainer> ConfigureClient()
        {
            // prepare httpclient for cookie fetching
            var cookieContainer = new CookieContainer();
            var client = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            });

            // Perform sign-in
            var loginSuccess = await SignInViaSsoPage(client);
            if (!loginSuccess) return cookieContainer;

            // Fetch user info
            var currentTime = (DateTimeOffset)DateTime.Now;
            var unixTime = currentTime.ToUnixTimeSeconds();
            var infoUri = new UriBuilder("https://my.dmu.ac.uk/campusm/sso/state")
            {
                Query = $"_={unixTime}"
            };
            var userInfoRequest = await client.GetAsync(infoUri.ToString());
            var userInfoResponse = await userInfoRequest.Content.ReadFromJsonAsync<CampusmUserInfo>();

            if (userInfoResponse == null) return cookieContainer;

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

        public async ValueTask Authenticate(RestClient client, RestRequest request)
        {
            var cookieContainer = await ConfigureClient();

            // check that the cookies we need were allocated
            var myDmuCookies = cookieContainer.GetCookies(new Uri("https://my.dmu.ac.uk"));
            if (myDmuCookies.Count == 0)
            {
                throw new ApplicationException("Invalid credentials, we didn't get the expected MyDMU cookies.");
            }

            client.CookieContainer.Add(cookieContainer.GetAllCookies());
        }
    }
}
