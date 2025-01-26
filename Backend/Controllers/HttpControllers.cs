using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace YourNamespace.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IFileProvider _fileProvider;

        public HomeController(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var fileInfo = _fileProvider.GetFileInfo("index.html");
            var readStream = fileInfo.CreateReadStream();
            return new FileStreamResult(readStream, "text/html");
        }
    }
}
