using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Reads a JSON string array, but also accepts a single string (sent by some servers where RFC 9083 requires an
///     array, e.g. for "description").
/// </summary>
public class RDAPStringListConverter : JsonConverter<List<string>>
{
    public override List<string> ReadJson(JsonReader reader, Type objectType, List<string> existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);

        return token.Type switch
        {
            JTokenType.Array => token.Children().Select(x => x.Type == JTokenType.Null ? null : x.ToString())
                .Where(x => x != null).ToList(),
            JTokenType.Null or JTokenType.Undefined => null,
            _ => [token.ToString()]
        };
    }

    public override void WriteJson(JsonWriter writer, List<string> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
