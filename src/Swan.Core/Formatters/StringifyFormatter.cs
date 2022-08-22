namespace Swan.Formatters;

#pragma warning disable CA1031 // Do not catch general exception types
using Extensions;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides methods to stringify objects.
/// </summary>
public static class StringifyFormatter
{
    private static readonly JsonSerializerOptions JsonStringifyOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = null, // Pascal case
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.Preserve,
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true
    };

    /// <summary>
    /// Returns text representing the properties of the specified object in a human-readable format.
    /// While this method is fairly expensive computationally speaking, it provides an easy way to
    /// examine objects.
    /// </summary>
    /// <param name="this">The object.</param>
    /// <param name="indentSpaces">The number of spaces to use per indenting levels.</param>
    /// <returns>A <see cref="string" /> that represents the current object.</returns>
    public static string Stringify(this object? @this, int indentSpaces = 2)
    {
        if (@this == null)
            return "(null)";

        indentSpaces = indentSpaces.Clamp(0, 8);

        try
        {
            var options = new JsonSerializerOptions(JsonStringifyOptions);
            // options.ReferenceHandler = null;

            var jsonData = @this.JsonSerialize(options);
            using var jsonObject = JsonDocument.Parse(jsonData);
            var output = jsonObject.RootElement.StringifyJson(indentSpaces, default);
            return output;
        }
        catch
        {
            return @this.ToStringInvariant();
        }
    }

    internal static string StringifyJson(this JsonElement element, int indentSpaces, int stackLevel)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', (stackLevel + 1) * indentSpaces);

        if (stackLevel == 0)
            builder.Append(CultureInfo.InvariantCulture, $"{"(Stringified)",-16}: ");

        if (element.ValueKind == JsonValueKind.Object)
        {
            var properties = element.EnumerateObject().ToArray();
            var idProperty = properties.FirstOrDefault(p => p.Name == "$id");
            var valuesProperty = !idProperty.IsUndefined()
                ? properties.FirstOrDefault(p => p.Name == "$values" && p.Value.ValueKind == JsonValueKind.Array)
                : default;

            if (!valuesProperty.IsUndefined())
            {
                builder.Append($"{idProperty.Name} = {idProperty.Value}" + StringifyJson(valuesProperty.Value, indentSpaces, stackLevel));
            }
            else
            {
                if (!idProperty.IsUndefined())
                    builder.Append($"{idProperty.Name} = {idProperty.Value}");

                builder.AppendLine();

                foreach (var property in properties)
                {
                    if (property.Name.StartsWith("$id", StringComparison.Ordinal))
                        continue;

                    builder
                        .Append($"{indentString}{property.Name,-16}: ")
                        .Append($"{StringifyJson(property.Value, indentSpaces, stackLevel + 1)}")
                        .AppendLine();
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            builder.AppendLine();

            var index = 0;
            foreach (var arrayElement in element.EnumerateArray())
            {
                builder
                    .Append($"{indentString}[{index}]: ")
                    .Append($"{StringifyJson(arrayElement, indentSpaces, stackLevel + 1)}")
                    .AppendLine();

                index++;
            }
        }
        else
        {
            builder.AppendLine(element.ValueKind == JsonValueKind.Null
                ? "(null)"
                : $"{element}".Truncate(24, "...")?.RemoveControlChars());
        }

        return builder.ToString().TrimEnd();
    }

    private static bool IsUndefined(this JsonProperty property) =>
        property.Value.ValueKind == JsonValueKind.Undefined;
}

#pragma warning restore CA1031 // Do not catch general exception types
