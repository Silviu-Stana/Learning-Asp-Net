using Microsoft.AspNetCore.Mvc;
using LearningControllers.Models;

namespace MyFirstApp.Controllers
{
    public class HomeController: Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return Content("<h1>Home Page", "text/html");
        }

        [Route("bookstore")]
        public IActionResult BookStore()
        {
            Console.WriteLine($"Keys: {string.Join(", ", Request.Query.Keys)}");
            if (!Request.Query.ContainsKey("bookid")) return BadRequest("Book id is not supplied");
            if (string.IsNullOrEmpty(Request.Query["bookid"])) return BadRequest("Book id cannot be empty");

            int bookId = Convert.ToInt16(ControllerContext.HttpContext.Request.Query["bookid"]);

            if (bookId <= 0) return BadRequest("Invalid book id");
            if (bookId > 1000) return BadRequest("Invalid book id");

            if (!Request.Query.ContainsKey("isloggedin")) return BadRequest("You must be logged in");
            if (Convert.ToBoolean(Request.Query["isloggedin"]) == false) return Unauthorized("You must be logged in");

            //return RedirectToAction("Books", "Store",  { id = bookId}); //status 302 - Found   
            return RedirectToActionPermanent("Books", "Store", new { id = bookId}); //status 301 - Moved Permanently
        }

        [Route("person")]
        public JsonResult Person()
        {
            Person person = new Person() { Id=Guid.NewGuid(), FirstName="James", LastName="Bond", Age=25};
            return Json(person);
        }
    }
}


