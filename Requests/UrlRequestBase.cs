using Core.Events;
using Core.Events.Interfaces;
using Core.Exceptions;
using Ihelpers.Helpers;
using Core.Interfaces;
using Core.Logger;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Runtime.CompilerServices;
using Ihelpers.DataAnotations;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Core.Helpers;
using Ihelpers.Extensions;

namespace Core
{
    /// <summary>
    /// UrlRequestBase is a standard URL request class for all requests.
    /// </summary>
    public class UrlRequestBase
    {
        /// <summary>
        /// An internal property to store the settings as a JToken object.
        /// </summary>
        private JToken? internalSettings { get; set; }

        /// <summary>
        /// An internal property to store the filters as a JToken object.
        /// </summary>
        private JToken? internalFilter { get; set; }

        /// <summary>
        /// A property to store the setting sent in API request.
        /// </summary>
        public string? setting { get; set; }

        /// <summary>
        /// A property to store the filter sent in API request..
        /// </summary>
        public string? filter { get; set; }

        /// <summary>
        /// A property to store the include sent in API request..
        /// </summary>
        public string? include { get; set; }

        /// <summary>
        /// A property to store the criteria sent in API request..
        /// </summary>
        public string? criteria { get; set; }

        /// <summary>
        /// A property to store the export parameters sent in API request.
        /// </summary>
        public string? exportParams { get; set; } = "";

        /// <summary>
        /// A property to store the filename, when exporting or reporting.
        /// </summary>
        [BindNever]
        public string? filename { get; set; } = "";

        /// <summary>
        /// A property to store the procedure name, for report purposes.
        /// </summary>
        [BindNever]
        public string? procedureName { get; set; } = "";

        /// <summary>
        /// A property to store the ordering field, used inside reports module.
        /// </summary>
        [BindNever]
        public string? orderingField { get; set; } = "position";

        /// <summary>
        /// An internal property to store the export parameters as a JObject object.
        /// </summary>
        [BindNever]
        public JObject? internalExportParams { get; private set; }

        /// <summary>
        /// A property to store the current context user.
        /// </summary>
        [BindNever]
        public dynamic? currentContextUser { get; set; } = null;

        /// <summary>
        /// A property to store the current context token.
        /// </summary>
        [BindNever]
        public string? currentContextToken { get; set; } = "";

        /// <summary>
        /// A property to store the current action.
        /// </summary>
        [BindNever]
        public string? currentAction { get; set; } = "";
        /// <summary>
        /// A property to store the page number.
        /// </summary>
        public int? page { get; set; }

        /// <summary>
        /// A property to store the number of items to take.
        /// </summary>
        public int? take { get; set; }

        /// <summary>
        /// A property to indicate if the parent should be included.
        /// </summary>
        public bool includeParent { get; set; } = false;

        /// <summary>
        /// A property to indicate if the default includes must be considered on GetIncludes function.
        /// </summary>
        [BindNever]
        public bool selectDefaultIncludes { get; set; } = true;

        /// <summary>
        /// A property to store the current request permission, it gets constructed inside Parse method.
        /// </summary>
        [BindNever]
        public string? permission { get; private set; } = null;

        /// <summary>
        /// A property to store the current request actionMessage with a custom value, if set the action will be logged with the custom value.
        /// </summary>
        [BindNever]
        public string? actionMessage { get; private set; } = null;



        /// <summary>
        /// A property to indicate if the default pagination must be considered on Parse Method, can be modificated in the PaginationMiddleware, when am endpoint is present on whitelist is set to false.
        /// </summary>
        [BindNever]
        public bool useDefaultPagination { get; set; } = true;

        /// <summary>
        /// A property to store the current request context.
        /// </summary>
        private HttpContext? currentContext;

        /// <summary>
        /// A property that indicates if permissions are going to be evaluated inside Parse method.
        /// </summary>
        private bool checkPermissions = true;


        /// <summary>
        /// A property that indicates if Query to remember or not
        /// </summary>
        internal bool rememberQuery = true;

        
        /// <summary>
        /// A property that indicates if the user timezone should be used.
        /// </summary>
        private bool useUserTimezone = true;




        #region Setters
        /// <summary>
        /// Gets or sets the value of `internalSettings`. If a value is passed as an argument, it will set the value of `internalSettings` to that value.
        /// </summary>
        /// <param name="_internalSettings">The value to set `internalSettings` to, if any.</param>
        /// <returns>The value of `internalSettings`.</returns>
        public JToken? InternalSettings(JToken? _internalSettings = null)
        {
            internalSettings = _internalSettings ?? internalSettings;
            return internalSettings;
        }

