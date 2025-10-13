using System.Text.Json.Serialization;

namespace FeaneMVC.Contracts.Weather;

public class CloudsData
{
    [JsonPropertyName("all")]
    public int All { get; set; }
}
