using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using ApiFoundation.Services;

namespace ApiFoundation.Web.Http.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public event EventHandler<ExceptionEventArgs> Exception;

        public override void OnException(HttpActionExecutedContext context)
        {
            var e = new ExceptionEventArgs(context.Exception);

            this.OnException(e);

            if (e.Handled)
            {
                var bizError = new HttpError(e.Message);
                bizError["ReturnCode"] = e.ReturnCode;
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, bizError);
            }
            else
            {
                var error = new HttpError(context.Exception, true);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }
        }

        protected virtual void OnException(ExceptionEventArgs e)
        {
            if (this.Exception != null)
            {
                this.Exception(this, e);
            }
        }
    }
}