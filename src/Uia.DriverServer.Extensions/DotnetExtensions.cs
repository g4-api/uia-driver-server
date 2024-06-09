/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;

namespace Uia.DriverServer.Extensions
{
    /// <summary>
    /// Provides extension methods for .NET classes.
    /// </summary>
    public static class DotnetExtensions
    {
        /// <summary>
        /// Converts the contents of the stream to a Base64 encoded string.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns>A Base64 encoded string representing the contents of the stream.</returns>
        public static string ConvertToBase64(this Stream stream)
        {
            // Check if the stream is a MemoryStream for optimized conversion.
            if (stream is MemoryStream memoryStream)
            {
                // Convert the MemoryStream to a byte array and then to a Base64 string.
                return Convert.ToBase64String(memoryStream.ToArray());
            }

            // Allocate a byte array to hold the stream's contents.
            var bytes = new byte[(int)stream.Length];

            // Reset the stream's position to the beginning.
            stream.Seek(0, SeekOrigin.Begin);

            // Read the stream's contents into the byte array.
            stream.Read(bytes, 0, (int)stream.Length);

            // Convert the byte array to a Base64 string and return it.
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts the input string to camel case.
        /// </summary>
        /// <param name="input">The input string to be converted.</param>
        /// <returns>The input string converted to camel case.</returns>
        public static string ConvertToCamelCase(this string input)
        {
            // Convert the first character to lower case and concatenate with the rest of the string,
            // then replace underscores with an empty string.
            return (char.ToLowerInvariant(input[0]) + input[1..]).Replace("_", string.Empty);
        }

        /// <summary>
        /// Converts a string to a <see cref="SecureString"/>.
        /// </summary>
        /// <param name="str">The input string to be converted.</param>
        /// <returns>A <see cref="SecureString"/> containing the characters of the input string.</returns>
        public static SecureString ConvertToSecureString(this string str)
        {
            // If the input string is null or empty, set it to an empty string.
            str = string.IsNullOrEmpty(str) ? string.Empty : str;

            // Create a new instance of SecureString.
            var securePassword = new SecureString();

            // Append each character of the input string to the SecureString.
            foreach (var character in str)
            {
                securePassword.AppendChar(character);
            }

            // Return the SecureString.
            return securePassword;
        }

        /// <summary>
        /// Formats the input string to be XML-safe by replacing special characters with their corresponding XML entities.
        /// </summary>
        /// <param name="input">The input string to be formatted.</param>
        /// <returns>The XML-safe formatted string, or an empty string if the input is null or empty.</returns>
        public static string FormatXml(this string input)
        {
            // Check if the input string is null or empty.
            if (string.IsNullOrEmpty(input))
            {
                // Return an empty string if the input is null or empty.
                return string.Empty;
            }

            // Replace special characters with their corresponding XML entities and return the result.
            return input
                .Replace("&", "&amp;")     // Ampersand
                .Replace("\"", "&quot;")   // Double quote
                .Replace("'", "&apos;")    // Single quote
                .Replace("<", "&lt;")      // Less than
                .Replace(">", "&gt;")      // Greater than
                .Replace("\n", "&#xA;")    // Newline
                .Replace("\r", "&#xD;");   // Carriage return
        }

        /// <summary>
        /// Gets the file name of the process's start info, or the process name if the file name is not accessible.
        /// </summary>
        /// <param name="process">The process instance.</param>
        /// <returns>The file name of the process's start info if accessible; otherwise, the process name.</returns>
        public static string GetNameOrFile(this Process process)
        {
            try
            {
                // Attempt to return the file name from the process's start info.
                return process.StartInfo.FileName;
            }
            catch (Exception e) when (e != null)
            {
                // If an exception occurs, return the process name instead.
                return process.ProcessName;
            }
        }

        /// <summary>
        /// Reads the HTTP request body as a JSON-formatted string and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON to.</typeparam>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized object of type <typeparamref name="T"/>.</returns>
        /// <exception cref="NotSupportedException">Thrown when the request body is not JSON formatted.</exception>
        public static async Task<T> ReadAsAsync<T>(this HttpRequest request)
        {
            // Read the request body as a string.
            var requestBody = await ReadAsync(request).ConfigureAwait(false);

            // Validate that the request body is JSON formatted.
            if (!ConfirmJson(requestBody))
            {
                // Throw an exception if the request body is not JSON formatted.
                throw new NotSupportedException("The request body must be JSON formatted.");
            }

            // Deserialize the JSON string to the specified type and return it.
            return JsonSerializer.Deserialize<T>(requestBody);
        }

        /// <summary>
        /// Reads the HTTP request body as a string.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the request body as a string.</returns>
        public static async Task<string> ReadAsync(this HttpRequest request)
        {
            // Create a StreamReader to read the request body.
            using var streamReader = new StreamReader(request.Body);

            // Read the request body to the end and return it as a string.
            return await streamReader.ReadToEndAsync();
        }

        // Validates if the provided string is a valid JSON.
        private static bool ConfirmJson(string json)
        {
            try
            {
                // Attempt to parse the JSON string.
                JsonDocument.Parse(json);

                // If parsing succeeds, return true.
                return true;
            }
            catch (Exception e) when (e != null)
            {
                // If an exception is thrown during parsing, return false.
                return false;
            }
        }
    }
}
