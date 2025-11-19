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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TypeSupport.Assembly;
using static Azure.Core.HttpHeader;

namespace Core.Filters.Attributes
{
    //This attribute is used to set ignore caching on certain endpoints
    public class IgnoreCaching: Attribute, IAsyncActionFilter 
    {

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();
        }
    }
}