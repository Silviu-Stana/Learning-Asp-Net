using Microsoft.AspNetCore.Mvc;
using ModelValidationsExample.CustomModelBinders;
using ModelValidationsExample.Models;
using System;

namespace ModelValidationsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/register")]
        public IActionResult Index([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                List<string> errorsList = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();

                string errors = string.Join("\n", errorsList);
                return BadRequest(errors);
            }

            //Request.Headers["key"];

            return Content($"{person}");
        }

        [Route("/order")]
        public IActionResult Order(Order order)
        {
            if (!ModelState.IsValid) return BadRequest(string.Join("\n", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            if (order == null || order.Products.Count == 0) return BadRequest("Must have at least 1 product, to place a order.");

            double totalCost = order.Products.Sum(p => p.Price * p.Quantity);


            //example: InvoicePrice ($170) doesn't match with the total cost of the specified products in the order. ($160)
            if (totalCost != order.InvoicePrice) return BadRequest($"InvoicePrice (${order.InvoicePrice}) doesn't match with the total cost of the specified products in the order. (${totalCost})");


            Random r = new Random();
            int orderNumber = r.Next(1, 99999);

            order.OrderNo = orderNumber;

            return Json(new { orderNumber });
        }

    }
}
