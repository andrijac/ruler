using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Converters
{
    /// <summary>
    /// Custom JSON converter that writes string-based JSON payloads raw and unescaped,
    /// allowing nested JSON to be fully indented within the container file.
    /// </summary>
    public class RawJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the nested JSON node and return its string representation
            var token = JToken.Load(reader);
            return token.ToString(Formatting.Indented);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is string rawJson && !string.IsNullOrEmpty(rawJson))
            {
                // Write the payload string directly as raw JSON without escaping
                writer.WriteRawValue(rawJson);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
