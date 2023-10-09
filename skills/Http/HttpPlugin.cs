using System.ComponentModel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace SkPlayground.Plugins;

public class HttpPlugin
{
  private static readonly string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
  private readonly HttpClient _httpClient = new HttpClient();

  [SKFunction, Description("Executes a GET request to retrieve a document from the given URL")]
  [SKParameter("url", "The URL to send the request to")]
  public async Task<string> ExecuteGetAsync(SKContext context)
  {
    var url = context.Variables["url"].ToString();
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.Add("User-Agent", userAgent);
    HttpResponseMessage response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
  }

  [SKFunction, Description("Executes a POST request to create a new resource at given URL")]
  [SKParameter("url", "The URL to send the request to")]
  [SKParameter("data", "The data to send in the request body")]
  public async Task<string> ExecutePostAsync(SKContext context)
  {
    var url = context.Variables["url"].ToString();
    var data = context.Variables["data"].ToString();
    HttpContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
    var request = new HttpRequestMessage(HttpMethod.Post, url)
    {
      Content = content
    };
    request.Headers.Add("User-Agent", userAgent);
    HttpResponseMessage response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
  }

  [SKFunction, Description("Executes a PUT request to update an existing resource at given URL")]
  [SKParameter("url", "The URL to send the request to")]
  [SKParameter("data", "The data to send in the request body")]
  public async Task<string> ExecutePutAsync(SKContext context)
  {
    var url = context.Variables["url"].ToString();
    var data = context.Variables["data"].ToString();
    HttpContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
    var request = new HttpRequestMessage(HttpMethod.Put, url)
    {
      Content = content
    };
    request.Headers.Add("User-Agent", userAgent);
    HttpResponseMessage response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
  }

}