        /// <summary>
        /// Gets or sets the value of `internalFilter`. If a value is passed as an argument, it will set the value of `internalFilter` to that value.
        /// </summary>
        /// <param name="_internalFilter">The value to set `internalFilter` to, if any.</param>
        /// <returns>The value of `internalFilter`.</returns>
        public JToken? InternalFilter(JToken? _internalFilter = null)
        {
            internalFilter = _internalFilter ?? internalFilter;
            return internalFilter;
        }

        /// <summary>
        /// Gets or sets the value of `internalExportParams`. If a value is passed as an argument, it will set the value of `internalExportParams` to that value.
        /// </summary>
        /// <param name="_internalExportParams">The value to set `internalExportParams` to, if any.</param>
        /// <returns>The value of `internalExportParams`.</returns>
        public JObject? InternalExportParams(JObject? _internalExportParams = null)
        {
            internalExportParams = _internalExportParams ?? internalExportParams;
            return internalExportParams;
        }

        /// <summary>
        /// Gets or sets the value of `currentContextUser`. If a value is passed as an argument, it will set the value of `currentContextUser` to that value.
        /// </summary>
        /// <param name="wichUser">The value to set `currentContextUser` to, if any.</param>
        /// <returns>The value of `currentContextUser`.</returns>
        public dynamic? setCurrentContextUser(dynamic? wichUser)
        {
            currentContextUser = wichUser ?? currentContextUser;
            return currentContextUser;
        }




        /// <summary>
        /// Gets the current context.
        /// </summary>
        /// <returns>The current HttpContext</returns>
        public HttpContext? getCurrentContext() => currentContext;

        /// <summary>
        /// Gets the current context token.
        /// </summary>
        /// <returns>The current context token as a string</returns>
        public string? getCurrentContextToken() => currentContextToken;

        /// <summary>
        /// Gets the current context user.
        /// </summary>
        /// <returns>The current context user as an object</returns>
        public dynamic? getCurrentContextUser() => currentContextUser;

        /// <summary>
        /// Sets the action message.
        /// </summary>
        /// <param name="wichMesssage">The message to be set</param>
        public void setActionMessage(string? wichMesssage) => actionMessage = wichMesssage ?? actionMessage;

        /// <summary>
        /// Indicates that permission check should not be performed.
        /// </summary>
        public void doNotCheckPermissions() => checkPermissions = false;

        /// <summary>

        /// Indicates if the query should be remembered.
        /// </summary>
        public void doNotRememberQuery() => rememberQuery = false;


        /// Indicates that user timezone should be ignored or not.
        /// </summary>
        public void doNotUseUserTimezone() => useUserTimezone = false;

        /// <summary>
        /// Indicates that default take should be used or ignored.
        /// </summary>
        public void doNotUseDefaultPagination() => useDefaultPagination = false;


        /// <summary>
        /// Indicates default includes for entity should be ignored or not..
        /// </summary>
        public void doNotUseDefaultIncludes() => selectDefaultIncludes = false;
        #endregion
        /// <summary>
        /// Default constructor
        /// </summary>
        public UrlRequestBase()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlRequestBase"/> class.
        /// </summary>
        /// <param name="_include">The include string to specify which entities to include in the response.</param>
        /// <param name="_filter">The filter string to specify conditions for filtering the data.</param>
        /// <param name="_criteria">The criteria string to specify conditions for retrieving the data.</param>
        /// <param name="_setting">The setting string to specify custom settings for the response.</param>
        /// <param name="_page">The page number for pagination.</param>
        /// <param name="_take">The number of items to retrieve per page for pagination.</param>
        /// <param name="wichController">The instance of the controller from which this class is being instantiated.</param>
        public UrlRequestBase(string? _include = null, string? _filter = null, string? _criteria = null, string? _setting = null, int? _page = null, int? _take = null, dynamic wichController = null)
        {
            include = _include;
            filter = _filter;
            criteria = _criteria;
            setting = _setting;
            page = _page;
            take = _take;

            // If the `wichController` parameter is not null, retrieve the current token and user information from the controller's HTTP context
            if (wichController != null)
            {
                currentContext = wichController.HttpContext;
                currentContextToken = currentContext.Request.Headers.Authorization;
                try { currentContextUser = wichController._authUser; } catch { }
            }

            // If the `include` string contains the word "parent", set the `includeParent` flag to true and remove "parent" from the `include` string
            if (include != null && include.Contains("parent"))
            {
                include = include.Replace("parent.", string.Empty);
                include = include.Replace("parent", string.Empty);
                includeParent = true;
            }

            // If the `currentContextToken` is not null, remove the "Bearer " string from it
            if (currentContextToken != null)
            {
                currentContextToken = currentContextToken.Replace("Bearer ", string.Empty);
            }

            // Replace commas in the `include` string with dots
            if (include != null)
            {
                include = include.Replace(',', '.');
            }

            // Parse the `setting` string into a JToken object using the `JObjectHelper.JTokenParseOrNull` method
            this.internalSettings = JObjectHelper.JTokenParseOrNull(setting);

            // Parse the `filter` string into a JToken object using the `JObjectHelper.JTokenParseOrNull` method
            this.internalFilter = JObjectHelper.JTokenParseOrNull(filter);
        }

