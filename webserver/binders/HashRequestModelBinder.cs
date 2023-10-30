using Microsoft.AspNetCore.Mvc.ModelBinding;
using SkPlayground.WebServer.Dtos;

namespace SkPlayground.WebSever.Binders;

using Microsoft.Net.Http.Headers;

public class HashRequestModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var httpContext = bindingContext.HttpContext;
        var contentType = httpContext.Request.ContentType;
        if (string.IsNullOrWhiteSpace(contentType)) contentType = "text/plain; charset=utf-8";

        var mediaType = MediaTypeHeaderValue.Parse(contentType).MediaType.Value;

        if (mediaType!.Equals("text/plain", StringComparison.OrdinalIgnoreCase) ||
            mediaType!.Equals("application/json", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(httpContext.Request.Body);
            var content = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(content))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var hashRequest = new HashRequest { Input = content };
            bindingContext.Result = ModelBindingResult.Success(hashRequest);
        }
    }
}






