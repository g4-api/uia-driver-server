/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Mvc.Formatters;

using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Uia.DriverServer.Formatters
{
    /// <summary>
    /// Custom input formatter to handle plain text content type.
    /// </summary>
    public class TextPlainInputFormatter : InputFormatter
    {
        // Define a constant for the plain text media type
        private const string ContentType = MediaTypeNames.Text.Plain;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlainInputFormatter"/> class.
        /// Adds the supported media type for plain text.
        /// </summary>
        public TextPlainInputFormatter()
        {
            // Add the supported media type to the formatter
            SupportedMediaTypes.Add(ContentType);
        }

        /// <summary>
        /// Reads the request body asynchronously and returns the result.
        /// </summary>
        /// <param name="context">The context for input formatter.</param>
        /// <returns>A task that represents the asynchronous read operation.</returns>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            // Create a StreamReader to read the request body
            using var streamReader = new StreamReader(context.HttpContext.Request.Body);

            // Read the entire request body as a string
            var requestBody = await streamReader.ReadToEndAsync();

            // Return the request body as the result of the formatter
            return await InputFormatterResult.SuccessAsync(requestBody);
        }

        /// <summary>
        /// Determines whether this formatter can read the request based on the context.
        /// </summary>
        /// <param name="context">The context for input formatter.</param>
        /// <returns>True if the formatter can read the request; otherwise, false.</returns>
        public override bool CanRead(InputFormatterContext context)
        {
            // Get the content type of the request
            var contentType = context.HttpContext.Request.ContentType;

            // Check if the content type starts with "text/plain"
            return contentType?.StartsWith(ContentType) == true;
        }
    }
}
