namespace DMUCalendarSync.Services.Models;

public class UserInfo
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
}