        /// <summary>
        /// This method initializes the UrlRequest base class properties, verifies permissions 
        /// </summary>
        /// <param name="wichController">The dynamic controller object from which to extract information such as the HttpContext, RouteData, and the user's authorization controller must inherit from ControllerBase[TEntity] <see cref="Core.Controllers.ControllerBase{TEntity}"/></param>
        /// <returns>An empty string</returns>
        public async Task<string> Parse(dynamic wichController = null)
        {
            // If the wichController parameter is not null, assign the currentContext to wichController's HttpContext
            if (wichController != null)
            {
                currentContext = wichController.HttpContext;

                // Get the authorization header from the request
                currentContextToken = currentContext.Request.Headers.Authorization;

                // Try to get the authenticated user from the controller
                try
                {
                    currentContextUser = wichController._authUser ?? AuthHelper.AuthUser(currentContextToken);
                    currentContextUser.Initialize();
                    currentContextUser.password = string.Empty;
                }
                catch
                {

                }

                // Try to get the action from the route data
                try
                {
                    currentAction = wichController.ControllerContext.RouteData.Values["action"].ToString().ToLower();
                }
                catch { }

                // Check if the user has access to the action, if so, assign the permission
                if (Ihelpers.Extensions.ConfigContainer.CheckPermissionAccessInsideControllerBase && currentContextUser != null && checkPermissions)
                {
                    // Split the URL into an array of segments
                    string[] route = wichController.Request.Path.Value.Split('/');

                    // Get the name of the controller and action
                    string controllerActionName = wichController.ControllerContext.RouteData.Values["action"].ToString().ToLower();
                    string controllerName = wichController.ControllerContext.ActionDescriptor.ControllerName.ToString().ToLower();

                    // Get the permission options
                    List<PermissionBaseOption>? options = Ihelpers.Extensions.ConfigContainer.permissionBaseOptions;

                    // Get the options for the current controller
                    PermissionBaseOption? opt = options != null ? options.Where(opt => opt.controller == controllerName).FirstOrDefault() : null;

                    // Check if the controller should be ignored for permission checking
                    bool skipThisController = opt != null ? opt.ignore : false;

                    if (!skipThisController)
                    {
                        // Assign the permission based on the options or the URL segments
                        permission = ((options != null && opt != null && opt.permissionBase != null) ? opt.permissionBase : $"{route[2]}.{route[4]}").ToLower();

                        // Assign the user action name
                        string userActionName = string.Empty;
                        switch (controllerActionName)
                        {
                            case "index":
                                permission += ".index";
                                userActionName = "list";
                                break;
                            case "show":
                                permission += ".index";
                                userActionName = "see";
                                break;
                            case "create":
                                permission += ".create";
                                userActionName = "create";
                                break;
                            case "update":
                                permission += ".edit";
                                userActionName = "edit";
                                break;
                            case "delete":
                                permission += ".destroy";
                                userActionName = "delete";
                                break;
                            case "restore":
                                permission += ".restore";
                                userActionName = "restore";
                                break;
                            case "export":
                                permission += ".export";
                                userActionName = "export";
                                break;
                        }

                        // If the user does not have the permission, log the message and throw an exception
                        if (!currentContextUser.HasAccess(permission))
                        {
                            string userEmail = (currentContextUser != null) ? (string)currentContextUser.email : "Anonymous";
                            string Message = $"{userEmail} has tried to {userActionName}: {route[4]} without proper permissions";
                            CoreLogger.LogMessage(Message, logType: LogType.Error, userId: currentContextUser.id);
                            throw new ExceptionBase(Message, 403);

                        }


                    }
                }
            }

            // If the `include` string contains the word "parent", set the `includeParent` flag to true and remove "parent" from the `include` string
            if (include != null && include.Contains("parent"))
            {
                include = include.Replace("parent.", string.Empty);
                include = include.Replace("parent", string.Empty);
                includeParent = true;
            }

            //if the 'currentContextToken' string is not null them remove "Bearer " from it and leave only the token.
            currentContextToken = string.IsNullOrEmpty(currentContextToken) ? currentContextToken : currentContextToken.Replace("Bearer ", string.Empty);

            try
            {
            //Parsing of internalSettings based on the setting comming from frontend
            this.internalSettings = JObjectHelper.ParseOrNull(setting);

            }
            catch
            {
                throw new ExceptionBase("Invalid JSON settings. Please ensure the settings are in a valid JSON format.", 400);

            }


            try
            {
                //Parsing of internalFilter based on the filter comming from frontend
                this.internalFilter = JObjectHelper.ParseOrNull(filter);
            }
            catch 
            {
                throw new ExceptionBase("Invalid JSON filters. Please ensure the filters are in a valid JSON format.", 400);
            }

            try
            {
                //Parsing of internalExportParams based on the exportParams comming from frontend
                this.internalExportParams = JObjectHelper.ParseOrNull(exportParams);

            }
            catch 
            {

                throw new ExceptionBase("Invalid JSON exportParams. Please ensure the exportParams are in a valid JSON format.", 400);
            }

            //if the timezone setting is set them log the user timezone
            if (!string.IsNullOrEmpty(GetSetting("timezone", out string? timezone)))
            {

                long? userId = currentContextUser?.id;

                CoreLogger.LogMessage($"track of user timezone for {currentContextUser?.email}  with id: {currentContextUser?.id}     timezone:{timezone}    offset:{getRequestTimezoneOffset()} ", "", LogType.Information, userId);
            }


            if ((!string.IsNullOrEmpty(exportParams) || !useDefaultPagination) && (this.page == null && this.take == null))
            {
                this.page = null;
                this.take = null;
            }
            else
            {

                if (useDefaultPagination)
                {
                    //Get default values from Page and Take
                    int defaultPage = ConfigurationHelper.GetConfig<int>("DefaultConfigs:Page");
                    int defaultTake = ConfigurationHelper.GetConfig<int>("DefaultConfigs:Take");
                    int maxTake = ConfigurationHelper.GetConfig<int>("DefaultConfigs:MaxTake");

                    //Query Params
                    //page = pageIndex
                    //take = pageSize


                    //Jira Ticket https://agione.atlassian.net/browse/AGIONE-255

                    this.page ??= defaultPage;
                    this.take ??= defaultTake;

                    if (this.take > maxTake) this.take = maxTake;
                }
            }

            return "";
        }

