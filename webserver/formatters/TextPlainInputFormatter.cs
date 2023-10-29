using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;

namespace SkPlayground.WebServer.Formatters;

public class TextPlainInputFormatter : TextInputFormatter
{
    public TextPlainInputFormatter()
    {
        SupportedMediaTypes.Add("text/plain");
        SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        SupportedEncodings.Add(UTF16EncodingLittleEndian);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
        var content = await reader.ReadToEndAsync();
        return await InputFormatterResult.SuccessAsync(content);
    }
}
