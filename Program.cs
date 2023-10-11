using System.Collections.Immutable;
using System.CommandLine;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Events;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SemanticFunctions;
using SkPlayground.Plugins;
using static Microsoft.SemanticKernel.SemanticFunctions.PromptTemplateConfig;
using SkPlayground.Extensions;

namespace SkPlayground;
class Program
{
  private static IConfiguration? Configuration { get; set; }

  public static async Task Main(string[] args)
  {
    var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFiles(Directory.GetCurrentDirectory(), "appsettings.*.json", optional: false, reloadOnChange: true);

    Configuration = builder.Build();

    var fileOption = new Option<FileInfo>(
        new[] { "--input", "-i" },
        "Path to the input file to be processed");

    var functionOption = new Option<string>(
        new[] { "--function", "-f" },
        "The function to be executed.");

    var rootCommand = new RootCommand
        {
            fileOption,
            functionOption
        };

    rootCommand.SetHandler(
        //Run,
        //RunWithActionPlanner,
        //RunWithSequentialPlanner,
        RunWithHooks, /* this example uses the native function "ExecuteGet" from HttpPlugin */
        //RunWithHooks2, /* this example uses a semantic function and the Markdown converter function */
        fileOption, functionOption
    );

    await rootCommand.InvokeAsync(args);
  }

