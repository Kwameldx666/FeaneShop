using FeaneMVC.Contracts.Weather;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FeaneMVC.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string city = "Moscow")
        {
            var apiKey = _configuration["OpenWeatherMap:ApiKey"];
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetStringAsync(url);
                Console.WriteLine("Ответ от API: " + response);

                var weatherData = JsonSerializer.Deserialize<WeatherData>(response);
                Console.WriteLine($"Десериализованные данные: Temp={weatherData?.Main?.Temp}, City={weatherData?.Name}, WindSpeed={weatherData?.Wind?.Speed}");
                return View(weatherData);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = $"Ошибка при получении погоды: {ex.Message}";
                return View(new WeatherData
                {
                    Name = city,
                    Main = new MainData(),
                    Weather = new WeatherInfo[0],
                    Coord = new Coordinates(),
                    Wind = new WindData(),
                    Clouds = new CloudsData(),
                    Sys = new SysData()
                });
            }
        }
    }
}