        //To be used inside if or linq statements
        //example: if(GetSetting("value", out string getSettingResult) == "") {getSettingResult = "notSet";]
        public string? GetSetting(string filter, out string? fieldOut)
        {
            //Try get the search filter
            string? field = null;

            if (setting != null)
            {
                field = (internalSettings?.SelectToken(filter))?.ToString();


            }
            fieldOut = field;
            return field;
        }
        /// <summary>
        /// Get setting from filter withput OUT variable
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string? GetSetting(string filter)
        {
            //Try get the search filter
            string field = null;

            if (setting != null)
            {
                field = (internalSettings.SelectToken(filter))?.ToString();


            }
            return field;
        }

        /// <summary>
        /// Sets or creates a value to the request filters, after parsed of before beign parsed.
        /// </summary>
        /// <param name="settingName">Filter name</param>
        /// <returns>true or false if succesfull or not</returns>
        public bool? SetSetting(string settingName, object? newValue)
        {
            //this request has noon been parsed
            if (newValue == null || string.IsNullOrEmpty(settingName))
            {
                return false;
            }

            //Checks if this request has been parsed in controller 

            bool parsed = this.internalSettings != null;

            if (parsed)
            {
                 internalSettings[settingName] = JToken.FromObject(newValue);
                 return true;
            }
            else
            {
                JObject settings = string.IsNullOrEmpty(this.setting) ? new JObject() : JObject.Parse(this.setting);
                settings[settingName] = JToken.FromObject(newValue);
                this.setting = settings.ToString();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets a filter setting with the specified name and value.
        /// </summary>
        /// <param name="settingName">The name of the filter setting to set.</param>
        /// <param name="newValue">The new value for the filter setting. Can be a string (potentially a JSON string) or any other object.</param>
        /// <returns>
        /// Returns true if the filter was successfully set; false if the input parameters are invalid (null or empty setting name).
        /// </returns>
        public bool? SetFilter(string settingName, object? newValue)
        {
            if (newValue == null || string.IsNullOrEmpty(settingName))
            {
                return false;
            }

            bool parsed = this.internalFilter != null;
            JToken tokenValue;

            if (newValue is string stringValue && stringValue.StartsWith("{"))
            {
                try
                {
                    tokenValue = JToken.Parse(stringValue);
                }
                catch (JsonReaderException)
                {
                    tokenValue = JToken.FromObject(newValue);
                }
            }
            else
            {
                tokenValue = JToken.FromObject(newValue);
            }

            if (parsed)
            {
                internalFilter[settingName] = tokenValue;
                filter = JsonConvert.SerializeObject(internalFilter);
            }
            else
            {
                JObject settings = string.IsNullOrEmpty(this.filter) ? new JObject() : JObject.Parse(this.filter);
                settings[settingName] = tokenValue;
                this.filter = settings.ToString();
            }

            return true;
        }

        /// <summary>
        /// This method retrieves the filter from the internal filter object, given a filter string as an argument. 
        /// </summary>
        /// <param name="filter">A string representing the filter to retrieve from the internal filter object</param>
        /// <returns>The value of the filter as a string, or the string "id" if the filter is not found in the internal filter object</returns>
        public string GetFilter(string filter)
        {
            // Try to get the search filter.
            string? field = "id";
            if (internalFilter != null)
            {
                field = (internalFilter.SelectToken(filter))?.ToString();

                // If the field is null and the filter is "field", return "id".
                field = (field == null && filter == "field") ? "id" : field;
            }
            else
            {
                if (!string.IsNullOrEmpty(this.filter))
                {

                    JObject filterObj = JObject.Parse(this.filter);

                    field = filterObj[filter]?.ToString();
                }
            }


            return field;
        }

        /// <summary>
        /// This method retrieves the filter from the internal filter object, given a filter string as an argument. 
        /// </summary>
        /// <param name="filter">A string representing the filter to retrieve from the internal filter object</param>
        /// <returns>The value of the filter as a string, or null if the filter is not found in the internal filter object</returns>
        public string? GetCustomFilter(string filter)
        {
            //Try get the search filter
            string? field = null;
            if (internalFilter != null)
            {
                field = (internalFilter.SelectToken(filter))?.ToString();

                field = (field == null && filter == "field") ? "id" : field;
            }

            return field;
        }



        /// <summary>
        /// Gets the related entities to be included in the query based on the model and the specified includes, used inside repositories
        /// </summary>
        /// <typeparam name="T">The type of the entities to be included in the query</typeparam>
        /// <param name="query">The query to add the includes to</param>
        /// <param name="model">The model used to determine the default includes</param>
        /// <returns>A query with the specified and default includes</returns>
        public IQueryable<T> GetIncludes<T>(IQueryable<T> query, dynamic model = null) where T : class
        {
            // Get a list of all the relation properties defined in the model
            List<string> allRelations = model.getRelations();

            // Store included relations for evaluation of deleted_at and deleted_by in nested relations
            List<string> queryRelations = new List<string>();

            // An array to store the names of the includes to be added
            string[] splitInclude = Array.Empty<string>();

            // If the `include` property is not null or empty, split it into an array
            if (!string.IsNullOrEmpty(include))
            {
                splitInclude = include.Split(',');
            }

            // If the model is not null, check if it has a `default_include` property
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.default_include) && selectDefaultIncludes)
                {
                    // Get the value of the `withoutDefaultInclude` filter
                    var withoutDefaultInclude = this.GetFilter("withoutDefaultInclude");

                    // If `withoutDefaultInclude` is null or "id" or "false", add the default includes to the query
                    if (withoutDefaultInclude == null || withoutDefaultInclude == "id" || (Convert.ToBoolean(withoutDefaultInclude) == false))
                    {
                        // Split the `default_include` property into an array
                        string[] defaultInclude = model.default_include.Split(',');

                        // Add the default includes to the list of includes
                        splitInclude = splitInclude.Union(defaultInclude).ToArray();
                    }

                }
            }

