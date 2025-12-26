// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "This is extending the target namespace and is not an error", Scope = "namespace", Target = "~N:Microsoft.Extensions.Hosting")]
[assembly: SuppressMessage("Minor Code Smell", "S125:Remove this commented out code", Justification = "Allowing comments in this class", Scope = "type", Target = "~T:Microsoft.Extensions.Hosting.Extensions")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Allowed in this project for now", Scope = "member", Target = "~M:Microsoft.Extensions.Hosting.Extensions.MapDefaultEndpoints(Microsoft.AspNetCore.Builder.WebApplication)~Microsoft.AspNetCore.Builder.WebApplication")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Allowed in this project for now", Scope = "member", Target = "~M:Microsoft.Extensions.Hosting.Extensions.ConfigureOpenTelemetry``1(``0)~``0")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "This is extending the target namespace and is not an error", Scope = "namespace", Target = "~N:Microsoft.Extensions.Hosting")]
[assembly: SuppressMessage("Style", "CA1724: The type name Extensions conflicts in whole or in part with the namespace name 'Microsoft.AspNetCore.Builder.Extensions'", Justification = "This is extending the target namespace and is not an error", Scope = "type", Target = "~T:Microsoft.Extensions.Hosting.Extensions")]
