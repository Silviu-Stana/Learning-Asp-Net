using Microsoft.AspNetCore.Mvc;
using LearningControllers.Models;

namespace MyFirstApp.Controllers
{
    public class HomeController: Controller
    {
        [Route("home")]
        [Route("/")]
        public ContentResult Index()
        {
            return Content("<h1>Index Page</h1>", "text/html");
        }

        [Route("person")]
        public JsonResult Person()
        {
            Person person = new Person() { Id=Guid.NewGuid(), FirstName="James", LastName="Bond", Age=25};
            return Json(person);
        }

        [Route("file-download")]
        public VirtualFileResult FileDownload()
        {
            //return new VirtualFileResult("Sample.pdf", "application/pdf");
            return File("Sample.pdf", "application/pdf");
        }

        [Route("file-download2")]
        public PhysicalFileResult FileDownload2()
        {
            //return new PhysicalFileResult(@"C:\Projects\LearningControllers\LearningControllers\wwwroot\Sample.pdf", "application/pdf");
            return PhysicalFile(@"C:\Projects\LearningControllers\LearningControllers\wwwroot\Sample.pdf", "application/pdf");
        }

        [Route("file-download3")]
        public FileContentResult FileDownload3()
        {
            byte[] bytes = System.IO.File.ReadAllBytes(@"C:\Projects\LearningControllers\LearningControllers\wwwroot\Sample.pdf");
            //return new FileContentResult(bytes, "application/pdf");
            return File(bytes, "application/pdf");
        }
    }
}
