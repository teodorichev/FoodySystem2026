using System.Text.Json.Serialization;

namespace Foody.Tests.DTOs
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("foodId")]
        public string? FoodId { get; set; }
    }
}