using Microsoft.AspNetCore.Mvc;
using ViewsExample.Models;

namespace ViewsExample.Controllers
{
    public class WeatherController : Controller
    {
        [Route("/weather")]
        public IActionResult Weather()
        {
            List<CityWeather> cityWeathers =
                [
                new() { CityUniqueCode = "LDN", CityName = "London", DateAndTime = Convert.ToDateTime("2030-01-01 8:00"),  TemperatureFahrenheit = 33 },
                new() { CityUniqueCode = "NYC", CityName = "London", DateAndTime = Convert.ToDateTime("2030-01-01 3:00"),  TemperatureFahrenheit = 60 },
                new() { CityUniqueCode = "PAR", CityName = "Paris", DateAndTime = Convert.ToDateTime("2030-01-01 9:00"),  TemperatureFahrenheit = 82 },
                ];

            return ViewComponent("Weather", cityWeathers);
        }


        [Route("/weather/{cityCode}")]
        public IActionResult CityWeather(string? cityCode)
        {
            List<CityWeather> allCities =
            [
               new() { CityUniqueCode = "LDN", CityName = "London", DateAndTime = Convert.ToDateTime("2030-01-01 8:00"),  TemperatureFahrenheit = 33 },
               new() { CityUniqueCode = "NYC", CityName = "London", DateAndTime = Convert.ToDateTime("2030-01-01 3:00"),  TemperatureFahrenheit = 60 },
               new() { CityUniqueCode = "PAR", CityName = "Paris", DateAndTime = Convert.ToDateTime("2030-01-01 9:00"),  TemperatureFahrenheit = 82 },
            ];

            CityWeather? cityWeather = allCities.Where(c => c.CityUniqueCode == cityCode).FirstOrDefault();

            if (cityWeather == null) return Content("City not found!");

            return View(cityWeather);
        }
    }
}
