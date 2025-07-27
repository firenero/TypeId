using System.Globalization;
using Newtonsoft.Json;

namespace FastIDs.TypeId.Serialization.JsonNet;

// Newtonsoft uses internal methods to populate exception details and won't make them public (see https://github.com/JamesNK/Newtonsoft.Json/pull/1963),
// so they are duplicated for our use.
internal static class JsonSerializationExceptionFactory
{
    // See https://github.com/JamesNK/Newtonsoft.Json/blob/8f579cf5970c57025237db4c7eae33ae4af289e3/Src/Newtonsoft.Json/JsonSerializationException.cs#L123
    public static JsonSerializationException Create(JsonReader reader, string message, Exception? ex)
    {
        return Create(reader as IJsonLineInfo, reader.Path, message, ex);
    }

    public static JsonSerializationException Create(IJsonLineInfo? lineInfo, string path, string message, Exception? ex)
    {
        message = FormatMessage(lineInfo, path, message);
        int lineNumber;
        int linePosition;
    
        if (lineInfo != null && lineInfo.HasLineInfo())
        {
            lineNumber = lineInfo.LineNumber;
            linePosition = lineInfo.LinePosition;
        }
        else
        {
            lineNumber = 0;
            linePosition = 0;
        }
    
        return new JsonSerializationException(message, path, lineNumber, linePosition, ex);
    }
    
    // See https://github.com/JamesNK/Newtonsoft.Json/blob/8f579cf5970c57025237db4c7eae33ae4af289e3/Src/Newtonsoft.Json/JsonPosition.cs#L150
    private static string FormatMessage(IJsonLineInfo? lineInfo, string path, string message)
    {
        // don't add a fullstop and space when message ends with a new line
        if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            message = message.Trim();

            if (!message.EndsWith('.'))
            {
                message += ".";
            }

            message += " ";
        }

        message += string.Format(CultureInfo.InvariantCulture, "Path '{0}'", path);

        if (lineInfo != null && lineInfo.HasLineInfo())
        {
            message += string.Format(CultureInfo.InvariantCulture, ", line {0}, position {1}", lineInfo.LineNumber, lineInfo.LinePosition);
        }

        message += ".";

        return message;
    }
}
