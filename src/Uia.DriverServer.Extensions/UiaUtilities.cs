/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Security;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Extensions
{
    /// <summary>
    /// Utility class for common UI Automation operations.
    /// </summary>
    public static class UiaUtilities
    {
        /// <summary>
        /// Starts a new process with the specified file name and arguments.
        /// </summary>
        /// <param name="fileName">The name of the file to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The started <see cref="Process"/> object.</returns>
        public static Process StartProcess(string fileName, string arguments)
        {
            // Call the overloaded StartProcess method with default impersonation and working directory
            return StartProcess(impersonation: default, fileName, arguments, workingDirectory: default);
        }

        /// <summary>
        /// Starts a new process with the specified file name, arguments, and working directory.
        /// </summary>
        /// <param name="fileName">The name of the file to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The started <see cref="Process"/> object.</returns>
        public static Process StartProcess(string fileName, string arguments, string workingDirectory)
        {
            // Call the overloaded StartProcess method with default impersonation
            return StartProcess(impersonation: default, fileName, arguments, workingDirectory);
        }

        /// <summary>
        /// Starts a new process with the specified impersonation model, file name, arguments, and working directory.
        /// </summary>
        /// <param name="impersonation">The impersonation model containing user credentials.</param>
        /// <param name="fileName">The name of the file to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The started <see cref="Process"/> object.</returns>
        public static Process StartProcess(ImpersonationModel impersonation, string fileName, string arguments, string workingDirectory)
        {
            // Creates a new process with the specified ProcessStartInfo.
            static Process NewDefaultProcess(ProcessStartInfo startInfo) => new()
            {
                // Set the start information for the new process
                StartInfo = startInfo
            };

            // Creates a new process with impersonation using the specified ImpersonationModel and ProcessStartInfo.
            static Process NewImpersonatedProcess(ImpersonationModel impersonation, ProcessStartInfo startInfo)
            {
                // Initialize a SecureString to store the password securely
                var password = new SecureString();

                // Append each character of the password from the impersonation model to the SecureString
                foreach (var character in impersonation.Password)
                {
                    password.AppendChar(character);
                }

                // Set UseShellExecute to false to enable user credentials to be specified
                startInfo.UseShellExecute = false;

                // Set the domain of the impersonated user
                startInfo.Domain = impersonation.Domain;

                // Set the password of the impersonated user
                startInfo.Password = password;

                // Set the username of the impersonated user
                startInfo.UserName = impersonation.Username;

                // Create and return a new Process object with the configured start information
                return new Process
                {
                    StartInfo = startInfo
                };
            }

            // Creates a new ProcessStartInfo object with the specified file name and arguments.
            static ProcessStartInfo NewProcessStartInfo(string fileName, string arguments, string workingDirectory)
            {
                // Create a new string from the file name
                var processName = new string(fileName);

                // Check if the processName is a directory
                var isDirectory = Directory.Exists(processName);

                // Initialize a new ProcessStartInfo object with default properties
                var info = new ProcessStartInfo
                {
                    // If the processName is a directory, set FileName to "explorer.exe", otherwise set it to fileName
                    FileName = isDirectory ? "explorer.exe" : fileName,

                    // If the processName is a directory, set Arguments to fileName, otherwise set it to arguments
                    Arguments = isDirectory ? fileName : arguments,
                };

                // If a working directory is specified, set the working directory and enable shell execution
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    info.UseShellExecute = true;
                    info.WorkingDirectory = workingDirectory;
                }

                // Return the configured ProcessStartInfo object
                return info;
            }

            // Create a new ProcessStartInfo object with the specified file name and arguments
            var startInfo = NewProcessStartInfo(fileName, arguments, workingDirectory);

            // Determine whether to create an impersonated process or a default process
            var process = impersonation?.Enabled == true
                ? NewImpersonatedProcess(impersonation, startInfo)
                : NewDefaultProcess(startInfo);

            // Start the process
            process.Start();

            // Return the started process
            return process;
        }

        /// <summary>
        /// Writes the UIA ASCII logo and related information to the console.
        /// </summary>
        public static void WriteUiaAsciiLogo(string version)
        {
            // Write the ASCII art logo to the console
            Console.WriteLine("   _   _ _       ____       _                  ____                                      ");
            Console.WriteLine("  | | | (_) __ _|  _ \\ _ __(_)_   _____ _ __  / ___|  ___ _ ____   _____ _ __           ");
            Console.WriteLine("  | | | | |/ _` | | | | '__| \\ \\ / / _ \\ '__| \\___ \\ / _ \\ '__\\ \\ / / _ \\ '__|  ");
            Console.WriteLine("  | |_| | | (_| | |_| | |  | |\\ V /  __/ |     ___) |  __/ |   \\ V /  __/ |            ");
            Console.WriteLine("   \\___/|_|\\__,_|____/|_|  |_| \\_/ \\___|_|    |____/ \\___|_|    \\_/ \\___|_|       ");
            Console.WriteLine("                                                                                         ");

            // Write additional information to the console
            Console.WriteLine("                                   WebDriver Implementation for Windows Native           ");
            Console.WriteLine("                                                      Powered by IUIAutomation           ");
            Console.WriteLine("                                                                                         ");
            Console.WriteLine("  Version:           " + version + "                                                     ");
            Console.WriteLine("  Project:           https://github.com/g4-api/uia-driver                                ");
            Console.WriteLine("  W3C Documentation: https://www.w3.org/TR/webdriver/                                    ");
            Console.WriteLine("  Documentation:     https://docs.microsoft.com/en-us/windows/win32/api/_winauto/        ");
            Console.WriteLine("  Open API:          /swagger                                                            ");

            // Write blank lines to the console for spacing
            Console.WriteLine("                                                                                         ");
            Console.WriteLine("                                                                                         ");
        }
    }
}
