using System.Text.Json.Serialization;

namespace FeaneMVC.Contracts.Weather;

public class Coordinates
{
    [JsonPropertyName("lon")]
    public float Lon { get; set; }

    [JsonPropertyName("lat")]
    public float Lat { get; set; }
}
