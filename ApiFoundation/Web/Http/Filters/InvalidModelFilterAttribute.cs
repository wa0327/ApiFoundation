using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ApiFoundation.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    internal sealed class InvalidModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid)
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                var errorSummary = new Dictionary<string, IEnumerable<string>>();

                foreach (var pair in actionContext.ModelState)
                {
                    var name = pair.Key;
                    var modelState = pair.Value;

                    if (modelState.Errors.Count > 0)
                    {
                        errorSummary[pair.Key] = modelState.Errors.Select(
                            error =>
                            {
                                if (string.IsNullOrEmpty(error.ErrorMessage))
                                {
                                    return error.Exception.Message;
                                }

                                return error.ErrorMessage;
                            }
                        );
                    }
                }

                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, errorSummary);
            }
        }
    }
}