  private static async Task Run(FileInfo file, string function)
  {
    var kernelSettings = KernelSettings.LoadSettings();

    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
              .SetMinimumLevel(kernelSettings.LogLevel ?? LogLevel.Warning)
              .AddConsole()
              .AddDebug();
    });

    IKernel kernel = new KernelBuilder()
        .WithLoggerFactory(loggerFactory)
        .WithCompletionService(kernelSettings)
        .Build();

    if (kernelSettings.EndpointType == EndpointTypes.TextCompletion)
    {
      var rootDirectory = Configuration!.GetSection("SkillSettings:Root").Get<string>();
      var pluginDirectories = Configuration.GetSection("SkillSettings:Plugins").Get<string[]>();

      var skillsRoot = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory!);
      var skillImport = kernel.ImportSemanticSkillFromDirectory(skillsRoot, pluginDirectories!);

      string description = await File.ReadAllTextAsync(file.FullName);
      var context = new ContextVariables();

      string key = "input";
      context.Set(key, description);

      var result = await kernel.RunAsync(context, skillImport[function]);
      Console.WriteLine(result);
    }
  }
  private static async Task RunWithActionPlanner(FileInfo file, string function)
  {
    var kernelSettings = KernelSettings.LoadSettings();

    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
              .SetMinimumLevel(kernelSettings.LogLevel ?? LogLevel.Warning)
              .AddConsole()
              .AddDebug();
    });

    IKernel kernel = new KernelBuilder()
        .WithLoggerFactory(loggerFactory)
        .WithCompletionService(kernelSettings)
        .Build();

    if (kernelSettings.EndpointType == EndpointTypes.TextCompletion)
    {

      var rootDirectory = Configuration!.GetSection("SkillSettings:Root").Get<string>();
      var pluginDirectories = Configuration.GetSection("SkillSettings:Plugins").Get<string[]>();

      var skillsRoot = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory!);
      var skillImport = kernel.ImportSemanticSkillFromDirectory(skillsRoot, pluginDirectories!);

      var httpPlugin = kernel.ImportSkill(new HttpPlugin(), nameof(HttpPlugin));

      var planner = new ActionPlanner(kernel);
      var ask = await File.ReadAllTextAsync(file.FullName);
      var plan = await planner.CreatePlanAsync(ask);

      Console.WriteLine($"\nPLAN:\n{plan.ToSafePlanString()}");

      var result = await plan.InvokeAsync();

      Console.WriteLine($"\nRESULT: {result}");
    }
  }

  private static async Task RunWithSequentialPlanner(FileInfo file, string function)
  {
    var kernelSettings = KernelSettings.LoadSettings();

    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
              .SetMinimumLevel(kernelSettings.LogLevel ?? LogLevel.Warning)
              .AddConsole()
              .AddDebug();
    });

    IKernel kernel = new KernelBuilder()
        .WithLoggerFactory(loggerFactory)
        .WithCompletionService(kernelSettings)
        .Build();

    if (kernelSettings.EndpointType == EndpointTypes.TextCompletion)
    {
      var rootDirectory = Configuration!.GetSection("SkillSettings:Root").Get<string>();
      var pluginDirectories = Configuration.GetSection("SkillSettings:Plugins").Get<string[]>();

      var skillsRoot = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory!);
      var skillImport = kernel.ImportSemanticSkillFromDirectory(skillsRoot, pluginDirectories!);
      var keyGenPlugin = kernel.ImportSkill(new KeyAndCertGenerator(), nameof(KeyAndCertGenerator));
      var secretsPlugin = kernel.ImportSkill(new SecretYamlUpdater(), nameof(SecretYamlUpdater));

      var planner = new SequentialPlanner(kernel);
      var ask = await File.ReadAllTextAsync(file.FullName);
      var plan = await planner.CreatePlanAsync(ask);

      Console.WriteLine($"\nPLAN:\n{plan.ToSafePlanString()}");

      var result = await plan.InvokeAsync();

      Console.WriteLine($"\nRESULT: {result}");
    }
  }

  private static async Task RunWithHooks(FileInfo file, string function)
  {
    #region Kernel Setup
    var kernelSettings = KernelSettings.LoadSettings();

    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
              .SetMinimumLevel(kernelSettings.LogLevel ?? LogLevel.Warning)
              .AddConsole()
              .AddDebug();
    });

    IKernel kernel = new KernelBuilder()
        .WithLoggerFactory(loggerFactory)
        .WithCompletionService(kernelSettings)
        .Build();
    #endregion

    if (kernelSettings.EndpointType == EndpointTypes.TextCompletion)
    {
      // import the plugin that contains native functions for sending http queries
      var httpPlugin = kernel.ImportSkill(new HttpPlugin(), nameof(HttpPlugin));

      // configure hooks
      kernel.FunctionInvoking += OnFunctionInvoking;
      kernel.FunctionInvoked += OnFunctionInvoked;

      // We want to download this document:
      // "The Development of the C Language"
      // that is located here:
      var ask = "https://www.bell-labs.com/usr/dmr/www/chist.html";

      // We send our ASK to the Kernel
      var result = await kernel.RunAsync(ask, httpPlugin.ToImmutableDictionary()["ExecuteGet"]);

      Console.WriteLine($"\nRESULT: {result}");
    }
  }

  private static async Task RunWithHooks2(FileInfo file, string function)
  {
    #region Kernel Setup
    var kernelSettings = KernelSettings.LoadSettings();

    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
              .SetMinimumLevel(kernelSettings.LogLevel ?? LogLevel.Warning)
              .AddConsole()
              .AddDebug();
    });

    IKernel kernel = new KernelBuilder()
        .WithLoggerFactory(loggerFactory)
        .WithCompletionService(kernelSettings)
        .Build();
    #endregion

    if (kernelSettings.EndpointType == EndpointTypes.TextCompletion)
    {
      // this is the "config.json" of our semantic function
      var promptConfig = new PromptTemplateConfig
      {
        Schema = 1,
        Type = "completion",
        Description = "Ask something about a person",
        Completion =
        {
            MaxTokens = 1000,
            Temperature = 0.5,
            TopP = 0.0,
            PresencePenalty = 0.0,
            FrequencyPenalty = 0.0
        },
        Input =
        {
            Parameters = new List<InputParameter>
            {
                new InputParameter
                {
                    Name = "input",
                    Description = "Person's names",
                    DefaultValue = ""
                }
            }
        }
      };
      // we define the semantic function
      var askAbutPerson = kernel.CreateSemanticFunction(
        "Write a short document about {{$input}}. It must have titles and paragraphs.",
        config: promptConfig,
        functionName: "askAboutPerson");

      // configure hooks
      kernel.FunctionInvoking += (object? sender, FunctionInvokingEventArgs e) =>
      {
        Console.WriteLine($"{e.FunctionView.Name}");
      };
      kernel.FunctionInvoked += (object? sender, FunctionInvokedEventArgs e) =>
      {
        // convert the result to Markdown
        Console.WriteLine($"{ConvertToMarkdown(e.SKContext.Result)}");
      };

      // We send our ASK to the Kernel
      var result = await kernel.RunAsync("Denis Ritchie", askAbutPerson);
    }
  }

  /// <summary>
  /// A helper function that converts plain text to markdown
  /// </summary>
  /// <param name="plainText">Plain text string</param>
  /// <returns>Content as Markdown string</returns>
  private static string ConvertToMarkdown(string text)
  {
    // This flag is used to differentiate between the document title and the subsequent titles
    bool isFirstTitle = true;

    // Identify and replace titles in the text.
    // The pattern (.*\w)\n looks for any line that ends with a word character (\w),
    // ensuring it's a title line and not a blank line or a line of text that's part of a paragraph.
    var result = Regex.Replace(text, @"(.*\w)\n", match =>
    {
      // For the first title, we prefix it with a single hash (#)
      if (isFirstTitle)
      {
        isFirstTitle = false;
        return $"# {match.Groups[1].Value}\n\n";
      }
      else
      {
        return $"## {match.Groups[1].Value}\n\n";
      }
    });

    return result;
  }

  //below are the pre/post event handlers
  private static void OnFunctionInvoked(object? sender, FunctionInvokedEventArgs e)
  {
    Console.WriteLine($"{e.FunctionView.Name}");
  }

  private static void OnFunctionInvoking(object? sender, FunctionInvokingEventArgs e)
  {
    // By default, the input variable is called INPUT.
    // However, the native function "ExecuteGet" from the Plugin expects "url" as its only argument.
    // So we must "hook into" the flow and manipulate variables before SK calls the "HttpGet" method.
    e.SKContext.Variables.Remove("INPUT", out var urlVal);
    e.SKContext.Variables.TryAdd("url", urlVal!);
    Console.WriteLine($"{e.FunctionView.Name}");
  }
}
