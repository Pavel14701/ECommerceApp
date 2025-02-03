using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

public class HideSwaggerEndpointsMiddleware
{
    private readonly RequestDelegate _next;
    public HideSwaggerEndpointsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(
            "/swagger", StringComparison.OrdinalIgnoreCase
        ))
        {
            var originalBodyStream = context.Response.Body;
            using var newBodyStream = new MemoryStream();
            context.Response.Body = newBodyStream;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var swaggerJson = await new StreamReader(
                context.Response.Body
            ).ReadToEndAsync();
            var modifiedSwaggerJson = ExcludeEndpointsByNamespace(
                swaggerJson, "NoApi.Controllers"
            );
            var bytes = Encoding.UTF8.GetBytes(modifiedSwaggerJson);

            context.Response.Body = originalBodyStream;
            context.Response.ContentLength = bytes.Length;
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
        else
        {
            await _next(context);
        }
    }

    private string ExcludeEndpointsByNamespace(string swaggerJson, string namespaceToHide)
    {
        JObject swaggerDoc;
        try
        {
            swaggerDoc = JObject.Parse(swaggerJson);
        }
        catch (JsonReaderException)
        {
            return swaggerJson;
        }
        if (swaggerDoc["paths"] == null)
        {
            return swaggerJson;
        }
        var pathsToRemove = new List<JToken>();
        foreach (
            var path in swaggerDoc["paths"]?.Children<JProperty>()
            ?? Enumerable.Empty<JProperty>()
        )
        {
            var pathItem = path.Value;
            foreach (
                var operation in pathItem?.Children<JProperty>() 
                ?? Enumerable.Empty<JProperty>()
            )
            {
                var tags = operation.Value["tags"];
                if (
                    tags != null && tags.Any(
                        tag => tag.ToString().StartsWith(namespaceToHide)
                    )
                )
                {
                    pathsToRemove.Add(path);
                    break;
                }
            }
        }
        foreach (var path in pathsToRemove)
        {
            path?.Remove();
        }
        return swaggerDoc.ToString();
    }
}
