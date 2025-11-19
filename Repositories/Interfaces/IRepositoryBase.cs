using Ihelpers.Helpers;

namespace Core.Interfaces
{
    /// <summary>
    /// The `IRepositoryBase` interface defines the basic operations that can be performed by a repository.
    /// </summary>
    [Obsolete("Use IreposirotyBase<TEntity> instead")]
    public interface IRepositoryBase
    {
        /// <summary>
        /// Retrieves a list of items from the repository, based on the given request.
        /// </summary>
        /// <param name="requestBase">The request that specifies the criteria for the items to be retrieved.</param>
        /// <returns>A list of items that match the criteria specified in the request.</returns>
        public Task<List<object>> GetItemsBy(UrlRequestBase? requestBase);

        /// <summary>
        /// Retrieves a single item from the repository, based on the given request.
        /// </summary>
        /// <param name="requestBase">The request that specifies the criteria for the item to be retrieved.</param>
        /// <returns>The item that matches the criteria specified in the request, or `null` if no such item was found.</returns>
        public Task<object?> GetItem(UrlRequestBase? requestBase);

        /// <summary>
        /// Creates a new item in the repository, based on the given request and body.
        /// </summary>
        /// <param name="requestBase">The request that specifies the details of the item to be created.</param>
        /// <param name="bodyRequestBase">The body of the request, which contains the data for the item to be created.</param>
        /// <returns>The item that was created in the repository.</returns>
        public Task<object?> Create(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase);

        /// <summary>
        /// Updates an existing item in the repository, based on the given request and body.
        /// </summary>
        /// <param name="requestBase">The request that specifies the criteria for the item to be updated.</param>
        /// <param name="bodyRequestBase">The body of the request, which contains the updated data for the item.</param>
        /// <returns>The item that was updated in the repository.</returns>
        public Task<object> UpdateBy(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase);

        /// <summary>
        /// Deletes an existing item in the repository, based on the given request.
        /// </summary>
        /// <param name="requestBase">The request that specifies the criteria for the item to be deleted.</param>
        /// <returns>The item that was deleted from the repository, or `null` if no such item was found.</returns>
        public Task<object?> DeleteBy(UrlRequestBase? requestBase, dynamic? modelToRemove = null);
    }

    /// <summary>
    /// Repository base interface that provides methods for working with entities of type `TEntity`.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being managed by the repository.</typeparam>
    public interface IRepositoryBase<TEntity>
    {


        public Task<IQueryable<TEntity?>> GetOrCreateQueryShow(UrlRequestBase? requestBase);


        public Task<IQueryable<TEntity?>> GetOrCreateQueryIndex(UrlRequestBase? requestBase);

        /// <summary>
        /// Updates the ordering of the entity.
        /// </summary>
        /// <param name="requestBase">The base request object that contains information about the request.</param>
        /// <param name="bodyRequestBase">The base request body that contains information about the request body.</param>
        /// <returns>The updated entity of type `TEntity`.</returns>
        public Task<TEntity?> UpdateOrdering(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase);

        /// <summary>
        /// Gets a list of entities that match the specified conditions.
        /// </summary>
        /// <param name="requestBase">The base request object that contains information about the request.</param>
        /// <returns>A list of entities of type `TEntity` that match the specified conditions.</returns>
        public Task<List<TEntity?>> GetItemsBy(UrlRequestBase? requestBase);

        /// <summary>
        /// Gets a single entity that matches the specified conditions.
        /// </summary>
        /// <param name="requestBase">The base request object that contains information about the request.</param>
        /// <returns>The entity of type `TEntity` that matches the specified conditions.</returns>
        public Task<TEntity?> GetItem(UrlRequestBase? requestBase);

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="requestBase">The base request object that contains information about the request.</param>
        /// <param name="bodyRequestBase">The base request body that contains information about the request body.</param>
        /// <returns>The created entity of type `TEntity`.</returns>
        public Task<TEntity?> Create(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="requestBase">The base request object that contains information about the request.</param>
        /// <param name="bodyRequestBase">The base request body that contains information about the request body.</param>
        /// <returns>The updated entity of type `TEntity`.</returns>
        public Task<TEntity?> UpdateBy(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="requestBase">The base request object that contains information about the request.</param>
        /// <returns>The deleted entity of type `TEntity`.</returns>
        public Task<TEntity?> DeleteBy(UrlRequestBase? requestBase, dynamic? modelToRemove = null);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="requestBase">The base request object that contains information about the request.</param>
        /// <returns>The deleted entity of type `TEntity`.</returns>
        public Task<TEntity?> RestoreBy(UrlRequestBase? requestBase);

        /// <summary>
        /// Synchronizes relationships between entities.
        /// </summary>
        /// <param name="input">The input used to synchronize the relationships.</param>
        /// <param name="relations">The relationships to synchronize.</param>
        /// <param name="dataContext">The data context in which to synchronize the relationships.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SyncRelations(object? input, dynamic relations, dynamic dataContext);

        /// <summary>
        /// Applies custom filters to a query.
        /// </summary>
        /// <param name="query">The query to which to apply the custom filters.</param>
        /// <param name="requestBase">The request base used to apply the custom filters.</param>
        /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
        public void CustomFilters(ref IQueryable<TEntity> query, ref UrlRequestBase? requestBase);

        /// <summary>
        /// Performs operations before updating an entity.
        /// </summary>
        /// <param name="common">The common data used in the update operation.</param>
        /// <param name="requestBase">The request base used in the update operation.</param>
        /// <param name="bodyRequestBase">The body request base used in the update operation.</param>
        public void BeforeUpdate(ref TEntity? common, ref UrlRequestBase? requestBase, ref BodyRequestBase? bodyRequestBase);

        /// <summary>
        /// Initializes the current object.
        /// </summary>
        /// <param name="wichContext">The context used to initialize the object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Initialize(dynamic wichContext);

        /// <summary>
        /// Initializes the current object with a user context.
        /// </summary>
        /// <param name="wichContext">The context used to initialize the object.</param>
        /// <param name="wichUser">The user context used to initialize the object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Initialize(dynamic wichContext, dynamic wichUser);

        /// <summary>
        /// Logs an action.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="requestBase">The request base associated with the action being logged.</param>
        /// <param name="logType">The type of log to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task LogAction(string message, UrlRequestBase? requestBase, LogType logType = LogType.Information);

        /// <summary>
        /// Creates an export.
        /// </summary>
        /// <param name="requestBase">The request base associated with the export being created.</param>
        /// <param name="bodyRequestBase">The body request base associated with the export being created.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task CreateExport(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase);

        public Task<List<dynamic>> GetDynamicPropertiesList(UrlRequestBase urlRequestBase, List<(string, string)> keys);
        public void NullifyFinalQuery();
    }
}
