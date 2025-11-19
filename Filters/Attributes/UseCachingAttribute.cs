using Azure;
using Core.Exceptions;
using Core.Interfaces;
using Core.Repositories;
using Core.Transformers;
using Idata.Data.Entities.Iprofile;
using Idata.Entities.Core;
using Idata.Entities.Test;
using Ihelpers.Caching.Interfaces;
using Ihelpers.Helpers;
using Ihelpers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TypeSupport.Assembly;
using static Azure.Core.HttpHeader;

namespace Core.Filters.Attributes
{
    public class UseCaching : Attribute, IAsyncActionFilter
    {

        ICacheBase _cache;
        public UseCaching(ICacheBase cache)
        {
            _cache = cache;

        }

        public virtual void CustomRequestFormatting(UrlRequestBase wichRequest)
        {
            return;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isActionAuthorized = !context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType() == typeof(IgnoreCaching));

            if (isActionAuthorized)
            {
                var controller = context.Controller as dynamic;

                var repository = controller._repositoryBase;

                context.ActionArguments.TryGetValue("urlRequestBase", out object urlRequestBaseObj);

                var requestBase = urlRequestBaseObj as UrlRequestBase;

                CustomRequestFormatting(requestBase);

                try
                {
                    await requestBase.Parse(controller);

                    var dbQuery = await repository.GetOrCreateQueryIndex(requestBase) as IQueryable<EntityBase>;

                    var entityType = controller._entityType as Type;

                    var requestingUser = controller._authUser;

                    string dbQueryString = dbQuery.ToQueryString();

                    var requestUrl = context.HttpContext.Request.QueryString;


                    var cacheKey = $"{entityType.FullName}_{dbQueryString}_{requestBase.page}_{requestBase.take}_{requestBase.GetSetting("timezone") ?? requestingUser.timezone ?? ""}_transformed";

                    // Attempt to get the values from the configured cache if the class is cacheable

                    object? response = await _cache.GetValue(cacheKey);

                    // If the response is not in the cache (or the class is not cacheable), fetch the data
                    if (response != null)
                    {
                        context.Result = new ObjectResult(response);

                        Task.Factory.StartNew(() => repository.LogAction($"has listed: {entityType.Name}", logType: LogType.Information, requestBase: requestBase));

                    }
                    else
                    {
                        var executedContext = await next();

                        if (executedContext.Result != null && executedContext.Exception == null)
                        {
                            //Guardar en el cache con key y tags
                            var mvcResult = executedContext.Result as Microsoft.AspNetCore.Mvc.ObjectResult;

                            if (mvcResult.StatusCode == 200)
                            {
                                Task.Factory.StartNew(() => _cache.Remember(cacheKey, mvcResult.Value, new List<string>() { $"{requestingUser?.GetType().FullName ?? "null"}.{requestingUser?.id ?? "null"}", entityType.FullName }));

                                context.Result = mvcResult;
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    var requestQueryString = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString.ToString();

                    Task.Factory.StartNew(() => ExceptionBase.HandleSilentException(
                    ex,
                    $"Error inside caching attribute",
                    $"ExceptionMessage = {ex.Message}  trace received: " +
                    $"  User = {controller._authUser?.email} userId = {controller._authUser?.id}  RequestQueryString = {requestQueryString} "
                    ));
                    //Task.Factory.StartNew(() => ExceptionBase.HandleSilentException(ex, $"Error inside caching attribute", $"ExceptionMessage = {ex.Message}   trace received: " + JsonConvert.SerializeObject(requestBase, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }).Trim().Replace("\"", "'")));

                    var executedContext = await next();
                }

            }
            else
            {
                await next();
            }


        }
    }
}