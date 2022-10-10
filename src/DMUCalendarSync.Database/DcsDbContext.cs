using DMUCalendarSync.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DMUCalendarSync.Database
{
    public class DcsDbContext : DbContext
    {
        public DbSet<MyDmuUser> MyDmuUsers { get; set; } = null!;
        public DbSet<MyDmuCookie> MyDmuCookies { get; set; } = null!;
        public DbSet<MyDmuCookieSet> MyDmuCookieSets { get; set; } = null!;


        public static string GetDatabasePath()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DMUCalendarSync");
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
            }
            return Path.Combine(databasePath, "dcs.db");
        }   

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite($"Data Source={GetDatabasePath()}");
            }
        }
    }
}