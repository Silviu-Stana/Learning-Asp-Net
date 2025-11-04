using ViewsExample.Models;

namespace Services
{
    public class WeatherService : IWeatherService
    {
        private List<CityWeather> _cityWeathers;
        public WeatherService()
        {
            _cityWeathers =
                [
                new() { CityUniqueCode = "LDN", CityName = "London", DateAndTime = Convert.ToDateTime("2030-01-01 8:00"),  TemperatureFahrenheit = 33 },
                new() { CityUniqueCode = "NYC", CityName = "London", DateAndTime = Convert.ToDateTime("2030-01-01 3:00"),  TemperatureFahrenheit = 60 },
                new() { CityUniqueCode = "PAR", CityName = "Paris", DateAndTime = Convert.ToDateTime("2030-01-01 9:00"),  TemperatureFahrenheit = 82 },
                ];
        }

        public List<CityWeather> GetCityWeathers()
        {
            return _cityWeathers;
        }

        public CityWeather? GetWeatherByCityCode(string cityCode)
        {
            if (string.IsNullOrEmpty(cityCode)) return null;
            return _cityWeathers.Where(c => c.CityUniqueCode == cityCode).FirstOrDefault();
        }
    }
}