            // Add each include to the query
            foreach (string fieldToInclude in splitInclude)
            {
                query = query.Include(fieldToInclude);

            }



            // Return the query with the includes added
            return query;
        }

        /// <summary>
        /// Determines if the attributes passed as a parameter are valid by checking if it contains any of the properties listed in the `properties` list.
        /// </summary>
        /// <param name="attrs">Array of dynamic attributes to check for validity.</param>
        /// <returns>True if the attributes are valid, false otherwise.</returns>
        private bool HasValidAttributes(dynamic[] attrs)
        {
            // List of properties to check against the attributes passed as a parameter.
            List<string> properties = new List<string> { "NotMapped", "ObjectToString", "RelationalField", "SimpleObjectToString" };

            // If the first attribute that matches any of the properties in the `properties` list is found, return false.
            if (attrs.Where(a => properties.Contains(a.TypeId.Name)).FirstOrDefault() != null)
                return false;

            // Otherwise, return true if no matching properties were found.
            return true;
        }

        /// <summary>
        /// Searchs inside search filter sent in request if present, and applies the search filter to the current IQueryable[T] query
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <param name="query">The entity query</param>
        /// <param name="model">The entity model</param>
        /// <returns>The entity query with search filters applied.</returns>
        public IQueryable<T> SetDynamicFilters<T>(IQueryable<T> query, dynamic model = null) where T : class
        {

            //find the propertyName in the JToken in the requestBase
            string searchFilter = this.GetFilter("search");
            string[]? searchableFields = model.searchable_fields != null ? model.searchable_fields.Split(',') : null;
            string searchQuery = "";
            string objName = model.GetType().FullName;
            //if no filters were specified skip the whole process
            if (internalFilter != null)
            {

                //gets model properties
                List<PropertyInfo>? objectProperties = ConfigContainer.cache.GetValue<List<PropertyInfo>?>(model.GetType().FullName).Result;

                if (objectProperties == null)
                {
                    try
                    {
                        objectProperties = model.getProperties();
                    }
                    catch
                    {
                        objectProperties = model.GetType().GetProperties();
                    }

                    ConfigContainer.cache.CreateValue(model.GetType().FullName, objectProperties).GetAwaiter().GetResult();

                }

                //User timezone offset for queries
                var dateTimeIana = getRequestTimezone();


                //iterate over every object property
                foreach (System.Reflection.PropertyInfo? prop in objectProperties)
                {
                    //Get the Data Anotations that are valid and were not mark as Ignore into an array

                    object[]? attrs = ConfigContainer.cache.GetValue<object[]?>($"{objName}.{prop.Name}.DataAnnotations").Result;

                    if (attrs == null)
                    {
                        attrs = prop.GetCustomAttributes(true).Where(att => att != null && att.ToString().Contains("Nullable") == false && att is IDataAnnotationBase).ToArray();
                        ConfigContainer.cache.CreateValue($"{objName}.{prop.Name}.DataAnnotations", attrs, 999999999).GetAwaiter().GetResult(); ;
                    }
                    //transform to camelCase the model property if is not date 
                    string propertyName = prop.Name == "date" ? prop.Name : ConfigContainer.cache.GetValue<string>($"{objName}.{prop.Name}.CamelCase").Result;
                    if (propertyName == null)
                    {
                        propertyName = StringHelper.ToCamelCase(prop.Name);
                        ConfigContainer.cache.CreateValue($"{objName}.{prop.Name}.CamelCase", propertyName, 999999999).GetAwaiter().GetResult();
                    }
                    //if the property doesn't have data anotation to avoid the relationalField, notMapped, ObjectToString annd simpleObjectToString
                    if ((HasValidAttributes(attrs) && propertyName != "translations") && internalFilter != null)
                    {

                        //find the propertyName in the JToken in the requestBase
                        JToken? filterValueToken = (internalFilter.SelectToken(propertyName));
                        var propertyTypeName = prop.PropertyType.FullName;

                        //catching special filter by dateRange 
                        if ((propertyName == "date" || propertyTypeName.Contains("System.DateTime")) )
                        {
                            if(filterValueToken == null) filterValueToken = (internalFilter.SelectToken("date"));

                            if (filterValueToken != null) { 
                            
                                //check if the field exists in the model to avoid errors (blockInBlockOut custom requestField, and others)
                                string? requestField = filterValueToken.SelectToken("field")?.ToString();

                                if (requestField == prop.Name || requestField == propertyName)
                                {

                                    var property = objectProperties.Where(prop => prop.Name == requestField).FirstOrDefault();

                                    string? typeDateRange = filterValueToken.SelectToken("type")?.ToString() ?? null;
                                    string? dateRangeFromFront = filterValueToken.SelectToken("from")?.ToString() ?? null;
                                    string? dateRangeToFront = filterValueToken.SelectToken("to")?.ToString() ?? null;

                                    var (dateRangeFrom, dateRangeTo) = DateTimeExtensions.GetRangeOfTimeFromTo(typeDateRange, dateRangeFromFront, dateRangeToFront);

                                    if (!String.IsNullOrEmpty(typeDateRange)
                                        && !String.IsNullOrEmpty(dateRangeFrom)
                                        && !String.IsNullOrEmpty(dateRangeTo)
                                        && property != null)

                                    {

                                        // DateTime dtFrom = (DateTime.Parse(dateRangeFrom)).SetTimezoneOffset(dateTimeOffset, negate: true);
                                        DateTime dtFrom = DateTime.Parse(dateRangeFrom);

                                        //DateTime dtTo = (DateTime.Parse(dateRangeTo)).SetTimezoneOffset(dateTimeOffset, negate: true);
                                        DateTime dtTo = DateTime.Parse(dateRangeTo);

                                        //attrs = property.GetCustomAttributes(true).Where(att => att != null && att is not Ignore && att.ToString().Contains("Nullable") == false && att is IDataAnnotationBase).ToArray();

                                        // Check if the NoUserTimezone data annotation is present
                                        var noConvertToUserTimezone = attrs.Where(att => att is NoUserTimezone).FirstOrDefault();

                                        if (noConvertToUserTimezone == null)
                                        {

                                            // DateTime dtFrom = (DateTime.Parse(dateRangeFrom)).SetTimezoneOffset(dateTimeOffset, negate: true);
                                            dtFrom = dtFrom.FromDateToTimezone(dateTimeIana, negate: true);

                                            //DateTime dtTo = (DateTime.Parse(dateRangeTo)).SetTimezoneOffset(dateTimeOffset, negate: true);
                                            dtTo = dtTo.FromDateToTimezone(dateTimeIana, negate: true);

                                        }

                                        string value = @"{
                                        'operator': 'between',
                                        'field':'" + (property?.Name) + @"',
                                        'from':'" + $"{dtFrom.ToString("yyyy-MM-dd HH:mm")}" + @"',
                                        'to':'" + $"{dtTo.ToString("yyyy-MM-dd HH:mm")}" + @"'
                                    }";
                                        //Core.Logger.CoreLogger.LogMessage($"Date filter detected for entity {typeof(T).Name}", stackTrace: value);
                                        filterValueToken = JToken.Parse(value);

                                    }
                                    //if the dateRange necessary fields are not completely we need to avoid the filter
                                    else
                                    {
                                        filterValueToken = null;
                                    }

                                }
                                else {
                                    continue;
                                }
                               
                            }
                        }

                        //if is set the propertyName in the filters 
                        if (filterValueToken?.Type != JTokenType.Null && filterValueToken?.Type != null)
                        {
                            string filterOperator = (filterValueToken.SelectToken("operator"))?.ToString() ?? "=="; // Get filter operator
                            // Get the filter value either from the "value" property or the filterValueToken's Json ToString() result
                            string filterValue = (filterValueToken.SelectToken("value"))?.ToString() ?? filterValueToken.ToString();
                            object? filterValueArray = null;


                            // Check if the filterValue is a valid array
                            if (filterValue.IsValidArray())
                            {
                                // If the filterValue is a non-empty array, set the operator to "contains".
                                // If it's an empty array, set the operator to "none".
                                filterOperator = filterValue != "[]" ? "contains" : "none";

                                // Deserialize the filterValue into a list of the appropriate type based on the property type name
                                if (propertyTypeName.Contains("System.Int32"))
                                {
                                    var internalFilterValues = JsonConvert.DeserializeObject<List<int?>>(filterValue.ToString());
                                    filterValueArray = internalFilterValues;

                                }
                                else if (propertyTypeName.Contains("System.Int64"))
                                {
                                    var internalFilterValues = JsonConvert.DeserializeObject<List<long?>>(filterValue.ToString());
                                    filterValueArray = internalFilterValues;
                                }
                                else if (propertyTypeName.Contains("System.String"))
                                {
                                    var internalFilterValues = JsonConvert.DeserializeObject<List<string?>>(filterValue.ToString());
                                    filterValueArray = internalFilterValues;
                                }
                                else if (propertyTypeName.Contains("System.DateTime"))
                                {
                                    var internalFilterValues = JsonConvert.DeserializeObject<List<DateTime?>>(filterValue.ToString());
                                    filterValueArray = internalFilterValues;
                                }
                                else
                                {
                                    var internalFilterValues = JsonConvert.DeserializeObject<List<int?>>(filterValue.ToString());
                                    filterValueArray = internalFilterValues;
                                }

                            }

                            // Check the filter operator
                            switch (filterOperator)
                            {
                                case "none":
                                    // No filter is applied
                                    break;
                                case "whereNull":
                                    query = query.Where($"obj => obj.{prop.Name} == null");
                                    // No filter is applied
                                    break;
                                case "contains":

                                    if (filterValueArray != null)
                                    {

                                        // If filterValueArray is not null, use it to filter the query using the "Contains" method
                                        query = query.Where($"obj => @0.Contains(obj.{prop.Name})", filterValueArray);
                                    }
                                    else
                                    {
                                        // If filterValueArray is null, use filterValue to filter the query using the "Contains" method
                                        query = query.Where($"obj => obj.{prop.Name}.Contains(@0)", filterValue);

                                    }
                                    break;
                                case "notContains":
                                    //adding the dynamic filter with not Contains
                                    query = query.Where($"obj => !obj.{prop.Name}.Contains(@0)", filterValue);
                                    break;

                                case "between":
                                    //Get field to filter
                                    string field = (filterValueToken.SelectToken("field"))?.ToString() ?? prop.Name;

                                    if (string.IsNullOrEmpty(field)) field = prop.Name;

                                    string? from = filterValueToken.SelectToken("from")?.ToString();
                                    string? to = filterValueToken.SelectToken("to")?.ToString();

                                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                                    {
                                        //adding the dynamic filter between
                                        query = query.Where($"obj => obj.{field}>=@0 && obj.{field}<=@1", from, to);
                                    }

                                    break;

                                default:
                                    //adding the dynamic filter with the == by default
                                    query = query.Where($"obj => obj.{prop.Name} {filterOperator} @0", filterValue == "null" ? null : filterValue);
                                    break;
                            }
                        }

                        // Check the searchFilter operator
                        if (!string.IsNullOrEmpty(searchFilter))
                        {
                            // Check if the current property name is present in the list of searchable fields
                            if (searchableFields.Contains(prop.Name))
                            {
                                // Initialize the subquery string
                                string subQuery = "";

                                // Get the name of the property type
                                string type = prop.PropertyType.Name;

                                // Check if the property type is a string
                                if (type == "String")
                                {
                                    // Add a condition to the subquery that checks if the property contains the search filter
                                    subQuery += $"obj.{prop.Name}.Contains(\"{searchFilter}\")";
                                }
                                // Check if the property type is Int64
                                if (prop.PropertyType.FullName.Contains("System.Int64"))
                                {
                                    // Try to parse the search filter as an integer
                                    int intSearch = 0;
                                    bool intParsed = Int32.TryParse(searchFilter, out intSearch);

                                    // If the parsing is successful, add a condition to the subquery that checks if the property is equal to the integer representation of the search filter
                                    if (intParsed) subQuery += $"obj.{prop.Name} == {intSearch}";

                                }

                                // Check if the search query is empty, and if so, prepend the subquery with the expression syntax "obj =>" for dynamic LINQ statment
                                if (searchQuery == "" && subQuery != "") subQuery = "obj => " + subQuery;
                                // If the search query is not empty, add the subquery to it using the "||" operator
                                if (searchQuery != "" && subQuery != "") subQuery = " || " + subQuery;

                                // Add the subquery to the main search query
                                searchQuery += subQuery;
                            }


                        }

                    }
                }
                //Apply the searchQuery if not null to the subQuery
                if (searchQuery != "")
                {
                    query = query.Where(searchQuery);
                }

                //Filter the order way and field based in the order into the filter data
                if (GetFilter("order") != null)
                {

                    //find the propertyName in the JToken in the requestBase
                    JToken? orderValueToken = (internalFilter.SelectToken("order"));
                    // The "field" is assigned the value of the "field" property in the "order" JToken, or "id" if the property is not found.
                    string? field = (orderValueToken.SelectToken("field"))?.ToString() ?? "id";
                    // The "way" is assigned the value of the "way" property in the "order" JToken, or "asc" if the property is not found.
                    string? way = (orderValueToken.SelectToken("way"))?.ToString() ?? "asc";
                    // If both "field" and "way" have values, the query is ordered based on the values.
                    if (!string.IsNullOrEmpty(field) && !string.IsNullOrEmpty(way))
                    {
                        query = query.OrderBy($"{field} {way}");
                    }


                }

            }


