using Ihelpers.DataAnotations;
using Ihelpers.Helpers;
using Newtonsoft.Json.Linq;
namespace Core
{
    /// <summary>
    /// Base class for the request body in all CRUDS and exports.
    /// </summary>
    public class BodyRequestBase
    {
        /// <summary>
        /// The attributes property stores the model for creating or updating an entity.
        /// </summary>
        public object? attributes { get; set; }

        /// <summary>
        /// The export parameters for the request.
        /// </summary>
        [SwaggerIgnore]
        public JObject? exportParams { get; set; }


        /// <summary>
        /// The confirmed parameter for the request.
        /// </summary>
        [SwaggerIgnore]
        public bool? confirmed { get; set; }

        /// <summary>
        /// The BulkAction for the request.
        /// </summary>
        [SwaggerIgnore]
        public object? bulkAction { get; set; }


        /// <summary>
        /// The filter for the request.
        /// </summary>
        [SwaggerIgnore]
        public object? filter { get; set; }

        /// <summary>
        /// A string representation of the attributes for the request.
        /// </summary>
        [SwaggerIgnore]
        public string? _attributes { get; set; }

        /// <summary>
        /// A string representation of the export parameters for the request.
        /// </summary>
        [SwaggerIgnore]
        public string? _exportParams { get; set; }

        /// <summary>
        /// A string representation of the filter for the request.
        /// </summary>
        [SwaggerIgnore]
        public string? _filter { get; set; }

        /// <summary>
        /// Creates an instance of BodyRequestBase.
        /// </summary>
        public BodyRequestBase()
        {
        }
        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="_attributes"></param>
        /// <param name="filter"></param>
        /// <param name="exportParams"></param>
        public BodyRequestBase(object? _attributes = null, object? filter = null, string? exportParams = null)
        {
            this.attributes = _attributes;
            this.filter = filter;
            this._exportParams = exportParams;
        }
        /// <summary>
        /// Parses the attributes, export parameters, and filter properties of the request.
        /// </summary>
        /// <returns>An empty string.</returns>
        public async Task<string> Parse()
        {
            this._attributes = attributes?.ToString().Trim();
            this._filter = filter?.ToString().Trim();
            this.exportParams = exportParams != null ? JObjectHelper.ParseOrNull(exportParams.ToString()) : exportParams;
            return "";
        }



        public string GetFilter(string wichFilter)
        {
            if (!string.IsNullOrEmpty(this.filter?.ToString()))
            {
                JObject jObjFilter = JObject.Parse(this.filter?.ToString());

                return jObjFilter[wichFilter]?.ToString();
            }

            return "";
        }
    }
}
