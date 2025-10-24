using Microsoft.AspNetCore.Mvc;

namespace MyFirstApp.Controllers
{
    public class FileController : Controller
    {
        [Route("file-download")]
        public IActionResult FileDownload()
        {
            //return new VirtualFileResult("Sample.pdf", "application/pdf");
            return File("Sample.pdf", "application/pdf");
        }

        [Route("file-download2")]
        public IActionResult FileDownload2()
        {
            //return new PhysicalFileResult(@"C:\Projects\LearningControllers\LearningControllers\wwwroot\Sample.pdf", "application/pdf");
            return PhysicalFile(@"C:\Projects\LearningControllers\LearningControllers\wwwroot\Sample.pdf", "application/pdf");
        }

        [Route("file-download3")]
        public IActionResult FileDownload3()
        {
            byte[] bytes = System.IO.File.ReadAllBytes(@"C:\Projects\LearningControllers\LearningControllers\wwwroot\Sample.pdf");
            //return new FileContentResult(bytes, "application/pdf");
            return File(bytes, "application/pdf");
        }
    }
}
