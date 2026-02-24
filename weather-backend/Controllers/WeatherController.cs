using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IConnectionMultiplexer redis,
        ILogger<WeatherController> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _redis = redis;
        _logger = logger;
    }

    [Authorize] // ✅ Require JWT
    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        var db = _redis.GetDatabase();
        string cacheKey = $"weather:{city.ToLower()}";

        // 1️⃣ Check Redis cache first
        var cachedData = await db.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            _logger.LogInformation("Redis CACHE HIT for city: {City}", city);
            // ✅ Convert RedisValue to string
            var cachedWeather = JsonSerializer.Deserialize<WeatherResult>(cachedData.ToString());
            return Ok(cachedWeather);
        }

        _logger.LogInformation("Redis CACHE MISS for city: {City}", city);

        // 2️⃣ Call OpenWeather API
        var apiKey = _configuration["WeatherApi:Key"];
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        _logger.LogInformation("Calling OpenWeather API for city: {City}", city);
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return BadRequest("City not found");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        var result = new WeatherResult
        {
            City = json.RootElement.GetProperty("name").GetString(),
            Temperature = json.RootElement.GetProperty("main").GetProperty("temp").GetDouble()
        };

        // 3️⃣ Store in Redis for 10 minutes
        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(result), TimeSpan.FromMinutes(10));
        _logger.LogInformation("Weather data cached in Redis for city: {City}", city);

        return Ok(result);
    }
}


public class WeatherResult
{
    public string City { get; set; }
    public double Temperature { get; set; }
}