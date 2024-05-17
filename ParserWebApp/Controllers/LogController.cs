using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParserWebApp.Data;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace ParserWebApp.Controllers
{
    /// <summary>
    /// Controller for managing log entries and parser operations.
    /// </summary>
    public class LogController : Controller
    {
        private readonly LogDbContext _context;
        private static Process _parserProcess;

        /// <summary>
        /// Initializes a new instance of the LogController class.
        /// </summary>
        /// <param name="context">Db context for log entries.</param>
        public LogController(LogDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays a paginated list of log entries with sorting options.
        /// </summary>
        /// <param name="sortOrder">The order in which to sort the log entries.</param>
        /// <param name="page">The page number to display.</param>
        /// <returns>A view displaying the sorted and paginated log entries.</returns>
        public async Task<IActionResult> Index(string sortOrder, int? page)
        {
            // Define sort parameters .
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.ClientSortParm = sortOrder == "Client" ? "client_desc" : "Client";

            // Retrieve log entries from db
            var logs = from l in _context.LogEntries select l;

            // Apply sorting based on the sortOrder parameter.
            switch (sortOrder)
            {
                case "date_desc":
                    logs = logs.OrderByDescending(l => l.Date);
                    break;
                case "Client":
                    logs = logs.OrderBy(l => l.Client);
                    break;
                case "client_desc":
                    logs = logs.OrderByDescending(l => l.Client);
                    break;
                default:
                    logs = logs.OrderBy(l => l.Date);
                    break;
            }

            // Define pagination parameters.
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            // Return the view 
            return View(await logs.AsNoTracking().ToPagedListAsync(pageNumber, pageSize));
        }

        /// <summary>
        /// Starts the parser 
        /// </summary>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPost]
        public IActionResult StartParser()
        {
            // Checker
            if (_parserProcess != null && !_parserProcess.HasExited)
            {
                return BadRequest("Parser is already running.");
            }

            // Define the path to the parser exe
            var parserPath = @"F:\Projects\ParserWebApp\ParserConsoleApp\bin\Debug\net8.0\ParserConsoleApp.exe";

            // Configure the parser 
            _parserProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = parserPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            // Handle output and error data from the parser 
            _parserProcess.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            _parserProcess.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            // Start the parser process.
            _parserProcess.Start();
            _parserProcess.BeginOutputReadLine();
            _parserProcess.BeginErrorReadLine();

            return Ok();
        }

        /// <summary>
        /// Stops the parser process .
        /// </summary>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPost]
        public IActionResult StopParser()
        {
            // Checker
            if (_parserProcess == null || _parserProcess.HasExited)
            {
                return BadRequest("Parser is not running.");
            }

            // Kill the parser process.
            _parserProcess.Kill();
            _parserProcess.Dispose();
            _parserProcess = null;

            return Ok();
        }
    }
}
