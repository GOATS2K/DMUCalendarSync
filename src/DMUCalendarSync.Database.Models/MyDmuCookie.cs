namespace DMUCalendarSync.Database.Models;

public class MyDmuCookie
{
    public int Id { get; set; }
    public MyDmuUser MyDmuUser { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public DateTime ExpiryTime { get; set; }
}