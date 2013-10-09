using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiFoundation.Web
{
    internal class MyHttpHandler : IHttpHandler
    {
        public MyHttpHandler()
        {
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var workerRequest = context.Request;
        }
    }
}