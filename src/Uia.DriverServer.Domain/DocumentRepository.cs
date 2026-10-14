using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

            // Supply the repository logger so script diagnostics survive successful process exits as well as failures.
            var result = method.Invoke(obj: new ScriptsRepository(logger: _logger), parameters: [src]);

            // Return a 200 status code with the result of the script invocation
            return (StatusCodes.Status200OK, result);
        }

        /// <inheritdoc />
        public Task<(int StatusCode, object Result)> InvokeScriptAsync(string src)
        {
            throw new NotImplementedException();
        }

        private sealed class ScriptsRepository(ILogger logger)
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

            /// <summary>
            /// Executes script source through Windows PowerShell and returns trimmed standard output.
            /// </summary>
            /// <param name="src">The script source to persist in a temporary UTF-8 file with a byte-order mark.</param>
            /// <returns>The script's standard output, excluding diagnostics written to standard error.</returns>
            /// <remarks>
            /// Drains both output streams concurrently and logs standard error even when the exit code is zero.
            /// Owns the child process and temporary file, attempting file cleanup on both success and failure.
            /// </remarks>
            /// <exception cref="InvalidOperationException">
            /// PowerShell exits with a nonzero code; the exception includes the code, script path, and standard error.
            /// </exception>
            [ScriptType("Powershell")]
            public string InvokePowershell(string src)
            {
                // Isolate each request's source so concurrent invocations cannot overwrite another script.
                var path = Path.Combine(path1: Path.GetTempPath(), path2: $"{Guid.NewGuid()}.ps1");

                try
                {
                    // Identify UTF-8 explicitly for Windows PowerShell, preserving non-ASCII script literals.
                    File.WriteAllText(path, contents: src, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

                    // Pass the path as a file argument rather than command text; ArgumentList preserves spaces.
                    // Dispose the owned process before the outer finally attempts to remove the script.
                    using var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            ArgumentList = { "-File", path },
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    // Start both readers before waiting so neither redirected pipe can block the child process.
                    process.Start();
                    var standardOutputTask = process.StandardOutput.ReadToEndAsync();
                    var standardErrorTask = process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    // Complete the concurrent reads before interpreting the exit status or returning any output.
                    var standardOutput = standardOutputTask.GetAwaiter().GetResult();
                    var standardError = standardErrorTask.GetAwaiter().GetResult();

                    // Preserve nonterminating errors and native diagnostics without changing successful output.
                    if (!string.IsNullOrWhiteSpace(standardError))
                    {
                        logger?.LogWarning(
                            "PowerShell script {ScriptPath} exited with code {ExitCode}. Standard error: {StandardError}",
                            path,
                            process.ExitCode,
                            standardError);
                    }

                    // Let the existing error middleware expose the actual child failure instead of HTTP 200.
                    if (process.ExitCode != 0)
                    {
                        var message = $"PowerShell script '{path}' exited with code {process.ExitCode}." +
                            $"{Environment.NewLine}Standard error: {standardError.Trim()}";
                        throw new InvalidOperationException(message);
                    }

                    // Preserve the existing successful response contract after validating process completion.
                    return standardOutput.Trim();
                }
                finally
                {
                    // Attempt cleanup even after startup, stream, or script failures without masking their cause.
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception exception)
                    {
                        logger?.LogWarning(exception, "Failed to delete temporary PowerShell script {ScriptPath}.", path);
                    }
                }
            }
        }
    }
}
