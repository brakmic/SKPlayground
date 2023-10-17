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
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Connectors.Memory.Weaviate;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Connectors.Memory.Sqlite;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.TemplateEngine.Basic;
using Microsoft.SemanticKernel.Memory;
using SkPlayground.Models;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using SkPlayground.Util.Helpers;

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
        //RunWithHooks, /* this example uses the native function "ExecuteGet" from HttpPlugin */
        //RunWithHooks2, /* this example uses a semantic function and the Markdown converter function */
        RunWithRag,
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
      var rootDirectory = Configuration!.GetSection("PluginSettings:Root").Get<string>();
      var pluginDirectories = Configuration.GetSection("PluginSettings:Plugins").Get<string[]>();

      var pluginsRoot = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory!);
      var pluginImport = kernel.ImportSemanticFunctionsFromDirectory(pluginsRoot, pluginDirectories!);

      string description = await File.ReadAllTextAsync(file.FullName);
      var context = new ContextVariables();

      string key = "input";
      context.Set(key, description);

      var result = await kernel.RunAsync(context, pluginImport[function]);
      Console.WriteLine(result.GetValue<string>());
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

      var rootDirectory = Configuration!.GetSection("PluginSettings:Root").Get<string>();
      var pluginDirectories = Configuration.GetSection("PluginSettings:Plugins").Get<string[]>();

      var pluginsRoot = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory!);
      var pluginImport = kernel.ImportSemanticFunctionsFromDirectory(pluginsRoot, pluginDirectories!);

      var httpPlugin = kernel.ImportFunctions(new HttpPlugin(), nameof(HttpPlugin));

      var planner = new ActionPlanner(kernel);
      var ask = await File.ReadAllTextAsync(file.FullName);
      var plan = await planner.CreatePlanAsync(ask);

      Console.WriteLine($"\nPLAN:\n{plan.ToSafePlanString()}");

      var result = await plan.InvokeAsync(kernel.CreateNewContext());

      Console.WriteLine($"\nRESULT: {result.GetValue<string>()}");
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
      var rootDirectory = Configuration!.GetSection("PluginSettings:Root").Get<string>();
      var pluginDirectories = Configuration.GetSection("PluginSettings:Plugins").Get<string[]>();

      var pluginsRoot = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory!);
      var pluginImport = kernel.ImportSemanticFunctionsFromDirectory(pluginsRoot, pluginDirectories!);
      var keyGenPlugin = kernel.ImportFunctions(new KeyAndCertGenerator(), nameof(KeyAndCertGenerator));
      var secretsPlugin = kernel.ImportFunctions(new SecretYamlUpdater(), nameof(SecretYamlUpdater));

      var planner = new SequentialPlanner(kernel);
      var ask = await File.ReadAllTextAsync(file.FullName);
      var plan = await planner.CreatePlanAsync(ask);

      Console.WriteLine($"\nPLAN:\n{plan.ToSafePlanString()}");

      var result = await plan.InvokeAsync(kernel.CreateNewContext());

      Console.WriteLine($"\nRESULT: {result.GetValue<string>()}");
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
      var httpPlugin = kernel.ImportFunctions(new HttpPlugin(), nameof(HttpPlugin));

      // configure hooks
      kernel.FunctionInvoking += OnFunctionInvoking;
      kernel.FunctionInvoked += OnFunctionInvoked;

      // We want to download this document:
      // "The Development of the C Language"
      // that is located here:
      var ask = "https://www.bell-labs.com/usr/dmr/www/chist.html";
      // We send our ASK to the Kernel
      var result = await kernel.RunAsync(ask, httpPlugin["ExecuteGet"]);

      Console.WriteLine($"\nRESULT: {result.GetValue<string>()}");
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
  private static async Task RunWithRag(FileInfo file, string function)
  {
    var kernelSettings = KernelSettings.LoadSettings();

    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
              .SetMinimumLevel(kernelSettings.LogLevel ?? LogLevel.Warning)
              .AddConsole()
              .AddDebug();
    });

    IKernel kernel = Kernel.Builder
                     .WithLoggerFactory(loggerFactory)
                     .WithOpenAITextCompletionService(
                            modelId: kernelSettings.DeploymentOrModelId,
                            apiKey: kernelSettings.ApiKey,
                            orgId: kernelSettings.OrgId,
                            serviceId: kernelSettings.ServiceId)
                    .WithOpenAITextEmbeddingGenerationService(
                      modelId: "text-embedding-ada-002",
                      apiKey: kernelSettings.ApiKey,
                      orgId: kernelSettings.OrgId,
                      serviceId: kernelSettings.ServiceId
                    )
                    .Build();

    var rootDirectory = Configuration!.GetSection("PluginSettings:Root").Get<string>();
    var pluginDirectories = Configuration.GetSection("PluginSettings:Plugins").Get<string[]>();

    var pluginsRoot = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory!);
    var pluginImport = kernel.ImportSemanticFunctionsFromDirectory(pluginsRoot, pluginDirectories!);

    //var memoryStore = new WeaviateMemoryStore(endpoint: "http://localhost:8080/v1", loggerFactory: loggerFactory);
    //var memoryStore = new VolatileMemoryStore();
    //var memoryStore = new QdrantMemoryStore("http://localhost:6333", 1536, loggerFactory: loggerFactory);
    var memoryStore = await SqliteMemoryStore.ConnectAsync("memories.sqlite");

    // Create an embedding generator to use for semantic memory.
    var embeddingGenerator = new OpenAITextEmbeddingGeneration(modelId: "text-embedding-ada-002", apiKey: kernelSettings.ApiKey,
                                                                organization: kernelSettings.OrgId,
                                                                loggerFactory: loggerFactory);

    // The combination of the text embedding generator and the memory store makes up the 'SemanticTextMemory' object used to
    // store and retrieve memories.
    var textMemory = new MemoryBuilder()
                .WithLoggerFactory(loggerFactory)
                .WithTextEmbeddingGeneration(embeddingGenerator)
                .WithMemoryStore(memoryStore)
                .Build();

    // Alternatively, one could use SemanticTextMemory instance instead
    //SemanticTextMemory textMemory = new(memoryStore, embeddingGenerator);

    var memoryPlugin = new TextMemoryPlugin(textMemory);
    var memoryFunctions = kernel.ImportFunctions(memoryPlugin, "MemoryPlugin");

    var collection = "aboutme";

    // use this to wipe memory
    //await memoryStore.DeleteCollectionAsync(collection); return;

    bool exists = await memoryStore.DoesCollectionExistAsync(collection);

    if (!exists)
    {
      // there are two ways to populate the memory:
      // * with Kernel + Plugins
      await PopulateInterestingFacts(kernel, memoryFunctions, collection, FactHelper.GetFacts());
      // * with SemanticTextMemory
      //await PopulateInterestingFacts(textMemory, collection, FactHelper.GetFacts());
    }

    // * Three ways to use the memory

    // * 1. By using TextMemory methods
    //MemoryQueryResult? lookup = await textMemory.GetAsync(collection, "INSERT_ID_HERE");
    //IAsyncEnumerable<MemoryQueryResult> lookup = textMemory.SearchAsync(collection, "Do I have pets?", 1, minRelevanceScore: 0.50);

    // await foreach (var r in lookup)
    // {
    //   Console.WriteLine("Memory with key 'id': " + r.Metadata.Text ?? "ERROR: memory not found");
    // }
    //return;

    // * 2. By using Kernel and TextMemoryPlugin
    // var result = await kernel.RunAsync(memoryFunctions["Recall"], new()
    // {
    //   [TextMemoryPlugin.CollectionParam] = collection,
    //   [TextMemoryPlugin.LimitParam] = "1",
    //   [TextMemoryPlugin.RelevanceParam] = "0.79",
    //   ["input"] = "Which season do I prefer?"
    // });
    // Console.WriteLine($"Answer: {result.GetValue<string>()}");

    // * 3. By using the RAG Pattern (Retrieval-augmented Generation)
    //var answer = await RunMiniRAG(textMemory, kernel, collection, "Tell me something about my hobbies.");
    // Console.WriteLine(answer);

    await RunChat(textMemory, kernel, collection);

  }

  private static async Task RunChat(ISemanticTextMemory textMemory, IKernel kernel, string collection)
  {
    string input = string.Empty;
    while (true)
    {
      Console.Write("User: ");
      input = Console.ReadLine()!;

      var answer = await ApplyRAG(textMemory, kernel, collection, input);
      Console.WriteLine($"Assistant: {answer}");

      // Exit conditions
      if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
          input.Equals("quit", StringComparison.OrdinalIgnoreCase))
      {
        break;
      }
    }
  }

  private static async Task<string> ApplyRAG(ISemanticTextMemory memory, IKernel kernel, string collectionName, string input)
  {

    // Exit conditions
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
      return "Goodbye!";
    }

    var context = kernel.CreateNewContext();
    context.Variables.Add("user_input", input);

    // retrieve user-specific context based on the user input
    var searchResults = memory.SearchAsync(collectionName, input);
    var retrieved = new List<string>();
    await foreach (var item in searchResults)
    {
      retrieved.Add(item.Metadata.Text);
    }
    context.Variables.Add("user_context", string.Join(',', retrieved));

    // run SK function and give it the two variables, input and context
    var func = kernel.Functions.GetFunction("Assistant", "Chat");
    var answer = await kernel.RunAsync(func, context.Variables);

    return answer.GetValue<string>()!;

  }

  /// <summary>
  /// Populates the memory with facts about an entity
  /// </summary>
  /// <param name="kernel">Kernel</param>
  /// <param name="memoryFunctions">Memory Plugin</param>
  /// <param name="collection">Collection name</param>
  /// <param name="facts">List of facts</param>
  /// <returns>Enumerable containing KernelResults</returns>
  private static async Task<IEnumerable<KernelResult?>> PopulateInterestingFacts(IKernel kernel, IDictionary<string, ISKFunction> memoryFunctions, string collection, IEnumerable<Fact> facts)
  {
    var results = new List<KernelResult?>();
    foreach (var fact in facts)
    {
      var result = await kernel.RunAsync(memoryFunctions["Save"], new()
      {
        [TextMemoryPlugin.CollectionParam] = collection,
        [TextMemoryPlugin.KeyParam] = fact.Id,
        ["input"] = fact.Text
      });
      results.Add(result);
    }
    return results;
  }

  /// <summary>
  /// Populates the memory with facts about an entity
  /// </summary>
  /// <param name="memory">Memory</param>
  /// <param name="collection">Collection name</param>
  /// <param name="facts">List of facts</param>
  /// <returns></returns>Enumerable containing IDs of saved memory records<summary>
  private static async Task<IEnumerable<string>> PopulateInterestingFacts(ISemanticTextMemory memory, string collection, IEnumerable<Fact> facts)
  {
    var ids = new List<string>();
    // Iterate through the facts and populate the memory
    foreach (var fact in facts)
    {
      var id = await memory.SaveInformationAsync(
        collection: collection,
        text: fact.Text,
        id: fact.Id,
        description: fact.Description,
        additionalMetadata: fact.AdditionalMetadata);
      ids.Add(id);
    }
    return ids;
  }

  /// <summary>
  /// Convert plain text to markdown
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
