/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uia.DriverServer.Converters
{
    /// <summary>
    /// A custom JSON converter for serializing and deserializing <see cref="Exception"/> objects.
    /// </summary>
    public class ExceptionConverter : JsonConverter<Exception>
    {
        /// <summary>
        /// Reads and converts the JSON to an <see cref="Exception"/> object.
        /// This method is not implemented and will throw a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <returns>An <see cref="Exception"/> object.</returns>
        /// <exception cref="NotImplementedException">Thrown always as this method is not implemented.</exception>
        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // implement the read
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> object as JSON.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The <see cref="Exception"/> object to write.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write the exception message
            writer.WriteString(nameof(value.Message), value.Message);

            // Write the stack trace
            writer.WriteString(nameof(value.StackTrace), value.StackTrace);

            // Write the help link, if any
            writer.WriteString(nameof(value.HelpLink), value.HelpLink);

            // Write the HResult code
            writer.WriteNumber(nameof(value.HResult), value.HResult);

            // Write the source of the exception
            writer.WriteString(nameof(value.Source), value.Source);

            // Write the target site of the exception, including declaring type and method name
            writer.WriteString(nameof(value.TargetSite), $"{value.TargetSite.DeclaringType.FullName}.{value.TargetSite.Name}");

            writer.WriteEndObject();
        }
    }
}
