/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Uia.DriverServer.Middlewares
{
    /// <summary>
    /// Middleware to handle exceptions in the HTTP request pipeline.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="jsonOptions">The JSON serializer options.</param>
    public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, JsonSerializerOptions jsonOptions)
    {
        // Initialize the next middleware in the pipeline to invoke after
        // handling exceptions in the current middleware class instance
        private readonly RequestDelegate _next = next;

        // Initialize the logger instance for logging information in the middleware class
        private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

        // Initialize the JSON serializer options for the middleware to use
        // when serializing responses to JSON format
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions;

        /// <summary>
        /// Invokes the middleware to handle exceptions in the HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Invoke the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception e)
            {
                // Log the exception
                _logger.LogError(e, "An unhandled exception occurred.");

                // Handle the exception and return an appropriate response
                await HandleExceptionAsync(context, exception: e, _jsonOptions);
            }
        }

        // Handles exceptions and returns an appropriate JSON response.
        private static Task HandleExceptionAsync(HttpContext context, Exception exception, JsonSerializerOptions jsonOptions)
        {
            // Set the content type and status code for the response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Get the base exception to capture the root cause of the exception
            var baseException = exception.GetBaseException();

            // Create the response object containing error details
            var response = new
            {
                Value = new
                {
                    Error = "Internal Server Error",
                    baseException.Message,
                    Stacktrace = $"{baseException}"
                }
            };

            // Serialize the response object to JSON and write it to the response body
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
