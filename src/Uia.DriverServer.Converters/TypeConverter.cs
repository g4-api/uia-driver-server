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
    /// A custom JSON converter for serializing and deserializing <see cref="Type"/> objects.
    /// </summary>
    public class TypeConverter : JsonConverter<Type>
    {
        /// <summary>
        /// Reads and converts the JSON to a <see cref="Type"/> object.
        /// This method is not supported and will throw a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <returns>A <see cref="Type"/> object.</returns>
        /// <exception cref="NotImplementedException">Thrown always as this method is not implemented.</exception>
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // implement the read
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a <see cref="Type"/> object as JSON.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The <see cref="Type"/> object to write.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            // Start writing the JSON object
            writer.WriteStartObject();

            // Write the full name of the type as a string property
            writer.WriteString(nameof(Type), value.FullName);

            // End writing the JSON object
            writer.WriteEndObject();
        }
    }
}
