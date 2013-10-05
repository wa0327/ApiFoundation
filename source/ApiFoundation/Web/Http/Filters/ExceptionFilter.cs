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
            if (this.Exception != null)
            {
                this.Exception(this, e);
            }

            if (e.IsBusinessError)
            {
                var bizError = new HttpError(e.ErrorMessage);
                bizError["ErrorCode"] = e.ErrorCode;
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, bizError);
            }
            else
            {
                // expression of other errors
                var error = new HttpError(context.Exception.Message);
                error["ErrorType"] = context.Exception.GetType().FullName;
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