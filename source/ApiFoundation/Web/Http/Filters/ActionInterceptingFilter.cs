using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ApiFoundation.Web.Http.Filters
{
    internal sealed class ActionInterceptingFilter : ActionFilterAttribute
    {
        internal Action<HttpActionContext> ActionExecutingDelegate { get; set; }

        internal Action<HttpActionExecutedContext> ActionExecutedDelegate { get; set; }

        public override void OnActionExecuting(HttpActionContext context)
        {
            if (this.ActionExecutingDelegate != null)
            {
                this.ActionExecutingDelegate(context);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (this.ActionExecutedDelegate != null)
            {
                this.ActionExecutedDelegate(context);
            }
        }
    }
}