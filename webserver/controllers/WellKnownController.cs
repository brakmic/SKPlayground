using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace SkPlayground.Controllers
{
  [ApiController]
  [Route(".well-known")]
  public class WellKnownController : ControllerBase
  {
        [HttpGet("ai-plugin.json")]
        [SwaggerOperation(OperationId = "GetAiPluginJson")]
        [Produces("application/json")]
            public async Task<IActionResult> GetAiPluginJson()
        {
          var request = HttpContext.Request;
          // when running behind a secured URL, set protocol to HTTPS
          var host = $"{request.Protocol}://{request.Host}";

          var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "webserver", "config", "ai-plugin.json");
          if (!System.IO.File.Exists(jsonFilePath))
          {
            return NotFound();
          }

          var json = await System.IO.File.ReadAllTextAsync(jsonFilePath);
          json = json.Replace("{url}", $"{host}");
          return Content(json, "application/json");
        }
  }
}
