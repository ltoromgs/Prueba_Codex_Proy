// RusticaPortal_PRMVAN.Api/Swagger/SwaggerIgnoreFilter.cs
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RusticaPortal_PRMVAN.Api.Swagger
{
    public class SwaggerIgnoreFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null) return;

            var excluded = context.Type.GetProperties()
                .Where(p => p.GetCustomAttribute<SwaggerIgnoreAttribute>() != null);

            foreach (var prop in excluded)
            {
                // nombre real en JSON (Newtonsoft)
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var name = jsonAttr?.PropertyName ?? prop.Name;

                if (schema.Properties.ContainsKey(name))
                {
                    schema.Properties.Remove(name);
                }
            }
        }
    }
}
