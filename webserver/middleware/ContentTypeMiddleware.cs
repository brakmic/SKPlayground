using Microsoft.AspNetCore.Http;
using SkPlayground.WebServer.Dtos;
using System.Text;
using System.Text.Json;

namespace SkPlayground.WebServer.Middleware;

public class ContentTypeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<Type> _processableTypes;

    public ContentTypeMiddleware(RequestDelegate next)
    {
        _next = next;
        _processableTypes = new List<Type> { typeof(HashRequest), typeof(CertificateInfo) };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var buffer = new MemoryStream();
        await context.Request.Body.CopyToAsync(buffer);
        buffer.Position = 0;

        using var reader = new StreamReader(buffer);
        var content = await reader.ReadToEndAsync();

        if (!string.IsNullOrEmpty(content) && TryDeserialize(content, out object deserializedObject))
        {
            ReplaceRequestBody(context, deserializedObject);
        }
        else
        {
            // Reset the buffer position for the next middleware
            buffer.Position = 0;
            context.Request.Body = buffer;
        }

        await _next(context);
    }

    private void ReplaceRequestBody(HttpContext context, object deserializedObject)
    {
        var newContent = JsonSerializer.Serialize(deserializedObject);
        var newBody = new MemoryStream(Encoding.UTF8.GetBytes(newContent));
        context.Request.Body = newBody;
        context.Request.ContentType = "application/json";
    }

    private bool TryDeserialize(string content, out object deserializedObject)
    {
        foreach (var type in _processableTypes)
        {
            try
            {
                deserializedObject = JsonSerializer.Deserialize(content, type);
                return true;
            }
            catch (JsonException)
            {
                // Continue to the next type
            }
        }

        // Check if content is a JSON object or array
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind == JsonValueKind.Object || doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                deserializedObject = null;
                return false;
            }
        }
        catch (JsonException)
        {
            // Not a JSON object or array, so it could be a raw string
            deserializedObject = new HashRequest { Input = content };
            return true;
        }

        deserializedObject = null;
        return false;
    }
}


