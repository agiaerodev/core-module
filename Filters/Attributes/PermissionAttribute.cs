using Core.Exceptions;
using Ihelpers.Helpers;
using Ihelpers.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Idata.Data;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Core.Logger;
using Idata.Data.Entities.Iprofile;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Core.Filters.Attributes
{
    public class PermissionAttribute : ActionFilterAttribute
    {
        private readonly string[] _permissions;

        public PermissionAttribute(string permissions)
        {

            _permissions = permissions.Split(',');

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {


            var isActionAuthorized = context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType() == typeof(AuthorizeAttribute));

            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var isControllerAuthorized = controllerActionDescriptor?.ControllerTypeInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any() ?? false;

            if (isActionAuthorized || isControllerAuthorized)
            {
                using (var dbContext = new IdataContext())
                {
                    var id = Ihelpers.Helpers.JWTHelper.getJWTTokenClaim(context.HttpContext.Request.Headers.Authorization.ToString(), "UserId");
                    var user = dbContext.Users.Include("roles").Include("departments").Where(u => u.id == Convert.ToInt64(id)).FirstOrDefault();

                    var controllerName = (string)context.RouteData.Values["controller"];
                    var actionName = (string)context.RouteData.Values["action"];
                    var endpoint = $"{controllerName}/{actionName}";


                    if (user != null)
                    {
                        user.Initialize();

                        foreach (var permission in _permissions)
                        {
                            if (!user.HasAccess(permission))
                            {
                                context.Result = new UnauthorizedObjectResult($"User {user.email} tried to access {endpoint} without proper permissions.");

                                CoreLogger.LogMessage($"User {user.email} tried to access {endpoint} without proper permissions.");

                                return;
                            }
                        }
                    }
                    else
                    {
                        context.Result = new UnauthorizedObjectResult($"{endpoint} doesn't allow anonymous access / user not found");
                    }

                }
            }

        }
    }
}
