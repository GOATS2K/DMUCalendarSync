﻿using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DMUCalendarSync.Services;
using DMUCalendarSync.Services.Models;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;

namespace DMUCalendarSync;

public interface ICalendarManager
{
    public Task SyncToGoogleCalendar();
}

internal class CalendarManager : ICalendarManager
{
    private readonly IMyDmuService _myDmuService;
    private readonly ApplicationArguments _applicationArguments;
    private readonly ILogger<CalendarManager> _logger;

    public CalendarManager(IMyDmuService myDmuService, ApplicationArguments applicationArguments, ILogger<CalendarManager> logger)
    {
        _myDmuService = myDmuService;
        _applicationArguments = applicationArguments;
        _logger = logger;
    }

    public async Task SyncToGoogleCalendar()
    {
        _logger.LogInformation("Fetching latest data...");
        
        var dmuCalendar = await GetDmuCalendar();
        if (dmuCalendar == null) return;
        var gCalService = await GetGoogleCalendarService();
        foreach (var calendarEvent in dmuCalendar.Events)
        {
            var generatedEvent = CreateGCalEvent(calendarEvent);
            // TODO: get events for the whole week and iterate on that instead
            var gCalEvents = await gCalService.GetEventsOnDay(calendarEvent.Start.Date);
            var existingEvent = gCalEvents.Items.FirstOrDefault(e =>
                e.Summary == generatedEvent.Summary
                && e.Start.DateTime == generatedEvent.Start.DateTime
                && e.End.DateTime == generatedEvent.End.DateTime);

            if (existingEvent == null)
            {
                var syncedEvent = await gCalService.CreateCalendarEntry(generatedEvent);
                _logger.LogInformation($"Synced event: {generatedEvent.Summary}" +
                                  $" at {calendarEvent.Start}" +
                                  $" with GCal ID {syncedEvent.Id}");
            }
            else
            {
                if (ExistingEventHasChanged(existingEvent, calendarEvent))
                {
                    var updatedEvent = await gCalService.UpdateEvent(generatedEvent, existingEvent.Id);
                    _logger.LogInformation($"Updated event: {generatedEvent.Summary}" +
                                      $" at {calendarEvent.Start}" +
                                      $" with GCal ID {updatedEvent.Id}");
                }
                else
                {
                    _logger.LogInformation($"Up to date: {existingEvent.Summary}" +
                                      $" at {calendarEvent.Start}");
                }
            }
        }
    }

    private bool ExistingEventHasChanged(Event existingEvent, CalendarEvent calendarEvent)
    {
        // check event hash
        var bracketMatchingExpression = @"(?<=^\[).*?(?=\])";
        var match = Regex.Match(existingEvent.Description.Replace("<br>", "\n"),
            bracketMatchingExpression,
            RegexOptions.Multiline);

        if (!match.Success) throw new ApplicationException("Unable to parse event hash from description");

        if (match.Value != GenerateHashForEvent(calendarEvent)) return true;

        return false;
    }

    private Event CreateGCalEvent(CalendarEvent calendarEvent)
    {
        var currentTime = DateTime.UtcNow;
        var eventHash = GenerateHashForEvent(calendarEvent);
        var parsedEventTitle = _myDmuService.ParseCalendarEventTitle(calendarEvent.Desc1!);

        var teacherInfo = calendarEvent.TeacherName?.Trim();
        if (!string.IsNullOrEmpty(calendarEvent.TeacherEmail))
        {
            teacherInfo += $" ({calendarEvent.TeacherEmail.Trim().ToLower()})";
        }

        var gCalEvent = new Event
        {
            Start = new EventDateTime
            {
                DateTime = calendarEvent.Start
            },
            End = new EventDateTime
            {
                DateTime = calendarEvent.End
            },
            Summary = $"{parsedEventTitle.ModuleName} - {parsedEventTitle.ModuleId}",
            Location = calendarEvent.LocAdd1,
            Description = $"Session taught by <b>{teacherInfo}</b>" +
                          "<br>" +
                          $"[{eventHash}] Synced at: {currentTime:s}",
            Reminders = new Event.RemindersData
            {
                UseDefault = false,
                Overrides = new List<EventReminder>
                {
                    new()
                    {
                        Method = "popup",
                        Minutes = 30
                    }
                }
            }
        };

        if (parsedEventTitle.Online)
            gCalEvent.ConferenceData = new ConferenceData
            {
                ConferenceId = eventHash,
                ConferenceSolution = new ConferenceSolution
                {
                    Name = "Microsoft Teams",
                    Key = new ConferenceSolutionKey
                    {
                        Type = "addOn"
                    }
                },
                EntryPoints = new List<EntryPoint>
                {
                    new()
                    {
                        Label = "Teams Meeting URL",
                        Uri = calendarEvent.MeetingURL,
                        EntryPointType = "video"
                    }
                },
                Notes = calendarEvent.MeetingURLDesc
            };

        return gCalEvent;
    }

    private async Task<CampusmCalendar?> GetDmuCalendar()
    {
        var username = _applicationArguments.DmuUsername;
        var password = _applicationArguments.DmuPassword;

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            _myDmuService.SetCredentials(username, password);
            return await _myDmuService.GetCalendar(DateTime.Today, DateTime.Today.AddDays(7));
        }

        return null;
    }
    
    private async Task<GoogleCalendarService> GetGoogleCalendarService()
    {
        var clientId = _applicationArguments.GoogleAppClientId;
        var clientSecret = _applicationArguments.GoogleAppClientSecret;
        
        var calendarClient = await GoogleCalendarClient.ConfigureClient(clientId, clientSecret);
        var gcal = new GoogleCalendarService(calendarClient);
        return gcal;
    }

    private string GenerateHashForEvent(CalendarEvent calendarEvent)
    {
        var uniqueCalendarString = calendarEvent.GetLongCalendarString();
        // _logger.LogInformation($">> Event hasher:\n {uniqueCalendarString}");
        
        using var hasher = SHA256.Create();
        var hashedBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(uniqueCalendarString));

        return Convert.ToHexString(hashedBytes).Substring(0, 8);
    }
}