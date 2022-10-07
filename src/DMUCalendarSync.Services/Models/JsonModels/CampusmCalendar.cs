using System.Text.Json.Serialization;

namespace DMUCalendarSync.Services.Models;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class CalendarEvent
{
    [JsonPropertyName("desc1")] public string? Desc1 { get; set; }

    [JsonPropertyName("desc2")] public string? Desc2 { get; set; }

    [JsonPropertyName("desc3")] public string? Desc3 { get; set; }

    [JsonPropertyName("calDate")] public DateTime? CalDate { get; set; }

    [JsonPropertyName("start")] public DateTime? Start { get; set; }

    [JsonPropertyName("end")] public DateTime? End { get; set; }

    [JsonPropertyName("teacherName")] public string? TeacherName { get; set; }

    [JsonPropertyName("teacherEmail")] public string? TeacherEmail { get; set; }

    [JsonPropertyName("locCode")] public string? LocCode { get; set; }

    [JsonPropertyName("locAdd1")] public string? LocAdd1 { get; set; }

    [JsonPropertyName("attendanceExclude")]
    public string? AttendanceExclude { get; set; }

    [JsonPropertyName("meeting")] public string? Meeting { get; set; }

    [JsonPropertyName("meetingURL")] public string? MeetingURL { get; set; }

    [JsonPropertyName("meetingURLDesc")] public string? MeetingURLDesc { get; set; }
}

public class CampusmCalendar
{
    [JsonPropertyName("events")] public List<CalendarEvent> Events { get; set; } = null!;
}