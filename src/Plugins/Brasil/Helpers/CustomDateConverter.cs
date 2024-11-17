using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JJMasterData.Brasil.Helpers;

internal sealed class CustomDateConverter(string dateFormat) : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        return DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(dateFormat));
    }
}