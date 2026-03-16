using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Uia.DriverServer.Attributes;
using Uia.DriverServer.Extensions;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Represents the repository for document-related operations.
    /// </summary>
    public class DocumentRepository(IDictionary<string, UiaSessionResponseModel> sessions, ILogger<SessionsRepository> logger) : IDocumentRepository
    {
        // Initialize the logger instance for logging information and errors in the repository class methods and properties
        private readonly ILogger<SessionsRepository> _logger = logger;

        /// <inheritdoc />
        public IDictionary<string, UiaSessionResponseModel> Sessions { get; } = sessions;

        /// <inheritdoc />
        public (int StatusCode, string Result) GetElementSource(UiaElementModel element)
        {
            // Validate that the provided element contains a valid UI Automation element.
            if (element?.UIAutomationElement == null)
            {
                _logger?.LogInformation("UIAutomationElement is missing. Cannot create Document Object Model.");
                return (StatusCodes.Status404NotFound, default);
            }

            // Generate a new Document Object Model (DOM) based on the UI Automation element.
            var elementsXml = DocumentObjectModelFactory.New(element.UIAutomationElement);

            // Log the successful creation of the DOM, including the element's ID for traceability.
            _logger?.LogInformation("Document Object Model created successfully for element with ID {ElementId}.", element.Id);

            // Return a success status code (200 OK) along with the generated XML document.
            return (StatusCodes.Status200OK, $"{elementsXml.Document}");
        }

        /// <inheritdoc />
        public (int StatusCode, string Result) GetPageSource(string session)
        {
            // Attempt to retrieve the session from the sessions dictionary
            if (!Sessions.TryGetValue(session, out UiaSessionResponseModel uiaSession))
            {
                _logger?.LogInformation("Session with ID {SessionId} not found.", session);
                return (StatusCodes.Status404NotFound, default);
            }

            // Create a new Document Object Model (DOM) for the session's application root
            var elementsXml = DocumentObjectModelFactory.New(uiaSession.ApplicationRoot);

            // Log the successful creation of the DOM for the session
            _logger?.LogInformation("New Document Object Model created for session with ID {SessionId}.", session);

            // Return a 200 OK status code and the XML document representing the DOM
            return (StatusCodes.Status200OK, $"{elementsXml.Document}");
        }

        /// <inheritdoc />
        public (int StatusCode, object Result) InvokeScript(string session, string src)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // Attempt to retrieve the session from the sessions dictionary
            if (!Sessions.TryGetValue(session, out UiaSessionResponseModel uiaSession))
            {
                _logger?.LogInformation("Session with ID {SessionId} not found.", session);
                return (StatusCodes.Status404NotFound, default);
            }

            // Get all methods from the ScriptsRepository class that have the ScriptType attribute
            var methods = typeof(ScriptsRepository)
                .GetMethods(Flags)
                .Where(i => i.GetCustomAttribute<ScriptTypeAttribute>() != null);

            // Determine the terminal type to use for script invocation based on
            // session options, defaulting to "Powershell" if not specified
            var terminal = string.IsNullOrEmpty(uiaSession.Options.Terminal)
                ? "Powershell"
                : uiaSession.Options.Terminal;

            // Find the first method that matches the specified terminal type in its ScriptType attribute
            var method = methods
                .FirstOrDefault(i => i.GetCustomAttribute<ScriptTypeAttribute>().Type.Equals(terminal, Compare));

            // If no matching method is found, return a 404 status code
            if (method == default)
            {
                _logger?.LogInformation(
                    "Terminal implementation '{Terminal}' for session with ID {SessionId} was not found.",
                    terminal,
                    session);
                return (StatusCodes.Status500InternalServerError, string.Empty);
            }

            // Invoke the method with the specified session and script source
            var result = method.Invoke(obj: null, parameters: [src]);

            // Return a 200 status code with the result of the script invocation
            return (StatusCodes.Status200OK, result);
        }

        /// <inheritdoc />
        public Task<(int StatusCode, object Result)> InvokeScriptAsync(string src)
        {
            throw new NotImplementedException();
        }

        private sealed class ScriptsRepository
        {
            // Writes the supplied command script to a temporary .cmd file, executes it
            // through cmd.exe, captures its standard output, and then attempts to delete
            // the temporary file.
            [ScriptType("Cmd")]
            public static string InvokeCmd(string src)
            {
                // Resolve the system temporary directory where the transient command file will be created.
                var tempPath = Path.GetTempPath();

                // Generate a unique file name to avoid collisions with other temporary command files.
                var fileName = $"{Guid.NewGuid()}.cmd";

                // Combine the temporary directory path with the generated file name.
                var path = Path.Combine(tempPath, fileName);

                // Persist the provided command script content to the temporary .cmd file.
                File.WriteAllText(path, src);

                // Configure cmd.exe to execute the generated .cmd file and capture its output streams.
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c \"{path}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Create the process instance that will run the temporary command script.
                var process = new Process
                {
                    StartInfo = startInfo,
                };

                // Start executing the command script.
                process.Start();

                // Read the entire standard output stream before waiting for process completion.
                var standardOutput = process.StandardOutput.ReadToEnd();

                // Wait until the command process fully exits.
                process.WaitForExit();

                // Try to remove the temporary command file after execution completes.
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // Ignore cleanup failures because they should not fail the command invocation itself.
                }

                // Return the captured standard output without leading or trailing whitespace.
                return standardOutput.Trim();
            }

            // Invokes a PowerShell script for the specified session.
            [ScriptType("Powershell")]
            public static string InvokePowershell(string src)
            {
                // Get the temporary directory path.
                var tempPath = Path.GetTempPath();

                // Create the file name for the PowerShell script.
                var fileName = $"{Guid.NewGuid()}.ps1";

                // Combine the temporary path and file name to get the full path.
                var path = Path.Combine(tempPath, fileName);

                // Write the script contents to the file.
                File.WriteAllText(path, src);

                // Prepare the process start info for running the PowerShell script.
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"\"{path}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Create a new process to execute the PowerShell script.
                var process = new Process
                {
                    StartInfo = startInfo,
                };

                // Start the process.
                process.Start();

                // Read all output *before* we block on WaitForExit
                var standardOutput = process.StandardOutput.ReadToEnd();

                // Wait for the process to exit to ensure the script execution is complete.
                process.WaitForExit();

                // Delete the file after execution is complete.
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // Ignore any exceptions that occur while deleting the file.
                }

                // Return the standard output from the PowerShell script execution.
                return standardOutput.Trim();
            }
        }
    }
}
