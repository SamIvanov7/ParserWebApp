using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;

namespace ParserConsoleApp
{
    /// <summary>
    /// Parses log files and fetches additional information for log entries.
    /// </summary>
    public class Parser
    {
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        private readonly string apiKey;
        private readonly ConcurrentDictionary<string, string> dnsCache = new ConcurrentDictionary<string, string>();
        private readonly Regex logLinePattern = new Regex(@"^(?<client>[^\s]+) - - \[(?<datetime>[^\]]+)\] ""GET (?<uri>[^\s\?]+)(\?[^\s]+)? (?<protocol>[^\s]+)"" (?<status>\d+) (?<size>\d+)$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the Parser class with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration containing the API key.</param>
        public Parser(IConfiguration configuration)
        {
            apiKey = configuration["ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("API key not found in configuration.");
            }
        }

        /// <summary>
        /// Parses a compressed log file asynchronously and yields log entries.
        /// </summary>
        /// <param name="filePath">The path to the compressed log file.</param>
        /// <returns>An async enumerable of log entries.</returns>
        public async IAsyncEnumerable<LogEntry> ParseLogAsync(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: true))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gzipStream))
            {
                var buffer = new List<string>();
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    buffer.Add(line);
                    if (buffer.Count >= 100)
                    {
                        foreach (var logEntry in await ProcessLinesAsync(buffer))
                        {
                            if (logEntry != null && logEntry.CountryCode != "Unknown")
                            {
                                yield return logEntry;
                            }
                        }
                        buffer.Clear();
                    }
                }
                if (buffer.Count > 0)
                {
                    foreach (var logEntry in await ProcessLinesAsync(buffer))
                    {
                        if (logEntry != null && logEntry.CountryCode != "Unknown")
                        {
                            yield return logEntry;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes a list of log lines asynchronously.
        /// </summary>
        /// <param name="lines">The list of log lines to process.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of log entries.</returns>
        private async Task<IEnumerable<LogEntry>> ProcessLinesAsync(List<string> lines)
        {
            var tasks = new List<Task<LogEntry>>();
            foreach (var line in lines)
            {
                tasks.Add(ProcessLineAsync(line));
            }
            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Processes a single log line asynchronously.
        /// </summary>
        /// <param name="line">The log line to process.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the parsed log entry.</returns>
        private async Task<LogEntry> ProcessLineAsync(string line)
        {
            try
            {
                var match = logLinePattern.Match(line);
                if (match.Success)
                {
                    var uri = match.Groups["uri"].Value;
                    if (IsValidUri(uri))
                    {
                        var client = match.Groups["client"].Value;
                        var (resolvedClient, isFallback) = await ResolveHostnameAsync(client);

                        return new LogEntry
                        {
                            Date = DateTime.ParseExact(match.Groups["datetime"].Value, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture),
                            Client = client,
                            Uri = uri,
                            Protocol = match.Groups["protocol"].Value,
                            StatusCode = int.Parse(match.Groups["status"].Value),
                            Size = int.Parse(match.Groups["size"].Value),
                            CountryCode = resolvedClient == "Unknown" ? "Unknown" : await GetCountryCodeAsync(resolvedClient),
                            IsFallback = isFallback
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing line: {line}");
                Console.WriteLine($"Exception: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Checks if a URI is valid and not pointing to a resource type we want to ignore.
        /// </summary>
        /// <param name="uri">The URI to check.</param>
        /// <returns>True if the URI is valid; otherwise, false.</returns>
        private bool IsValidUri(string uri)
        {
            var invalidExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".css", ".js", ".mpg", ".xbm", ".wav", ".txt" };
            var ext = Path.GetExtension(uri);
            return !invalidExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the country code for the specified client.
        /// </summary>
        /// <param name="client">The client IP or domain.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the country code.</returns>
        private async Task<string> GetCountryCodeAsync(string client)
        {
            if (IPAddress.TryParse(client, out _))
            {
                return await GetCountryCodeFromIpAsync(client);
            }
            else
            {
                return await GetCountryCodeFromDomainAsync(client);
            }
        }

        /// <summary>
        /// Gets the country code for the specified IP address.
        /// </summary>
        /// <param name="ip">The IP address.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the country code.</returns>
        private async Task<string> GetCountryCodeFromIpAsync(string ip)
        {
            try
            {
                var response = await GetWithTimeoutAsync($"https://api.ip2location.io/?key={apiKey}&ip={ip}&format=json", 3);
                if (response != null)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic location = JObject.Parse(responseContent);
                    return location.country_code;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting country code for IP {ip}: {ex.Message}");
            }
            return "Unknown";
        }

        /// <summary>
        /// Gets the country code for the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the country code.</returns>
        private async Task<string> GetCountryCodeFromDomainAsync(string domain)
        {
            try
            {
                var response = await GetWithTimeoutAsync($"https://api.ip2whois.com/v2?key={apiKey}&domain={domain}", 5);
                if (response != null)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic location = JObject.Parse(responseContent);
                    return location.country_code;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting country code for domain {domain}: {ex.Message}");
            }
            return "Unknown";
        }

        /// <summary>
        /// Resolves the hostname to an IP address and checks if the IP is a fallback.
        /// </summary>
        /// <param name="client">The client hostname or IP address.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the resolved IP address and a fallback flag.</returns>
        private async Task<(string resolvedClient, bool isFallback)> ResolveHostnameAsync(string client)
        {
            if (dnsCache.TryGetValue(client, out var cachedIp))
            {
                return (cachedIp, false);
            }

            try
            {
                if (IPAddress.TryParse(client, out _))
                {
                    dnsCache[client] = client;
                    return (client, false);
                }

                var ipAddresses = await Dns.GetHostAddressesAsync(client);
                if (ipAddresses.Length > 0)
                {
                    var resolvedIp = ipAddresses[0].ToString();
                    dnsCache[client] = resolvedIp;
                    return (resolvedIp, false);
                }
                else
                {
                    var baseDomain = ExtractBaseDomain(client);
                    if (baseDomain != null)
                    {
                        var baseIpAddresses = await Dns.GetHostAddressesAsync(baseDomain);
                        if (baseIpAddresses.Length > 0)
                        {
                            var baseResolvedIp = baseIpAddresses[0].ToString();
                            dnsCache[client] = baseResolvedIp;
                            return (baseResolvedIp, true);
                        }
                    }

                    return ("Unknown", false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resolving hostname {client}: {ex.Message}");
                return ("Unknown", false);
            }
        }

        /// <summary>
        /// Extracts the base domain from a given hostname.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <returns>The base domain.</returns>
        private string ExtractBaseDomain(string hostname)
        {
            try
            {
                var parts = hostname.Split('.');
                if (parts.Length > 2)
                {
                    return string.Join('.', parts[^2], parts[^1]);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sends an HTTP GET request with a specified timeout.
        /// </summary>
        /// <param name="url">The URL to request.</param>
        /// <param name="timeoutSeconds">The timeout in seconds.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        private async Task<HttpResponseMessage> GetWithTimeoutAsync(string url, int timeoutSeconds)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                try
                {
                    return await client.GetAsync(url, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Request to {url} timed out.");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error requesting {url}: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
