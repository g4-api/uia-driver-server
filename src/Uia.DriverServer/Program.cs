/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using CommandBridge;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Uia.DriverServer.Converters;
using Uia.DriverServer.Domain;
using Uia.DriverServer.Extensions;
using Uia.DriverServer.Formatters;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Models;

// Write the application logo to the console
UiaUtilities.WriteUiaAsciiLogo();

// Initialize the web application builder
var builder = WebApplication.CreateBuilder(args);

#region *** Url & Kestrel ***
// Use Kestrel as the web server and configure the URL settings
// for the application to listen on all interfaces on port 5000
// and 5001 for HTTPS requests by default
builder.WebHost.UseUrls();
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
        i.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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
builder.Services.AddSingleton<IDictionary<string, UiaSessionModel>>(new ConcurrentDictionary<string, UiaSessionModel>());

// Add the element repository, session repository, document repository,
builder.Services.AddTransient<IElementsRepository, ElementsRepository>();
builder.Services.AddTransient<ISessionsRepository, SessionRepository>();
builder.Services.AddTransient<IDocumentRepository, DocumentRepository>();
builder.Services.AddTransient<IUiaDomain, UiaDomain>();
#endregion

#region *** Configuration ***
// Initialize the application builder
var app = builder.Build();

// Configure the application to use the developer exception page
app.UseDeveloperExceptionPage();

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
