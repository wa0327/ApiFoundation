using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace ApiFoundation.Web.Http.Filters
{
    internal sealed class ExceptionInterceptingFilter : ExceptionFilterAttribute
    {
        internal Action<HttpActionExecutedContext> ExceptionDelegate { get; set; }

        public override void OnException(HttpActionExecutedContext context)
        {
            if (this.ExceptionDelegate != null)
            {
                this.ExceptionDelegate(context);
            }
        }
    }
}