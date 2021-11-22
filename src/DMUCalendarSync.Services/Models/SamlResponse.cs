namespace DMUCalendarSync.Services.Models;

public class SamlResponse
{
    public string ResponseDocument { get; set; }
    public string RelayState { get; set; }
    public string ResponseUrl { get; set; }
}