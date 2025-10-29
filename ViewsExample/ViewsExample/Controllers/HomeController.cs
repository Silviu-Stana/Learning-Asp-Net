using Microsoft.AspNetCore.Mvc;
using ViewsExample.Models;

namespace ViewsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/home")]
        [Route("/")]
        public IActionResult Index()
        {
            ViewData["appTitle"] = "Asp.Net Core Demo App";

            List<Person> people = new List<Person>()
            {
                new Person(){Name = "John", DateOfBirth = DateTime.Parse("2000-05-06"), PersonGender = Gender.Male},
                new Person(){Name = "Linda", DateOfBirth = DateTime.Parse("2002-01-09"), PersonGender = Gender.Female},
                new Person(){Name = "Susan", DateOfBirth = DateTime.Parse("2008-03-04"), PersonGender = Gender.Female},
            };

            return View(people); //Index.cshtml
        }

        [Route("person-details/{name}")]
        public IActionResult Details(string? name)
        {
            if (name == null) return Content("Person name cannot be null.");

            List<Person> people = new List<Person>()
            {
                new(){Name = "John", DateOfBirth = DateTime.Parse("2000-05-06"), PersonGender = Gender.Male},
                new(){Name = "Linda", DateOfBirth = DateTime.Parse("2002-01-09"), PersonGender = Gender.Female},
                new(){Name = "Susan", DateOfBirth = DateTime.Parse("2008-03-04"), PersonGender = Gender.Female},
            };

            Person? matchingPerson = people.Where(temp=>temp.Name == name).FirstOrDefault();

            return View(matchingPerson);
        }

        [Route("person-with-product")]
        public IActionResult PersonWithProduct() {  
            Person person = new() { Name="Sara", PersonGender= Gender.Female, DateOfBirth=Convert.ToDateTime("2004-07-01")};
            Product product = new() { Id=1, Name = "Air Conditioner" };

            PersonAndProductWrapperModel personAndProductWrapperModel = new() { PersonData=person, ProductData=product };

            return View(personAndProductWrapperModel); }
    }
}
