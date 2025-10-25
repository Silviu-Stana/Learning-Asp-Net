using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LearningControllers.Controllers
{
    public class BankController : Controller
    {
        public Object account = new { accountNumber = 1001, accountHolderName = "Example Name", currentBalance = 5000 };

        [Route("/bank")]
        public IActionResult Bank()
        {
            return Content("Welcome to the Best Bank", "text/plain");
        }


        [Route("/account-details")]
        public IActionResult AccountDetails()
        {
            return Json(account, JsonSerializerOptions.Default);
        }


        [Route("/account-statement")]
        public IActionResult AccountStatement()
        {
            return File("BankStatement.pdf", "application/pdf");
        }


        //http://localhost:5025/get-current-balance/100
        [Route("/get-current-balance/{accountNumber:int?}")]
        public IActionResult GetCurrentBalance(int accountNumber)
        {
            if(!HttpContext.Request.RouteValues.ContainsKey("accountNumber")) return NotFound("Account Number should be supplied");
            if (accountNumber != 1001) return BadRequest("Account Number should be 1001");

            if (accountNumber == 1001) return Ok("5000");

            return NotFound("Account id: " + accountNumber + " not found!");
        }
    }
}
