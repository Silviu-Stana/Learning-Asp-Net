using System;
using ViewsExample.Models;

namespace Services
{
    public interface IWeatherService
    {
        List<CityWeather> GetCityWeathers();
        CityWeather? GetWeatherByCityCode(string cityCode);
    }
}
