using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMUCalendarSync.Database.Models
{
    public class MyDmuCookieSet
    {
        public int Id { get; set; }
        public MyDmuUser MyDmuUser { get; set; } = null!;
        public List<MyDmuCookie> Cookies { get; set; } = null!;
        public DateTime EarliestCookieExpiry { get; set; }
    }
}
