using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using ParserConsoleApp;
using ParserWebApp.Data;
using ConsoleLogEntry = ParserConsoleApp.LogEntry;
using DataLogEntry = ParserWebApp.Data.LogEntry;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Configuration;

class Program
{
    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    static async Task Main(string[] args)
    {
        // Build configuration from various sources.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();

        // Path to the log file.
        string logFilePath = @"F:\Projects\NASA_access_log_Jul95.gz";

        // Setup dependency injection.
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddDbContext<LogDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
            .AddSingleton<Parser>()
            .BuildServiceProvider();

        // Get an instance of the Parser from the service provider.
        var parser = serviceProvider.GetService<Parser>();

        var logEntries = new List<ConsoleLogEntry>();
        var batchSize = 20;

        // Parse log entries and save them in batches.
        await foreach (var logEntry in parser.ParseLogAsync(logFilePath))
        {
            logEntries.Add(logEntry);
            if (logEntries.Count >= batchSize)
            {
                await SaveBatchAsync(logEntries, serviceProvider);
                logEntries.Clear();
            }
        }

        // Save any remaining log entries.
        if (logEntries.Count > 0)
        {
            await SaveBatchAsync(logEntries, serviceProvider);
        }

        Console.WriteLine("Log entries parsed and saved to the database.");
    }

    /// <summary>
    /// Saves a batch of log entries to the database.
    /// </summary>
    /// <param name="logEntries">The list of log entries to save.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    private static async Task SaveBatchAsync(List<ConsoleLogEntry> logEntries, IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LogDbContext>();
            var logEntryEntities = new List<DataLogEntry>();

            // Convert ConsoleLogEntry instances to DataLogEntry instances.
            foreach (var logEntry in logEntries)
            {
                logEntryEntities.Add(new DataLogEntry
                {
                    Date = logEntry.Date,
                    Client = logEntry.Client,
                    Uri = logEntry.Uri,
                    Protocol = logEntry.Protocol,
                    StatusCode = logEntry.StatusCode,
                    Size = logEntry.Size,
                    CountryCode = logEntry.CountryCode
                });
            }

            // Perform bulk insert of log entries into the database.
            await context.BulkInsertAsync(logEntryEntities);
        }
    }
}
