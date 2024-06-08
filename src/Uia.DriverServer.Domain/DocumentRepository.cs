/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Uia.DriverServer.Attributes;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Represents the repository for document-related operations.
    /// </summary>
    public class DocumentRepository : IDocumentRepository
    {
        /// <inheritdoc />
        public (int StatusCode, string Result) GetPageSource(string session)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [SuppressMessage(
            category: "Major Code Smell",
            checkId: "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
            Justification = "Reflection is used here to dynamically invoke methods based on attributes, which provides flexibility for handling different script types.")]
        public (int StatusCode, object Result) InvokeScript(string session, string src)
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
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            try
            {
                // Invoke the method with the specified session and script source
                var result = method.Invoke(obj: null, parameters: [session, src]);

                // Return a 200 status code with the result of the script invocation
                return (StatusCodes.Status200OK, result);
            }
            catch (Exception e)
            {
                // If an exception occurs, return a 500 status code with the exception message
                return (StatusCodes.Status500InternalServerError, $"{e.GetBaseException().Message}");
            }
        }

        /// <inheritdoc />
        public Task<(int StatusCode, object Result)> InvokeScriptAsync(string session, string src)
        {
            throw new NotImplementedException();
        }

        // Invokes a PowerShell script for the specified session.
        [ScriptType("Powershell")]
        [SuppressMessage(
            category: "CodeQuality",
            checkId: "IDE0051:Remove unused private members",
            Justification = "This method is intended to be used via reflection, which may not be detected by static code analysis.")]
        private static string InvokePowershell(string session, string src)
        {
            // Get the temporary directory path.
            var tempPath = Path.GetTempPath();

            // Create the file name for the PowerShell script.
            var fileName = $"{session}.ps1";

            // Combine the temporary path and file name to get the full path.
            var path = Path.Combine(tempPath, fileName);

            // Check if the src is a path to an existing file, otherwise treat it as script content.
            var contents = File.Exists(src) ? File.ReadAllText(src) : src;

            // Write the script contents to the file.
            File.WriteAllText(path, contents);

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

            // Return an empty string upon completion.
            return string.Empty;
        }
    }
}
