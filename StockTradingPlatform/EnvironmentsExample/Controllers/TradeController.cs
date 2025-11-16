using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    public class TradeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
