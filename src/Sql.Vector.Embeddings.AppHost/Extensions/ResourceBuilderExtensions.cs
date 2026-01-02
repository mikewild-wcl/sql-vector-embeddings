namespace Sql.Vector.Embeddings.AppHost.Extensions;

internal static class ResourceBuilderExtensions
{
    extension(IResourceBuilder<OllamaResource> builder)
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "<Pending>")]
        internal IResourceBuilder<OllamaResource> WithGPUSupportIfVendorParameterProvided(
            string? ollamaGpuVendorParameter = default)
        {
            var vendor = Enum.TryParse<OllamaGpuVendor>(ollamaGpuVendorParameter, out var gpuVendor)
                ? gpuVendor
                : default(OllamaGpuVendor?);

            return (vendor is not null)
                ? builder!.WithGPUSupport(vendor.Value)
                : builder!;
        }
    }
}
