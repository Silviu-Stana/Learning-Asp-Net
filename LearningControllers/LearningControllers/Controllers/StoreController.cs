using Microsoft.AspNetCore.Mvc;

namespace LearningControllers.Controllers
{
    public class StoreController : Controller
    {
        [Route("store/books/{id:int}")]
        public IActionResult Books(int id)
        {
            id = Convert.ToInt32(Request.RouteValues["id"]);
            return Content($"<h1>Book: {id}", "text/html");
        }
    }
}
