using System.Text.Json.Serialization;

namespace Sql.Vector.Embeddings.Ingestion.Functions.Models;

public record UriListRequest(
    IReadOnlyCollection<Uri> Uris)
{
    public string? Test { get; set; }

    [JsonPropertyName("test2")]
    public string? Test2 { get; set; }

    [JsonPropertyName("uris")]
    IReadOnlyCollection<Uri>? MoreUris { get; init; }
}
