using Core.Entities;
using Core.Exceptions;
using Core.Filters.Attributes;
using Core.Interfaces;
using Core.Transformers;
using Hangfire;
using Idata.Data;
using Idata.Entities.Core;
using Ihelpers.Caching.Interfaces;
using Ihelpers.Extensions;
using Ihelpers.Helpers;
using Ihelpers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Core.Controllers
{
    /// <summary>
    /// API controller base class for generic repository operations.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class ControllerBase<TEntity> : Controller where TEntity : EntityBase
    {

        /// <summary>
        /// Defines the type of the current Entity
        /// </summary>
        public readonly Type _entityType = typeof(TEntity);

        /// <summary>
        /// The repository base.
        /// </summary>
        public IRepositoryBase<TEntity> _repositoryBase { get; set; }

        /// <summary>
        /// The context.
        /// </summary>
        protected readonly dynamic? _context;

        /// <summary>
        /// The database set.
        /// </summary>
        protected DbSet<TEntity> _dbSet { get; set; }

        /// <summary>
        /// The authentication user.
        /// </summary>
        public dynamic? _authUser { get; set; }

        /// <summary>
        /// The cache base.
        /// </summary>
        public ICacheBase _cache;

        /// <summary>
        /// The hangfire job scheduler
        /// </summary>
        public IBackgroundJobClient _backgroundJobClient;

        //Constructor wich sets dataContext
        public ControllerBase(IRepositoryBase<TEntity> repositoryBase)
        {

            _context = new IdataContext();


            _dbSet = _context.Set<TEntity>();

            repositoryBase.Initialize(_context);

            _repositoryBase = repositoryBase;



        }

        //Constructor wich sets dataContext and the user given by Iprofile.AuthUser 
        public ControllerBase(IRepositoryBase<TEntity> repositoryBase, dynamic? authUser)
        {
            _context = new IdataContext();

            _dbSet = _context.Set<TEntity>();

            //Necesary to Repository to access current user
            repositoryBase.Initialize(_context, authUser);

            _repositoryBase = repositoryBase;

            _authUser = authUser;

        }

        //Constructor wich sets dataContext, the user given by Iprofile.AuthUser and the BackgroundJobClient to use with bulk actions
        public ControllerBase(IRepositoryBase<TEntity> repositoryBase, dynamic? authUser, IBackgroundJobClient backgroundJobClient)
        {
            _context = new IdataContext();

            _dbSet = _context.Set<TEntity>();

            //Necesary to Repository to access current user
            repositoryBase.Initialize(_context, authUser);

            _repositoryBase = repositoryBase;

            _authUser = authUser;

            _backgroundJobClient = backgroundJobClient;

        }

        //Default Constructor
        public ControllerBase()
        {


        }

        /// <summary>
        /// The Index action returns a collection of items based on the passed URL request.
        /// </summary>
        /// <param name="urlRequestBase">The URL request object used to filter the items.</param>
        /// <returns>A collection of items, or a corresponding error message if any errors occur.</returns>
        
        [ServiceFilter(typeof(UseCaching))]
        [HttpGet("")]
        public virtual async Task<IActionResult> Index([FromQuery] UrlRequestBase? urlRequestBase)
        {
            int status = 200;
            dynamic? response = null;
            dynamic? meta = new object();
            try
            {
                //Parse the URL request to extract the necessary information
                await urlRequestBase.Parse(this);

                //Get the items based on the URL request
                response = await _repositoryBase.GetItemsBy(urlRequestBase);

                //Get the meta data before transforming the response, as the transformation may cause loss of meta information
                meta = await ResponseBase.GetMeta(response);

                //Transform the response to match the camelCase format
                response = await TransformerBase.TransformCollection(response, _cache, userTimezone: urlRequestBase.getRequestTimezone());
            }
            catch (ExceptionBase ex)
            {
                //Return a status code and the error message in the response if an error occurs
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }
            //Return the response with the desired status code
            return StatusCode(status, await ResponseBase.Response(response, meta));
        }

        /// <summary>
        /// This HttpGet method returns an item based on the criteria and UrlRequestBase query
        /// </summary>
        /// <param name="criteria">The criteria used to get the IActionResult</param>
        /// <param name="urlRequestBase">The urlRequestBase query</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{criteria}")]
        public virtual async Task<IActionResult> Show(string? criteria, [FromQuery] UrlRequestBase? urlRequestBase)
        {
            int status = 200;
            object? response = null;
            try
            {
                // Parse the URL request base
                await urlRequestBase.Parse(this);

                // Set the criteria
                urlRequestBase.criteria = criteria;

                // Get the item from the repository
                response = await _repositoryBase.GetItem(urlRequestBase);

                // Transform the item
                response = await TransformerBase.TransformItem(response, _cache, userTimezone: urlRequestBase.getRequestTimezone());

            }
            catch (ExceptionBase ex)
            {
                if(ex.CodeResult == 204)
                {
                    return NoContent();
                }
                // Return the code result and exception response
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }

            // Return the status code and response
            return StatusCode(status, await ResponseBase.Response(response));
        }

        /// <summary>
        /// The Create action allows creation of a new item based on the submitted URL and body request.
        /// </summary>
        /// <param name="urlRequestBase">The URL request object used to filter the items.</param>
        /// <param name="bodyRequestBase">The body request object containing the data for the new item.</param>
        /// <returns>The newly created item, or a corresponding error message if any errors occur.</returns>
        [HttpPost("")]
        public virtual async Task<IActionResult> Create([FromQuery] UrlRequestBase? urlRequestBase, [FromBody] BodyRequestBase? bodyRequestBase)
        {
            int status = 200;
            object? response = null;

            try
            {
                //Parse the URL request to extract the necessary information
                await urlRequestBase.Parse(this);

                //Parse the body request to extract the necessary information
                await bodyRequestBase.Parse();

                //Create the new item based on the URL and body request
                response = await _repositoryBase.Create(urlRequestBase, bodyRequestBase);

                //Transform the created item to match the camelCased format, applying requesting user timezone
                response = await TransformerBase.TransformItem(response, _cache, userTimezone: urlRequestBase.getRequestTimezone());
            }
            catch (ExceptionBase ex)
            {
                //Return a status code with error and the error message in the response if an error occurs
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }
            //Return the response with the desired status code
            return StatusCode(status, await ResponseBase.Response(response));
        }


        /// <summary>
        /// The Update action allows updating an existing item based on the submitted criteria, URL, and body request.
        /// </summary>
        /// <param name="criteria">The identifier used to determine which item to update.</param>
        /// <param name="urlRequestBase">The URL request object used to filter the items.</param>
        /// <param name="bodyRequestBase">The body request object containing the updated data for the item.</param>
        /// <returns>The updated item, or a corresponding error message if any errors occur.</returns>
        [HttpPut("")]
        [HttpPut("{criteria}")]
        public virtual async Task<IActionResult> Update(string? criteria, [FromQuery] UrlRequestBase? urlRequestBase, [FromBody] BodyRequestBase? bodyRequestBase)
        {
            object? response = null;
            int status = 200;
            try
            {
                //Parse the URL request to extract the necessary information
                await urlRequestBase.Parse(this);

                //Parse the body request to extract the necessary information
                await bodyRequestBase.Parse();

                //Set the criteria in the URL request to the passed criteria
                urlRequestBase.criteria = criteria;

                //Update the item based on the criteria, URL, and body request
                response = await _repositoryBase.UpdateBy(urlRequestBase, bodyRequestBase);

                //Transform the response to match the desired format
                response = await TransformerBase.TransformItem(response, _cache, userTimezone: urlRequestBase.getRequestTimezone());

            }
            catch (ExceptionBase ex)
            {
                //Return a status code and the error message in the response if an error occurs
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }
            //Return the response with the desired status code
            return StatusCode(status, await ResponseBase.Response(response));
        }


        /// <summary>
        /// Deletes a record from the database based on the criteria provided in the URL request.
        /// </summary>
        /// <param name="criteria">The identifier for the record to be deleted.</param>
        /// <param name="urlRequestBase">A URL request object containing request parameters.</param>
        /// <returns>A 200 status code indicating a successful deletion or an error message with a corresponding status code.</returns>
        [HttpDelete("{criteria}")]
        public virtual async Task<IActionResult> Delete(string? criteria, [FromQuery] UrlRequestBase? urlRequestBase)
        {
            object response = null;
            int status = 200;
            try
            {
                // Parse the URL request
                await urlRequestBase.Parse(this);

                // Set the criteria for the record to be deleted
                urlRequestBase.criteria = criteria;

                // Delete the record from the repository
                response = await _repositoryBase.DeleteBy(urlRequestBase);

            }
            catch (ExceptionBase ex)
            {
                // Return an error message with the corresponding status code
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }

            // Return a 200 status code and the deleted object
            return StatusCode(status, response);
        }

        /// <summary>
        /// Restores a previous deleted entity based on the criteria provided in the URL request.
        /// </summary>
        /// <param name="criteria">The identifier for the record to be restored.</param>
        /// <param name="urlRequestBase">A URL request object containing request parameters.</param>
        /// <returns>A 200 status code indicating a successful deletion or an error message with a corresponding status code.</returns>
        [HttpPut("restore/{criteria}")]
        [HttpPut("restore")]
        public virtual async Task<IActionResult> Restore(string? criteria, [FromQuery] UrlRequestBase? urlRequestBase)
        {
            object response = null;
            int status = 200;
            try
            {
                // Parse the URL request
                await urlRequestBase.Parse(this);

                // Set the criteria for the record to be restored
                urlRequestBase.criteria = criteria;

                // Restore the record from the repository
                response = await _repositoryBase.RestoreBy(urlRequestBase);

            }
            catch (ExceptionBase ex)
            {
                // Return an error message with the corresponding status code
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }

            // Return a 200 status code and the restored object
            return StatusCode(status, response);
        }

        /// <summary>
        /// Export method for exporting data entity based
        /// </summary>
        /// <param name="urlRequestBase">The URL request object used to filter the items.</param>
        /// <param name="bodyRequestBase">The body request object containing the configuration for item export.</param>
        /// <returns>Action result with HTTP status code</returns>
        [HttpPost("export")]
        public virtual async Task<IActionResult> Export([FromQuery] UrlRequestBase? urlRequestBase, [FromBody] BodyRequestBase? bodyRequestBase)
        {
            // HTTP status code
            int status = 200;

            try
            {
                // Disable permission check
                urlRequestBase.doNotCheckPermissions();


                //Set ExportParam if null or empty
                if (string.IsNullOrEmpty(urlRequestBase.exportParams)) urlRequestBase.exportParams = "{}";

                // Parse URL request parameters
                await urlRequestBase.Parse(this);

                // Parse body request parameters
                await bodyRequestBase.Parse();

#if DEBUG
                await _repositoryBase.CreateExport(urlRequestBase, bodyRequestBase);
#else

                _backgroundJobClient.Enqueue(() => _repositoryBase.CreateExport(urlRequestBase, bodyRequestBase));
                    
#endif

				//await _repositoryBase.CreateExport(urlRequestBase, bodyRequestBase);

                // Return HTTP status code
                return StatusCode(status);

            }
            catch (ExceptionBase ex)
            {
                // Return HTTP status code and exception response
                return StatusCode(ex.CodeResult, ex.CreateResponseFromException());
            }

        }


       

    }
}
