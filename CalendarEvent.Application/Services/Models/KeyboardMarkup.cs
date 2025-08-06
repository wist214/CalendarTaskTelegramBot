using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarEvent.Application.Services.Models
{
    public class KeyboardMarkup(string text, string url)
    {
        public string Text { get; set; } = text;

        public string Url { get; set; } = url;
    }
}
