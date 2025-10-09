using System.Text.Json.Serialization;

namespace FeaneMVC.Contracts.Weather;

public class WeatherData
{
    [JsonPropertyName("coord")]
    public Coordinates Coord { get; set; } = new();

    [JsonPropertyName("weather")]
    public WeatherInfo[] Weather { get; set; } = Array.Empty<WeatherInfo>();

    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("main")]
    public MainData Main { get; set; } = new();

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("wind")]
    public WindData Wind { get; set; } = new();

    [JsonPropertyName("clouds")]
    public CloudsData Clouds { get; set; } = new();

    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("sys")]
    public SysData Sys { get; set; } = new();

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cod")]
    public int Cod { get; set; }
}
