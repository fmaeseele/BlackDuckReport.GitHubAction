using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlackDuckReport.GitHubAction.JsonConverters;

public class ExpiresInMilliseconds2DateTimeJsonConverter : JsonConverter<DateTime>
{
    //
    // Summary:
    //     Determines whether the specified type can be converted.
    //
    // Parameters:
    //   typeToConvert:
    //     The type to compare against.
    //
    // Returns:
    //     /// true if the type can be converted; otherwise, false.
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(DateTime))
            return true;
        return false;
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// Convert DateTime object to epoch int value
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        var now = DateTime.Now;
        if (value > now)
        {
            writer.WriteNumberValue((now - value).TotalMilliseconds);
        }
        else
        {
            throw new InvalidOperationException("Date > now");
        }
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// Convert epoch int value to DateTime object
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">Type of the object.</param>
    /// <returns>The object value.</returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTime));

        return DateTime.Now.AddMilliseconds(reader.GetInt32());
    }
}

