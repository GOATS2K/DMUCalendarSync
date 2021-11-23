namespace DMUCalendarSync.Services.Models;

public class SamlResponse
{
    public string ResponseDocument { get; set; } = null!;
    public string RelayState { get; set; } = null!;
    public string ResponseUrl { get; set; } = null!;
}