using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using Services;

namespace DIExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICitiesService _citiesService;
        private readonly ICitiesService _citiesService2;
        private readonly ICitiesService _citiesService3;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HomeController(ICitiesService citiesService, ICitiesService citiesService2, ICitiesService citiesService3, IServiceScopeFactory serviceScopeFactory)
        {
            _citiesService = citiesService;
            _citiesService2 = citiesService2;
            _citiesService3 = citiesService3;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [Route("/")]
        public IActionResult Index()
        {
            List<string> cities = _citiesService.GetCities();
            ViewBag.InstanceId_CitiesService = _citiesService.InstanceId;
            ViewBag.InstanceId_CitiesService2 = _citiesService2.InstanceId;
            ViewBag.InstanceId_CitiesService3 = _citiesService3.InstanceId;

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                ICitiesService citiesService = scope.ServiceProvider.GetRequiredService<ICitiesService>();
                //DB work
                ViewBag.InstanceId_CitiesService_InScope = citiesService.InstanceId;
            }//Calls Dispose
            return View(cities);
        }
    }
}
