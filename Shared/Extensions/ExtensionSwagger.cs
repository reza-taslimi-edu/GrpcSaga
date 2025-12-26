using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Extensions
{
    public static class ExtensionSwagger
    {
        public static void SwaggerGenDoc(this SwaggerGenOptions swaggerGenOptions,string title,string version,string description)
        {
            swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = description
            });

            // Include XML comments if available
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                swaggerGenOptions.IncludeXmlComments(xmlPath);
            }

            // Ignore circular references
            swaggerGenOptions.CustomSchemaIds(x => x.FullName);

            // فیلتر کردن endpointهای gRPC بر اساس نام
            swaggerGenOptions.DocInclusionPredicate((docName, apiDesc) =>
            {
                // حذف endpointهایی که مربوط به gRPC هستند
                if (apiDesc.ActionDescriptor.DisplayName?.Contains("Grpc", StringComparison.OrdinalIgnoreCase) == true ||
                    apiDesc.ActionDescriptor.DisplayName?.Contains("gRPC", StringComparison.OrdinalIgnoreCase) == true)
                    return false;

                // حذف endpointهایی که مربوط به proto هستند
                if (apiDesc.RelativePath?.Contains("proto", StringComparison.OrdinalIgnoreCase) == true)
                    return false;

                return true;
            });
        }
    }
}
