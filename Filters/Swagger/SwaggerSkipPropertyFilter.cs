using Ihelpers.DataAnotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Core.Filters.Swagger
{
    /// <summary>
    /// This class is used to filter properties from the Swagger documentation.
    /// </summary>
    public class SwaggerSkipPropertyFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies the filter to the Swagger schema by removing properties with the `SwaggerIgnoreAttribute`.
        /// </summary>
        /// <param name="schema">The Swagger schema to filter.</param>
        /// <param name="context">The context of the filter operation.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Return if the schema or its properties are null
            if (schema?.Properties == null)
            {
                return;
            }

            // Get the properties with the `SwaggerIgnoreAttribute`
            var skipProperties = context.Type.GetProperties().Where(t => t.GetCustomAttribute<SwaggerIgnoreAttribute>() != null);

            // Remove the properties from the schema
            foreach (var skipProperty in skipProperties)
            {
                var propertyToSkip = schema.Properties.Keys.SingleOrDefault(x => string.Equals(x, skipProperty.Name, StringComparison.OrdinalIgnoreCase));

                if (propertyToSkip != null)
                {
                    schema.Properties.Remove(propertyToSkip);
                }
            }
        }
    }
}
