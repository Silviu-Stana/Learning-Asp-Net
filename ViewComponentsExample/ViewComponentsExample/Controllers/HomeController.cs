using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }


        [Route("friends-list")]
        public IActionResult LoadFriendsList()
        {
            PersonGridModel personGridModel = new()
            {
                Title = "Friends",
                People = new()
                {
                    new(){Name="Mia", JobTitle="Developer"},
                    new(){Name="Emma", JobTitle="UI"},
                    new(){Name="Avva", JobTitle="CEO"}
                }
            };

            return ViewComponent("Grid", new { grid = personGridModel });
        }
    }
}
