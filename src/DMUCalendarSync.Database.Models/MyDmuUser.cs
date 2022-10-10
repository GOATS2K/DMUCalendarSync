namespace DMUCalendarSync.Database.Models
{
    public class MyDmuUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public List<MyDmuCookieSet> CookieSets { get; set; } = null!;
    }
}