            //Modify the query based on the deleted entity filters.
            //if the withTrashed filter is sent, returns all.
            //if the withTrashed filter is sent and onlyTrashed is not send or is false, then will be all the deleted or non deleted items.
            //if the onlyTrashed filter is sent, returns deleted only.
            //else returns non deleted only (default).

            if (!query.GetType().FullName.Contains("Core.Log"))
            {
                bool withTrashed = internalFilter?.SelectToken("withTrashed") != null ? (internalFilter?.SelectToken("withTrashed").ToString() == "False" ? false : true) : false;

                if (!withTrashed)
                {
                    bool ignoreTrashedFilter = internalFilter?.SelectToken("ignoreTrashedFilter") != null ? (internalFilter?.SelectToken("ignoreTrashedFilter").ToString() == "False" ? false : true) : true;

                    if (internalFilter?.SelectToken("onlyTrashed") != null
                           && internalFilter?.SelectToken("onlyTrashed").ToString() == "True" && !ignoreTrashedFilter)
                    {
                        query = query.Where($"obj => obj.deleted_at != null");

                    }
                    else {
                        query = query.Where($"obj => obj.deleted_at == null");

                    }
                }
                else {
                    return query;
                }

            }




            return query;
        }



        /// <summary>
        /// Gets the actual requestBase timezone priorizing DB timezone, if not found then use setting timezone if nout founf the returns empty string.
        /// </summary>
        /// <returns>IANA timezone or empty string</returns>
        public string? getRequestTimezone()
        {
            //get timezone setting if present
            GetSetting("timezone", out string? settingTimezone);

            string? finalTimezone = useUserTimezone && currentContextUser != null && !string.IsNullOrEmpty($"{currentContextUser.timezone}") ? currentContextUser.timezone : settingTimezone ?? "";

            return finalTimezone;

        }

        public string? getRequestTimezoneOffset()
        {
            //get timezone setting if present
            GetSetting("timezone", out string? settingTimezone); //America/Bogota

            //Convert from normal timezone (region/city) to usable timezone ("04:00")
            if (settingTimezone != null) settingTimezone = TimezoneHelper.getTimezoneOffset(settingTimezone);

            //When auth helper doesn't find a timezone assigns "00:00" for reperence only to the timezone of requesting user
            //if not present in both them return default value 00:00
            string? finalTimezone = useUserTimezone && currentContextUser != null && currentContextUser.timezone_offset != "00:00" && !string.IsNullOrEmpty($"{currentContextUser.timezone_offset}") ? currentContextUser.timezone_offset : settingTimezone ?? "00:00";

            return finalTimezone;

        }

    }
}
