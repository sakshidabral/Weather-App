using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    //[Authorize]
    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        var apiKey = _configuration["WeatherApi:Key"];
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return BadRequest("City not found");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        var temperature = json.RootElement
                              .GetProperty("main")
                              .GetProperty("temp")
                              .GetDecimal();

        return Ok(new
        {
            City = city,
            Temperature = temperature,
            Unit = "Celsius"
        });
    }
}




/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


[ApiController]
[Route("api/weather")]
[Authorize]
public class WeatherController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("API Working!");
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        using var http = new HttpClient();
        string apiKey = "3fe9aa220b0a2daa8c0f92918ffe1ba2";

        string url =
            $"https://api.openweathermap.org/data/2.5/weather" +
            $"?q={city}&appid={apiKey}&units=metric";

        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return BadRequest("Open Weather URL not responded correctly");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        var result = new WeatherDto
        {
            City = root.GetProperty("name").GetString()!,
            Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
            Latitude = root.GetProperty("coord").GetProperty("lat").GetDouble(),
            Longitude = root.GetProperty("coord").GetProperty("lon").GetDouble()
        };

        return Ok(result);
    }
}
*/