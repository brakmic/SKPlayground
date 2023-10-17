namespace SkPlayground.Models;

public class Fact
{
  public string Text { get; }
  public string Id { get; } = Guid.NewGuid().ToString();
  public string Description { get; }
  public string AdditionalMetadata { get; }

  public Fact(string text, string description, string additionalMetadata)
  {
    Text = text;
    Description = description;
    AdditionalMetadata = additionalMetadata;
  }
}
