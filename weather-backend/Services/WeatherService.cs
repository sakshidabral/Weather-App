using StackExchange.Redis;
using System.Text.Json;

public class WeatherService : IWeatherService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IConnectionMultiplexer redis,
        ILogger<WeatherService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _redis = redis;
        _logger = logger;
    }

    public async Task<WeatherResult> GetWeather(string city)
    {
        var db = _redis.GetDatabase();
        string cacheKey = $"weather:{city.ToLower()}";

        // Check Redis
        var cachedData = await db.StringGetAsync(cacheKey);

        if (!cachedData.IsNullOrEmpty)
        {
            _logger.LogInformation("Redis CACHE HIT for city: {City}", city);

            return JsonSerializer.Deserialize<WeatherResult>(cachedData.ToString());
        }

        _logger.LogInformation("Redis CACHE MISS for city: {City}", city);

        // Call API
        var apiKey = _configuration["WeatherApi:Key"];
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception("City not found");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        var result = new WeatherResult
        {
            City = json.RootElement.GetProperty("name").GetString(),
            Temperature = json.RootElement.GetProperty("main").GetProperty("temp").GetDouble()
        };

        // Store in Redis
        await db.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            TimeSpan.FromMinutes(10)
        );

        _logger.LogInformation("Weather cached for city: {City}", city);

        return result;
    }
}