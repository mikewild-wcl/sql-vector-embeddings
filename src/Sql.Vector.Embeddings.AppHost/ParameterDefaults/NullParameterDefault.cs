using Aspire.Hosting.Publishing;

namespace Sql.Vector.Embeddings.AppHost.ParameterDefaults;

internal sealed class NullParameterDefault : ParameterDefault
{
    public override string GetDefaultValue()
    {
        return null!;
    }

    public override void WriteToManifest(ManifestPublishingContext context)
    {
    }
}
