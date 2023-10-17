using Microsoft.Extensions.Configuration;

namespace SkPlayground.Extensions;

public static class ConfigurationBuilderExtensions
{
  public static IConfigurationBuilder AddJsonFiles(this IConfigurationBuilder builder, string directory, string searchPattern, bool optional, bool reloadOnChange)
  {
    foreach (var file in Directory.EnumerateFiles(directory, searchPattern))
    {
      builder = builder.AddJsonFile(file, optional: optional, reloadOnChange: reloadOnChange);
    }
    return builder;
  }
}
