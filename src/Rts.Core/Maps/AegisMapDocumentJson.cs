using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectAegisRTS.Maps
{
    public static class AegisMapDocumentJson
    {
        static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(AegisMapDocument document)
        {
            return JsonSerializer.Serialize(document, JsonOptions);
        }

        public static AegisMapDocument Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            return JsonSerializer.Deserialize<AegisMapDocument>(json, JsonOptions);
        }
    }
}
