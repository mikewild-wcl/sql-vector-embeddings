namespace Sql.Vector.Embeddings.AppHost.Extensions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "<Pending>")]
internal static class ResourceBuilderExtensions
{
    extension(IResourceBuilder<ParameterResource> parameter)
    {
        internal string? GetValue() => parameter.Resource
            .GetValueAsync(default)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        internal int? GetInt32Value()
        {
            var value = parameter.GetValue();
            return int.TryParse(value, out var result)
                ? result
                : default(int?);
        }
    }

    extension(IResourceBuilder<OllamaResource> builder)
    {        
        internal IResourceBuilder<OllamaResource> WithGPUSupportIfVendorParameterProvided(
            IResourceBuilder<ParameterResource> vendorParameter)
        {
            var vendorValue = vendorParameter.GetValue();
            var vendor = vendorValue is not null && Enum.TryParse<OllamaGpuVendor>(vendorValue, out var gpuVendor)
                ? gpuVendor
                : default(OllamaGpuVendor?);

            return vendor is not null
                ? builder.WithGPUSupport(vendor.Value)
                : builder;
        }
    }

    extension(IResourceBuilder<SqlServerServerResource> builder)
    {
        internal IResourceBuilder<SqlServerServerResource> WithEndpointIfPortParameterProvided(
            string name,
            IResourceBuilder<ParameterResource>? portParameter)
        {
            var port = portParameter?.GetInt32Value();
            return port is not null
                ? builder.WithEndpoint(targetPort: port, name: name)
                : builder;
        }

        internal IResourceBuilder<SqlServerServerResource> WithPasswordIfProvided(
            IResourceBuilder<ParameterResource>? password) =>
                password?.GetValue() is not null
                    ? builder.WithPassword(password)
                    : builder;
    }
}
