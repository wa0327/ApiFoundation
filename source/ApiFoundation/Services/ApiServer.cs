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
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using ApiFoundation.Net.Http;
using ApiFoundation.Web.Http.Filters;

namespace ApiFoundation.Services
{
    public class ApiServer
    {
        public ApiServer(HttpConfiguration configuration, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (routeTemplate == null)
            {
                throw new ArgumentNullException("routeTemplate");
            }

            if (handler == null)
            {
                handler = new HttpControllerDispatcher(configuration);
            }

            var interceptor = new MessageInterceptingHandler
            {
                InnerHandler = handler,
            };
            interceptor.ProcessingRequest += (sender, e) => this.OnRequestReceived(e);
            interceptor.ProcessingResponse += (sender, e) => this.OnSendingResponse(e);

            configuration.Routes.MapHttpRoute(name, routeTemplate, defaults, constraints, interceptor);
        }

        public event EventHandler<HttpRequestEventArgs> RequestReceived;

        public event EventHandler<HttpResponseEventArgs> SendingResponse;

        protected virtual void OnRequestReceived(HttpRequestEventArgs e)
        {
            if (this.RequestReceived != null)
            {
                this.RequestReceived(this, e);
            }
        }

        protected virtual void OnSendingResponse(HttpResponseEventArgs e)
        {
            if (this.SendingResponse != null)
            {
                this.SendingResponse(this, e);
            }
        }
    }
}