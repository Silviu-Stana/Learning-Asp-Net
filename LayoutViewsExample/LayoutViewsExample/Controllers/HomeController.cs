using LayoutViewsExample.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace LayoutViewsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("/contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("/programming-languages")]
        public IActionResult ProgrammingLanguages()
        {
            ListModel listModel = new() { Items = new List<string>() { "Python", "C#", "Go"},Title= "ProgrammingLanguages List" };
            return PartialView("_ListPartialView", listModel);
        }
    }
}
