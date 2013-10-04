using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class ApiServer : IApiServer
    {
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
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IExceptionHandler ExceptionHandler
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<HttpRequestEventArgs> RequestReceived;

        public event EventHandler<HttpResponseEventArgs> SendingResponse;

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

        protected virtual void OnRegisterMessageHandlers(Collection<DelegatingHandler> handlers)
        {
            var interceptor = new MessageInterceptingHandler
            {
                ProcessRequestDelegate = request => this.OnRequestReceived(request),
                ProcessResponseDelegate = response => this.OnSendingResponse(response),
            };

            handlers.Add(interceptor);
        }

        protected virtual void OnRegisterServices(ServicesContainer services)
        {
        }

        protected virtual void OnRegisterFilters(HttpFilterCollection filters)
        {
            filters.Add(new InvalidModelFilterAttribute());
        }

        protected virtual void OnRegisterRoute(HttpRouteCollection routes)
        {
            routes.MapHttpRoute("Timestamp", "api/TimeStampService", new { controller = "TimestampController" });
        }

        protected virtual void OnRegisterFormatters(MediaTypeFormatterCollection formatters)
        {
        }
    }
}