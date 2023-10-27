using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using SkPlayground.WebServer.Dtos;

namespace SkPlayground.WebServer.Formatters;

public class InputDataFormatter : TextInputFormatter
{
  public InputDataFormatter()
  {
    SupportedMediaTypes.Add("text/plain");
    SupportedMediaTypes.Add("application/json");
    SupportedEncodings.Add(UTF8EncodingWithoutBOM);
    SupportedEncodings.Add(UTF16EncodingLittleEndian);
  }

  public override async Task<InputFormatterResult> ReadRequestBodyAsync(
      InputFormatterContext context, Encoding effectiveEncoding)
  {
    var request = context.HttpContext.Request;
    using var reader = new StreamReader(request.Body, effectiveEncoding);
    try
    {
      var content = await reader.ReadToEndAsync();
      if (string.IsNullOrEmpty(content))
      {
        return await InputFormatterResult.NoValueAsync();
      }
      content = content.Trim('"');
      return InputFormatterResult.Success(content);
    }
    catch
    {
      return InputFormatterResult.Failure();
    }
  }
}
