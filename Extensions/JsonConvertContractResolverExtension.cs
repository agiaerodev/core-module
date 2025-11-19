using ChoETL;
using Newtonsoft.Json.Serialization;

namespace Core.Extensions.JsonConvertContractResolverExtensions
{

    /// <summary>
    /// This class is a contract resolver that converts all JSON keys to camel case format.
    /// </summary>
    public class GlobalCamelCaseContractResolver : CamelCasePropertyNamesContractResolver
    {
        /// <summary>
        /// Resolves the property name by converting it to camel case format if it contains an underscore character.
        /// </summary>
        /// <param name="propertyName">The property name to resolve.</param>
        /// <returns>The resolved property name in camel case format.</returns>
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.Contains('_') ? propertyName.ToCamelCase() : base.ResolvePropertyName(propertyName);
        }
    }

    /// <summary>
    /// This class is a naming strategy that converts all JSON keys to camel case format.
    /// </summary>
    public class CamelCaseNamingStrategySet : CamelCaseNamingStrategy
    {
        /// <summary>
        /// Resolves the property name by converting it to camel case format if it contains an underscore character.
        /// </summary>
        /// <param name="name">The property name to resolve.</param>
        /// <returns>The resolved property name in camel case format.</returns>
        protected override string ResolvePropertyName(string name)
        {
            return name.Contains('_') ? name.ToCamelCase() : base.ResolvePropertyName(name);
        }
    }
}
