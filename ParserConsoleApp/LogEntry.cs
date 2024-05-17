using System;

namespace ParserConsoleApp
{
    /// <summary>
    /// Represents a single log entry.
    /// </summary>
    public class LogEntry
    {
        public DateTime Date { get; set; }
        public string Client { get; set; }
        public string Uri { get; set; }
        public string Protocol { get; set; }
        public int StatusCode { get; set; }
        public int Size { get; set; }
        public string CountryCode { get; set; }
        public bool IsFallback { get; set; }
    }
}
