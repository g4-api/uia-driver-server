/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using CommandBridge;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

using Uia.DriverServer.Converters;
using Uia.DriverServer.Domain;
using Uia.DriverServer.Extensions;
using Uia.DriverServer.Formatters;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Middlewares;
using Uia.DriverServer.Models;

// Write the application logo to the console
UiaUtilities.WriteUiaAsciiLogo(version: "0000.00.00.0000");

// Initialize the web application builder
var builder = WebApplication.CreateBuilder(args);

#region *** Url & Kestrel ***
// Function to extract port from CLI arguments
static int GetPort(string[] arguments, int defaultPort)
{
    // Iterate through each argument in the array.
    for (int i = 0; i < arguments.Length; i++)
    {
        // Check if the current argument is "--port" and if there is a subsequent argument.
        if (arguments[i] == "--port" && i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int port))
        {
            // If the next argument is a valid integer, return it as the port number.
            return port;
        }
    }
    // If "--port" is not found or the subsequent argument is not a valid integer, return the default port.
    return defaultPort;
}

// Default port if none provided via CLI
const int defaultPort = 5555;

// Extract port from arguments
var port = GetPort(args, defaultPort);

// Use Kestrel as the web server and configure the URL settings
// for the application to listen on all interfaces on port 5000
// and 5001 for HTTPS requests by default
builder.WebHost.UseUrls($"http://+:{port}");
#endregion

#region *** Service       ***
// Add routing and set the URL to lowercase for consistency in routing paths and URLs
builder.Services.AddRouting(i => i.LowercaseUrls = true);

// Add controllers and configure the JSON serialization options
builder.Services
    .AddControllers(i => i.InputFormatters.Add(new TextPlainInputFormatter()))
    .AddJsonOptions(i =>
    {
        i.JsonSerializerOptions.WriteIndented = true;
        i.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        i.JsonSerializerOptions.Converters.Add(new TypeConverter());
        i.JsonSerializerOptions.Converters.Add(new ExceptionConverter());
    });

// Add Swagger documentation generation and configuration options
// for the API documentation UI page and the API version information
// for the Swagger documentation endpoint
builder.Services.AddSwaggerGen(i =>
{
    i.SwaggerDoc("v4", new OpenApiInfo { Title = "UiaDriver Server", Version = "v4" });
    i.OrderActionsBy(a => a.HttpMethod);
    i.EnableAnnotations();
});

// Add the cookie policy options for the application to check
// for consent and set the minimum same-site policy to None for
// cross-site requests
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Add CORS policy options to allow any origin, method, and header
// for the application to accept cross-origin requests from any domain
// and method
builder
    .Services
    .AddCors(o => o.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
#endregion

#region *** Dependencies  ***
// Add the session model dictionary as a singleton service to store
builder.Services.AddSingleton<IDictionary<string, UiaSessionResponseModel>>(new ConcurrentDictionary<string, UiaSessionResponseModel>());

// Add repositories and domain services to the application services
// collection as transient services for dependency injection and service
// resolution during runtime operations and requests to the application services and controllers
builder.Services.AddTransient<IActionsRepository, ActionsRepository>();
builder.Services.AddTransient<IDocumentRepository, DocumentRepository>();
builder.Services.AddTransient<IElementsRepository, ElementsRepository>();
builder.Services.AddTransient<IOcrRepository, OcrRepository>();
builder.Services.AddTransient<ISessionsRepository, SessionsRepository>();
builder.Services.AddTransient<IUiaDomain, UiaDomain>();

// Register JsonSerializerOptions as a singleton service
builder.Services.AddSingleton(provider => provider.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);
#endregion

#region *** Configuration ***
// Initialize the application builder
var app = builder.Build();

// Configure the application to use the exception handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add the cookie policy
app.UseCookiePolicy();

// Add the CORS policy to the application to allow cross-origin requests
app.UseCors("CorsPolicy");

// Add the Swagger documentation and UI page to the application
app.UseSwagger();
app.UseSwaggerUI(i =>
{
    i.SwaggerEndpoint("/swagger/v4/swagger.json", "UiaDriver Server v4");
    i.DisplayRequestDuration();
    i.EnableFilter();
    i.EnableTryItOutByDefault();
});

// Add the routing and controller mapping to the application
app.UseRouting();
app.MapDefaultControllerRoute();
app.MapControllers();
#endregion

#region *** Command Line  ***
// Invoke the command from the command line arguments (if any)
CommandBase.FindCommand(args)?.Invoke(args);
#endregion

// Set the process DPI awareness context to Per Monitor v2 (-4)
User32.SetDpiAwareness();

// Run the application asynchronously and wait for it to complete before exiting
await app.RunAsync();
