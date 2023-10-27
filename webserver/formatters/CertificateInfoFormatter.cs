using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using SkPlayground.WebServer.Dtos;

namespace SkPlayground.WebServer.Formatters;

public class CertificateInfoFormatter : TextInputFormatter
{
  public CertificateInfoFormatter()
  {
    SupportedMediaTypes.Add("application/json");
    SupportedEncodings.Add(System.Text.Encoding.UTF8);
  }

  protected override bool CanReadType(Type type)
  {
    if (type == typeof(CertificateInfo))
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
      var obj = JsonSerializer.Deserialize<CertificateInfo>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      return await InputFormatterResult.SuccessAsync(obj);
    }
    catch
    {
      return await InputFormatterResult.FailureAsync();
    }
  }
}
