/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using CommandBridge;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uia.DriverServer.Commands
{
    /// <summary>
    /// Represents the command to register a new node in the WebDriver grid.
    /// </summary>
    [Command(name: "register", description: "Register a new node in the WebDriver v3.x grid")]
    public class RegisterCommand : CommandBase
    {
        // Provides configuration options for JSON serialization.>
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            // Use camel case naming policy for JSON properties
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

            // Ignore properties with null values during serialization
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

            // Indent JSON output for readability
            WriteIndented = true
        };

        // A static dictionary to store command parameters.
        private static readonly Dictionary<string, IDictionary<string, CommandData>> s_commands = new()
        {
            ["register"] = new Dictionary<string, CommandData>(StringComparer.Ordinal)
            {
                { "b", new() { Name = "browserName", Description = "Specifies the name of the browser to use." } },
                { "c", new() { Name = "config", Description = "Path to the configuration file." } },
                { "ht", new() { Name = "host", Description = "Specifies the node host address." } },
                { "hb", new() { Name = "hub", Description = "Specifies the hub address." } },
                { "p", new() { Name = "hubPort", Description = "Specifies the port of the hub." } },
                { "P", new() { Name = "hostPort", Description = "Specifies the port of the node host." } },
                { "t", new() { Name = "tags", Description = "Specifies the tags for the node. These tags will be converted to capabilities." } }
            }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterCommand"/> class.
        /// </summary>
        public RegisterCommand()
            : base(s_commands)
        { }

        /// <inheritdoc />
        protected override void OnInvoke(Dictionary<string, string> parameters)
        {
            // Attempt to get the "host" parameter, defaulting to "localhost" if not found
            var host = parameters.TryGetValue("host", out string hostOut)
                ? hostOut
                : "localhost";

            // Attempt to get the "hostPort" parameter and parse it to an integer, defaulting to 5555 if not found or invalid
            var port = parameters.TryGetValue("hostPort", out string portValue) && int.TryParse(portValue, out int portOut)
                ? portOut
                : 5555;

            // Attempt to get the "hub" parameter, defaulting to "localhost" if not found
            var hub = parameters.TryGetValue("hub", out string hubOut)
                ? hubOut
                : "localhost";

            // Attempt to get the "hubPort" parameter and parse it to an integer, defaulting to 4444 if not found or invalid
            var hubPort = parameters.TryGetValue("hubPort", out string hubPortValue) && int.TryParse(hubPortValue, out int hubPortOut)
                ? hubPortOut
                : 4444;

            // Attempt to get the "tags" parameter and format it, defaulting to an empty dictionary if not found
            var tags = parameters.TryGetValue("tags", out string tagsOut)
                ? FormatTags(tagsOut)
                : [];

            // Attempt to get the "browserName" parameter, defaulting to "UiaDriver" if not found
            var browserName = parameters.TryGetValue("browserName", out string browserNameOut)
                ? browserNameOut
                : "UiaDriver";

            // Create a new node configuration using the parsed and defaulted parameters
            var nodeConfiguration = NewNodeConfiguration(new NodeConfigurationModel
            {
                Port = port,
                HubPort = hubPort,
                Host = host,
                BrowserName = browserName,
                Tags = tags
            });

            // Serialize the node configuration to JSON
            var content = JsonSerializer.Serialize(nodeConfiguration, s_jsonOptions);

            // Create a new StringContent object with the serialized JSON
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            // Create a new HttpRequestMessage object for the registration request
            var request = new HttpRequestMessage
            {
                Content = stringContent,
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{hub}:{hubPort}/grid/register/")
            };

            // initialize a new HttpClient object
            using var client = new HttpClient();

            // Send the registration request and wait for the response asynchronously
            // to complete the registration process synchronously
            var response = client.SendAsync(request).GetAwaiter().GetResult();

            // Check if the registration was successful
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Node registered successfully at {host}:{port}");
                return;
            }

            // If registration failed, print error message
            var message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine($"Failed to register node:\n {message}");
        }

        // Formats a semicolon-separated string of tags into a dictionary.
        private static Dictionary<string, string> FormatTags(string tags)
        {
            // Split the input string by semicolons to get individual tags
            var _tags = tags.Split(';');

            // Initialize a dictionary to hold the formatted tags
            var outcome = new Dictionary<string, string>();

            // Iterate through each tag in the array
            foreach (var tag in _tags)
            {
                // Split the tag by the equals sign to separate key and value
                var _tag = tag.Split('=');

                // Trim whitespace from key and value, and add to the dictionary
                outcome[_tag[0].Trim()] = _tag[1].Trim();
            }

            // Return the formatted dictionary of tags
            return outcome;
        }

        // Creates a new node configuration object based on the provided model.
        private static object NewNodeConfiguration(NodeConfigurationModel configuration)
        {
            // Initialize a dictionary to hold the capabilities of the node
            var capabilities = new Dictionary<string, object>
            {
                ["browserName"] = configuration.BrowserName,
                ["browserVersion"] = "1.0",
                ["platform"] = "WINDOWS",
                ["maxInstances"] = 1,
                ["role"] = "WebDriver"
            };

            // Add additional tags from the configuration model to the capabilities dictionary
            foreach (var tag in configuration.Tags)
            {
                capabilities[tag.Key] = tag.Value;
            }

            // Create and return a new anonymous object representing the node configuration
            return new
            {
                // Set the capabilities array with the configured capabilities
                Capabilities = new[]
                {
                    capabilities
                },
                // Set additional configuration settings
                Configuration = new
                {
                    _comment = "Configuration for Windows, IUIAutomation based Node.",
                    CleanUpCycle = 2000,
                    Timeout = 30000,
                    configuration.Port,
                    configuration.Host,
                    Register = true,
                    configuration.HubPort,
                    MaxSessions = 1
                }
            };
        }

        /// <summary>
        /// Represents the configuration settings for a node in the WebDriver grid.
        /// </summary>
        private sealed class NodeConfigurationModel
        {
            /// <summary>
            /// Gets or sets the port on which the node is running.
            /// </summary>
            public int Port { get; set; }

            /// <summary>
            /// Gets or sets the port on which the hub is running.
            /// </summary>
            public int HubPort { get; set; }

            /// <summary>
            /// Gets or sets the host address of the node.
            /// </summary>
            public string Host { get; set; }

            /// <summary>
            /// Gets or sets the name of the browser that the node supports.
            /// </summary>
            public string BrowserName { get; set; }

            /// <summary>
            /// Gets or sets additional tags for the node configuration.
            /// </summary>
            public IDictionary<string, string> Tags { get; set; }
        }
    }
}
