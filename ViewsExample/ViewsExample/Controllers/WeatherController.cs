using Microsoft.AspNetCore.Mvc;
using ViewsExample.Models;
using Services;

namespace ViewsExample.Controllers
{
    public class WeatherController(IWeatherService weatherService) : Controller
    {
        private readonly IWeatherService _weatherService = weatherService;

        [Route("/weather")]
        public IActionResult Weather()
        {
            List<CityWeather> cityWeathers = _weatherService.GetCityWeathers();
            return ViewComponent("Weather", cityWeathers);
        }


        [Route("/weather/{cityCode}")]
        public IActionResult CityWeather(string cityCode)
        {
            CityWeather? cityWeather = _weatherService.GetWeatherByCityCode(cityCode);

            if (cityWeather == null) return Content("City not found!");

            return View(cityWeather);
        }
    }
}
