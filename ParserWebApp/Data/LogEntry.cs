using System;

namespace ParserWebApp.Data
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Client { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public int Size { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public bool IsFallback { get; set; }
    }
}
