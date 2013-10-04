using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ApiFoundation.Net.Http;
using ApiFoundation.Web.Http.Filters;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 起始 API server 基本設定。
    /// </summary>
    /// <remarks>
    /// 請將此設定套件放置於設置其它 HttpConfiguration 邏輯之前。
    /// </remarks>
    public class ApiServer : IApiServer
    {
        private ILogWriter logWriter;

        public ApiServer(HttpConfiguration configuration)
        {
            this.OnRegisterMessageHandlers(configuration.MessageHandlers);
            this.OnRegisterServices(configuration.Services);
            this.OnRegisterFilters(configuration.Filters);
            this.OnRegisterRoute(configuration.Routes);
            this.OnRegisterFormatters(configuration.Formatters);
        }

        public ILogWriter LogWriter
        {
            get { return this.logWriter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.logWriter = value;
            }
        }

        public event EventHandler<HttpRequestEventArgs> RequestReceived;

        public event EventHandler<HttpResponseEventArgs> SendingResponse;

        public event EventHandler<ExceptionEventArgs> Exception;

        protected virtual void OnRequestReceived(HttpRequestMessage request)
        {
            if (this.RequestReceived != null)
            {
                var e = new HttpRequestEventArgs(request);
                this.RequestReceived(this, e);
            }
        }

        protected virtual void OnSendingResponse(HttpResponseMessage response)
        {
            if (this.SendingResponse != null)
            {
                var e = new HttpResponseEventArgs(response);
                this.SendingResponse(this, e);
            }
        }

        protected virtual void OnActionExecuting(HttpActionContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, context.ModelState);
            }
        }

        protected virtual void OnActionExecuted(HttpActionExecutedContext context)
        {
        }

        protected virtual void OnException(HttpActionExecutedContext context)
        {
            if (this.Exception == null)
            {
                return;
            }

            var e = new ExceptionEventArgs(context.Exception);
            this.Exception(this, e);

            if (e.IsBusinessError)
            {
                var error = new HttpError(e.ErrorMessage);
                error["ErrorCode"] = e.ErrorCode;
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, error);
            }
            else
            {
                var error = new HttpError(e.Exception.Message);
                error["ErrorType"] = e.Exception.GetType().FullName;
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }
        }

        protected virtual void OnRegisterMessageHandlers(Collection<DelegatingHandler> handlers)
        {
            // 連接 http message 處理程序
            handlers.Add(new MessageInterceptingHandler
            {
                ProcessRequestDelegate = request => this.OnRequestReceived(request),
                ProcessResponseDelegate = response => this.OnSendingResponse(response),
            });
        }

        protected virtual void OnRegisterServices(ServicesContainer services)
        {
        }

        protected virtual void OnRegisterFilters(HttpFilterCollection filters)
        {
            // 連接 model validation 處理程序
            filters.Add(new ActionInterceptingFilter
            {
                ActionExecutingDelegate = context => this.OnActionExecuting(context),
                ActionExecutedDelegate = context => this.OnActionExecuted(context),
            });

            // 連接 exception 處理程序
            filters.Add(new ExceptionInterceptingFilter
            {
                ExceptionDelegate = context => this.OnException(context),
            });
        }

        protected virtual void OnRegisterRoute(HttpRouteCollection routes)
        {
            // 相容於舊版的 ApiFoundation
            routes.MapHttpRoute(
                "TimeStampService",
                "api/TimeStampService/GetTimeStamp",
                defaults: new
                {
                    controller = "Timestamp",
                    action = "Get"
                });
        }

        protected virtual void OnRegisterFormatters(MediaTypeFormatterCollection formatters)
        {
        }
    }
}