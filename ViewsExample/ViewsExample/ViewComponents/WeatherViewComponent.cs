using Microsoft.AspNetCore.Mvc;
using ViewsExample.Models;

namespace ViewsExample.ViewComponents
{
    public class WeatherViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<CityWeather> cityWeathers)
        {
            return View(cityWeathers);
        }
    }
}
