/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Threading.Tasks;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Defines methods for document operations in a Windows desktop automation session.
    /// </summary>
    public interface IDocumentRepository
    {
        /// <summary>
        /// Retrieves the page source or the state of the current application window in a Windows desktop automation session.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Result: The page source or state as a string.
        /// </returns>
        (int StatusCode, string Result) GetPageSource(string session);

        /// <summary>
        /// Injects and executes a PowerShell script in the context of the current Windows desktop automation session.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <param name="src">The source code of the PowerShell script to execute.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Result: The result of the script execution.
        /// </returns>
        (int StatusCode, object Result) InvokeScript(string session, string src);

        /// <summary>
        /// Asynchronously injects and executes a PowerShell script in the context of the current Windows desktop automation session.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <param name="src">The source code of the PowerShell script to execute.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Result: The result of the script execution.
        /// </returns>
        Task<(int StatusCode, object Result)> InvokeScriptAsync(string session, string src);
    }
}
