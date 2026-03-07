using System;

public interface IWeatherService
{
    Task<WeatherResult> GetWeather(string city);
}
