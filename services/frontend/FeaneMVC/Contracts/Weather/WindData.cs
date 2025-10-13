using System.Text.Json.Serialization;

namespace FeaneMVC.Contracts.Weather;

public class WindData
{
    [JsonPropertyName("speed")]
    public float Speed { get; set; }

    [JsonPropertyName("deg")]
    public int Deg { get; set; }
}
