using System.Text.Json.Serialization;

namespace Foody.Tests.DTOs
{
    public class PatchFoodDTO
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = "/name";

        [JsonPropertyName("op")]
        public string Op { get; set; } = "replace";

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}