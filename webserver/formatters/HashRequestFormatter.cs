using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using SkPlayground.WebServer.Dtos;

namespace SkPlayground.WebServer.Formatters;

public class HashRequestFormatter : TextInputFormatter
{
    public HashRequestFormatter()
    {
        SupportedMediaTypes.Add("application/json");
        SupportedMediaTypes.Add("text/plain");
        SupportedEncodings.Add(System.Text.Encoding.UTF8);
    }

    protected override bool CanReadType(Type type)
    {
        if (type == typeof(HashRequestFormatter))
        {
            return base.CanReadType(type);
        }
        return false;
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, System.Text.Encoding encoding)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

        using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
        try
        {
            var content = await reader.ReadToEndAsync();
            var obj = JsonSerializer.Deserialize<HashRequestFormatter>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return await InputFormatterResult.SuccessAsync(obj);
        }
        catch
        {
            return await InputFormatterResult.FailureAsync();
        }
    }
}
