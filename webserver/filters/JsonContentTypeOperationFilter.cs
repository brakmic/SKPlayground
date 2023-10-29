using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkPlayground.WebServer.Filters;

public class JsonContentTypeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody != null)
        {
            var content = operation.RequestBody.Content;
            if (content.ContainsKey("text/plain"))
            {
               content.Remove("text/plain");
            }
            if (!content.ContainsKey("application/json"))
            {
                content.Add("application/json", new OpenApiMediaType());
            }
        }

        foreach (var response in operation.Responses.Values)
        {
            var content = response.Content;
            if (content.ContainsKey("text/plain"))
            {
               content.Remove("text/plain");
            }
            if (!content.ContainsKey("application/json"))
            {
                content.Add("application/json", new OpenApiMediaType());
            }
        }
    }
}
