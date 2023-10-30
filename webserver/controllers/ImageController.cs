using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;
using System.Threading.Tasks;

namespace SkPlayground.Controllers
{
  [ApiController]
  [Route("")]
  public class ImageController : ControllerBase
  {
        [HttpGet("logo.png")]
        [SwaggerOperation(OperationId = "GetLogoImage")]
        [Produces("image/png")]
        public Task<IActionResult> GetLogoImage()
        {
          var imageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "webserver", "assets", "images", "logo.png");
          if (!System.IO.File.Exists(imageFilePath))
          {
            return Task.FromResult<IActionResult>(NotFound());
          }

          var imageStream = new FileStream(imageFilePath, FileMode.Open);
          return Task.FromResult<IActionResult>(new FileStreamResult(imageStream, "image/png"));
        }
  }
}
