using Microsoft.AspNetCore.Mvc;
using LearningControllers.Models;
using MyFirstApp.Models;

namespace MyFirstApp.Controllers
{
    public class HomeController: Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return Content("<h1>Home Page", "text/html");
        }

        //Route data > Query string (in Model Binding priority)
        [Route("bookstore/{bookid:int?}/{isloggedin:bool?}")]
        // /bookstore?bookid=10&isloggedin=true
        public IActionResult BookStore(int? bookid, [FromRoute]bool? isloggedin, Book book)
        {
            Console.WriteLine($"Keys: {string.Join(", ", Request.Query.Keys)}");
            if (!bookid.HasValue) return BadRequest("Book id is not supplied");
            if (bookid<=0 || bookid>1000) return BadRequest("Invalid book id");

            if (!isloggedin.HasValue) return BadRequest("You must be logged in");
            if (isloggedin==false) return Unauthorized("You must be logged in");

            return Content("Book id: " +  bookid + ", Author: " + book.Author);
            //return RedirectToAction("Books", "Store",  { id = bookid}); //status 302 - Found   
            //return RedirectToActionPermanent("Books", "Store", new { id = bookid}); //status 301 - Moved Permanently
        }

        [Route("person")]
        public JsonResult Person()
        {
            Person person = new Person() { Id=Guid.NewGuid(), FirstName="James", LastName="Bond", Age=25};
            return Json(person);
        }
    }
}


