using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace SkPlayground.Plugins;

//
// Summary:
//     TextMemoryExPlugin provides a plugin to save or recall information from the long
//     or short term memory. This plugin is based n TextMemoryPlugin from Microsoft.
//     Its SaveAsync method accepts two additional arguments: description and additionalMetadata
public sealed class TextMemoryExPlugin
{
  //
  // Summary:
  //     Name of the context variable used to specify which memory collection to use.
  public const string CollectionParam = "collection";

  //
  // Summary:
  //     Name of the context variable used to specify memory search relevance score.
  public const string RelevanceParam = "relevance";

  //
  // Summary:
  //     Name of the context variable used to specify a unique key associated with stored
  //     information.
  public const string KeyParam = "key";

  //
  // Summary:
  //     Name of the context variable used to specify the number of memories to recall
  public const string LimitParam = "limit";

  private const string DefaultCollection = "generic";

  private const double DefaultRelevance = 0.0;

  private const int DefaultLimit = 1;

  private readonly ISemanticTextMemory _memory;

  //
  // Summary:
  //     Creates a new instance of the TextMemoryPlugin
  public TextMemoryExPlugin(ISemanticTextMemory memory)
  {
    _memory = memory;
  }

  //
  // Summary:
  //     Key-based lookup for a specific memory
  //
  // Parameters:
  //   collection:
  //     Memories collection associated with the memory to retrieve
  //
  //   key:
  //     The key associated with the memory to retrieve.
  //
  //   loggerFactory:
  //     The Microsoft.Extensions.Logging.ILoggerFactory to use for logging. If null,
  //     no logging will be performed.
  //
  //   cancellationToken:
  //     The System.Threading.CancellationToken to monitor for cancellation requests.
  //     The default is System.Threading.CancellationToken.None.
  [SKFunction]
  [Description("Key-based lookup for a specific memory")]
  public async Task<string> RetrieveAsync([SKName("collection")][Description("Memories collection associated with the memory to retrieve")][DefaultValue("generic")] string? collection,
  [SKName("key")][Description("The key associated with the memory to retrieve")] string key,
  ILoggerFactory? loggerFactory, CancellationToken cancellationToken = default(CancellationToken))
  {
    if (string.IsNullOrWhiteSpace(collection) || string.IsNullOrWhiteSpace(key))
    {
      throw new Exception("collection and key must not be empty");
    }
    loggerFactory?.CreateLogger(typeof(TextMemoryExPlugin)).LogDebug("Recalling memory with key '{0}' from collection '{1}'", key, collection);
    return (await _memory.GetAsync(collection, key, withEmbedding: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))?.Metadata.Text ?? string.Empty;
  }

  //
  // Summary:
  //     Semantic search and return up to N memories related to the input text
  //
  // Parameters:
  //   input:
  //     The input text to find related memories for.
  //
  //   collection:
  //     Memories collection to search.
  //
  //   relevance:
  //     The relevance score, from 0.0 to 1.0, where 1.0 means perfect match.
  //
  //   limit:
  //     The maximum number of relevant memories to recall.
  //
  //   loggerFactory:
  //     The Microsoft.Extensions.Logging.ILoggerFactory to use for logging. If null,
  //     no logging will be performed.
  //
  //   cancellationToken:
  //     The System.Threading.CancellationToken to monitor for cancellation requests.
  //     The default is System.Threading.CancellationToken.None.
  [SKFunction]
  [Description("Semantic search and return up to N memories related to the input text")]
  public async Task<string> RecallAsync([Description("The input text to find related memories for")] string input,
  [SKName("collection")][Description("Memories collection to search")][DefaultValue("generic")] string collection,
  [SKName("relevance")][Description("The relevance score, from 0.0 to 1.0, where 1.0 means perfect match")][DefaultValue(0.0)] double? relevance,
  [SKName("limit")][Description("The maximum number of relevant memories to recall")][DefaultValue(1)] int? limit,
  ILoggerFactory? loggerFactory, CancellationToken cancellationToken = default(CancellationToken))
  {
    if (string.IsNullOrWhiteSpace(collection))
    {
      throw new Exception("collection and key must not be empty");
    }

    relevance.GetValueOrDefault();
    if (!relevance.HasValue)
    {
      double value = 0.0;
      relevance = value;
    }

    limit.GetValueOrDefault();
    if (!limit.HasValue)
    {
      int value2 = 1;
      limit = value2;
    }

    ILogger? logger = loggerFactory?.CreateLogger(typeof(TextMemoryExPlugin));
    logger?.LogDebug("Searching memories in collection '{0}', relevance '{1}'", collection, relevance);
    var results = _memory.SearchAsync(collection, input, limit.Value, relevance.Value, withEmbeddings: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: true);

    var memories = new List<MemoryQueryResult>();
    await foreach (var result in results.WithCancellation(cancellationToken))
    {
      memories.Add(result);
    }

    if (memories.Count == 0)
    {
      logger?.LogWarning("Memories not found in collection: {0}", collection);
      return string.Empty;
    }

    logger?.LogTrace("Done looking for memories in collection '{0}')", collection);
    return (limit == 1) ? memories[0].Metadata.Text : JsonSerializer.Serialize(memories.Select((MemoryQueryResult x) => x.Metadata.Text));
  }

