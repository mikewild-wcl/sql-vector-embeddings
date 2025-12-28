using System.Text.Json.Serialization;

namespace Sql.Vector.Embeddings.Ingestion.Functions.Models;

public record UriListRequest(
    //IReadOnlyCollection<Uri> Uris
    )
{
    [JsonPropertyName("uris")]
    public IReadOnlyCollection<Uri> Uris { get; init; } = [];
}
