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
#pragma warning restore IDE005
    }
}
