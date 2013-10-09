using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ApiFoundation.Web
{
    internal class MyHttpModule : IHttpModule
    {
        private HttpApplication app;

        public void Init(HttpApplication app)
        {
            app.BeginRequest += (sender, e) =>
            {
                var request = app.Request;
                Trace.TraceInformation("HttpModule [RECV {0}]", request.Headers["From"]);
            };

            app.EndRequest += (sender, e) =>
            {
                var response = app.Response;

                Trace.TraceInformation("HttpModule [REPLY]");
            };

            this.app = app;
        }

        private void app_BeginRequest(object sender, EventArgs e)
        {
        }

        public void Dispose()
        {
        }
    }
}