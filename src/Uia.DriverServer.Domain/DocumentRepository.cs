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
        public (int StatusCode, object Result) InvokeScript(string src)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // Get all methods from the DocumentRepository class that have the ScriptType attribute
            var methods = typeof(DocumentRepository)
                .GetMethods(Flags)
                .Where(i => i.GetCustomAttribute<ScriptTypeAttribute>() != null);

            // Find the method that has the ScriptType attribute with the type "Powershell"
            var method = methods
                .FirstOrDefault(i => i.GetCustomAttribute<ScriptTypeAttribute>().Type.Equals("Powershell", Compare));

            // If no matching method is found, return a 404 status code
            if (method == default)
            {
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

#pragma warning disable IDE0051 // These methods are used via reflection to handle specific locator segment types.
        // Invokes a PowerShell script for the specified session.
        [ScriptType("Powershell")]
        private static string InvokePowershell(string src)
#pragma warning restore IDE0051 // Remove unused private members
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
            var startInfo = new ProcessStartInfo("powershell", $"\"{path}\"");

            // Create a new process to execute the PowerShell script.
            var process = new Process
            {
                StartInfo = startInfo,
            };

            // Start the process and wait for it to exit.
            process.Start();
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

            // Return an empty string upon completion.
            return string.Empty;
        }
    }
}
