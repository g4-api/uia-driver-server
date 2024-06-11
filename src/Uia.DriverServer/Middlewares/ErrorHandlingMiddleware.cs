/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Middlewares
{
    /// <summary>
    /// Middleware to handle exceptions in the HTTP request pipeline.
    /// </summary>
    /// <param name="uiaDomain">The UI automation domain.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="jsonOptions">The JSON serializer options.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    public class ErrorHandlingMiddleware(
        IUiaDomain uiaDomain,
        ILogger<ErrorHandlingMiddleware> logger,
        JsonSerializerOptions jsonOptions,
        RequestDelegate next)
    {
        // Initialize the next middleware in the pipeline to invoke after
        // handling exceptions in the current middleware class instance
        private readonly RequestDelegate _next = next;

        // Initialize the logger instance for logging information in the middleware class
        private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

        // Initialize the JSON serializer options for the middleware to use
        // when serializing responses to JSON format
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions;

        // Initialize the UIA domain interface for the middleware to interact
        // with the domain layer services and repositories
        private readonly IUiaDomain _uiaDomain = uiaDomain;

        /// <summary>
        /// Middleware to handle exceptions and customize responses in the HTTP request pipeline.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Save the original response body stream
                var bodyStream = context.Response.Body;

                // Create a memory stream to temporarily hold the response body
                await using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                // Invoke the next middleware in the pipeline
                await _next(context);

                // Restore the original response body stream
                context.Response.Body = bodyStream;

                // Rewind the memory stream to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Read the response body from the memory stream
                using var reader = new StreamReader(memoryStream, Encoding.UTF8);
                var responseBody = await reader.ReadToEndAsync();

                // Check if the response body contains model validation errors
                var isModelValidation = responseBody.Contains("errors", StringComparison.OrdinalIgnoreCase);

                // Retrieve session ID from the route
                var session = context.Request.RouteValues["session"]?.ToString();

                // Check if the session is invalid
                if (session != null)
                {
                    // Handle invalid session errors and return an appropriate response
                    var isInvalid = await HandleInvalidSession(context, _uiaDomain, session, _jsonOptions);

                    // Return early if the session is invalid
                    if (isInvalid)
                    {
                        // Log the occurrence of an invalid session error
                        _logger.LogError("Invalid session ID: {Session}", session);

                        // Return early
                        return;
                    }
                }

                // Check for BadRequest status and handle it
                if (context.Response.StatusCode == (int)HttpStatusCode.BadRequest && isModelValidation)
                {
                    // Handle the BadRequest response and return a customized response
                    await HandleBadRequestAsync(context, responseBody, _jsonOptions);

                    // Log the BadRequest response handled by the middleware class instance logger instance for information purposes only
                    _logger.LogError("A BadRequest response was handled. Response Body:\n{ResponseBody}", responseBody);

                    // Return early
                    return;
                }

                // Write the original response body back to the context response
                await context.Response.Body.WriteAsync(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                // Log the exception
                _logger.LogError(e, "An unhandled exception occurred.");

                // Handle the exception and return an appropriate response
                await HandleExceptionAsync(context, exception: e, _jsonOptions);
            }
        }

        // Handles BadRequest responses by customizing the response body.
        private static Task HandleBadRequestAsync(HttpContext context, string responseBody, JsonSerializerOptions jsonOptions)
        {
            // Formats the data from the response body, attempting to parse it as JSON
            static object FormatData(string responseBody)
            {
                try
                {
                    // Try to parse the response body as a JSON document
                    return JsonDocument.Parse(responseBody);
                }
                catch (Exception)
                {
                    // If parsing fails, return the original response body as a string
                    return responseBody;
                }
            }

            // Create the response object
            var response = new
            {
                Value = new
                {
                    Error = "Bad Request",
                    Message = "The request could not be understood by the server due to malformed syntax.",
                    Data = FormatData(responseBody)
                }
            };

            // Serialize the response object to a JSON string
            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            // Clear the current response
            context.Response.Clear();

            // Set the response content type to JSON
            context.Response.ContentType = "application/json";

            // Write the serialized JSON response to the response body
            return context.Response.WriteAsync(jsonResponse);
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

            // Serialize the response object to a JSON string
            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            // Serialize the response object to JSON and write it to the response body
            return context.Response.WriteAsync(jsonResponse);
        }

        // Handles invalid session errors and returns an appropriate JSON response.
        private static async Task<bool> HandleInvalidSession(HttpContext context, IUiaDomain domain, string session, JsonSerializerOptions jsonOptions)
        {
            // Get the session status code from the domain's session repository
            var statusCode = domain.SessionsRepository.GetSession(session).StatusCode;

            // If the session status code is not 404 (Not Found), return early
            if (statusCode != StatusCodes.Status404NotFound)
            {
                // Return false to indicate that the session is valid
                return false;
            }

            // Create a response indicating an invalid session error
            var response = WebDriverResponseModel.NewInvalidSessionResponse(session);

            // Set the response status code to 404 (Not Found)
            context.Response.StatusCode = StatusCodes.Status404NotFound;

            // Serialize the response object to a JSON string
            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            // Clear the current response
            context.Response.Clear();

            // Write the serialized JSON response to the response body
            await context.Response.WriteAsync(jsonResponse);

            // Return true to indicate that the invalid session was handled
            return true;
        }
    }
}