  //
  // Summary:
  //     Save information to semantic memory
  //
  // Parameters:
  //   input:
  //     The information to save
  //
  //   description:
  //     description
  //
  //   additionalMetadata:
  //    additional metadata
  //
  //   collection:
  //     Memories collection associated with the information to save
  //
  //   key:
  //     The key associated with the information to save
  //
  //   loggerFactory:
  //     The Microsoft.Extensions.Logging.ILoggerFactory to use for logging. If null,
  //     no logging will be performed.
  //
  //   cancellationToken:
  //     The System.Threading.CancellationToken to monitor for cancellation requests.
  //     The default is System.Threading.CancellationToken.None.
  [SKFunction]
  [Description("Save information to semantic memory")]
  public async Task SaveAsync([Description("The information to save")] string input,
  [Description("Description")][DefaultValue(null)] string description,
  [Description("Additional Metadata")][DefaultValue(null)] string additionalMetadata,
  [SKName("collection")][Description("Memories collection associated with the information to save")][DefaultValue("generic")] string collection,
  [SKName("key")][Description("The key associated with the information to save")] string key,
  ILoggerFactory? loggerFactory, CancellationToken cancellationToken = default(CancellationToken))
  {
    if (string.IsNullOrWhiteSpace(collection) || string.IsNullOrWhiteSpace(key))
    {
      throw new Exception("collection and key must not be empty");
    }
    loggerFactory?.CreateLogger(typeof(TextMemoryExPlugin)).LogDebug("Saving memory to collection '{0}'", collection);
    await _memory.SaveInformationAsync(collection, input, key, description, additionalMetadata, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
  }

  //
  // Summary:
  //     Remove specific memory
  //
  // Parameters:
  //   collection:
  //     Memories collection associated with the information to save
  //
  //   key:
  //     The key associated with the information to save
  //
  //   loggerFactory:
  //     The Microsoft.Extensions.Logging.ILoggerFactory to use for logging. If null,
  //     no logging will be performed.
  //
  //   cancellationToken:
  //     The System.Threading.CancellationToken to monitor for cancellation requests.
  //     The default is System.Threading.CancellationToken.None.
  [SKFunction]
  [Description("Remove specific memory")]
  public async Task RemoveAsync([SKName("collection")][Description("Memories collection associated with the information to save")][DefaultValue("generic")] string collection, [SKName("key")][Description("The key associated with the information to save")] string key, ILoggerFactory? loggerFactory, CancellationToken cancellationToken = default(CancellationToken))
  {
    if (string.IsNullOrWhiteSpace(collection) || string.IsNullOrWhiteSpace(key))
    {
      throw new Exception("collection and key must not be empty");
    }
    loggerFactory?.CreateLogger(typeof(TextMemoryExPlugin)).LogDebug("Removing memory from collection '{0}'", collection);
    await _memory.RemoveAsync(collection, key, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
  }
}
