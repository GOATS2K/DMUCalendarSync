namespace DMUCalendarSync.Services.Models;

public class ParsedEventTitle
{
    public string ModuleName { get; set; } = null!;
    public string ModuleId { get; set; } = null!;
    public string SessionCode { get; set; } = null!;
    public bool Online { get; set; }
}