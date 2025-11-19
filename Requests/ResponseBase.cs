using Ihelpers.Helpers;

namespace Core
{
    /// <summary>
    /// A base class to provide a common response format for API calls, is meatn to be used only for TEntity controllers .
    /// </summary>
    public class ResponseBase
    {
        /// <summary>
        /// Creates a dictionary with a "data" and/or "meta" key, depending on the input parameters.
        /// </summary>
        /// <param name="_data">The data to include in the response dictionary.</param>
        /// <param name="_meta">The meta information to include in the response dictionary.</param>
        /// <returns>A dictionary with the "data" and/or "meta" keys.</returns>
        public static async Task<Dictionary<string, dynamic?>> Response(object? _data = null, object? _meta = null)
        {
            Dictionary<string, dynamic?> response = new Dictionary<string, object?>();
            if (_data != null)
            {
                response.Add("data", _data);
            }
            if (_meta != null)
            {
                response.Add("meta", _meta);
            }
            return response;
        }

        /// <summary>
        /// Gets the "meta" property of a dynamic object. If the "meta" property doesn't exist, it returns a default meta object.
        /// </summary>
        /// <param name="wichObject">The dynamic object to extract the "meta" property from.</param>
        /// <returns>The "meta" property or a default meta object if the property doesn't exist.</returns>
        public static async Task<dynamic?> GetMeta(dynamic? wichObject)
        {
            try
            {
                // Attempt to access the "meta" property of the dynamic object.
                dynamic meta = wichObject.GetType().GetProperty("meta").GetValue(wichObject, null);

                return meta;
            }
            catch
            {
                // If the "meta" property doesn't exist or an exception occurs, return a default meta object.
                return JObjectHelper.GetDefaultMeta();
            }
        }
    }
}