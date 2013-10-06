﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Net.Http
{
    internal sealed class TimestampServiceHandler : DelegatingHandler
    {
        internal const string ServiceName = "!TimestampService!";
        internal const string RouteTemplate = "!timestamp!/{action}";
        internal const string GetUri = "/!timestamp!/get";

        internal static TimestampServiceHandler Register(HttpConfiguration configuration, ITimestampService timestampService)
        {
            TimestampServiceHandler handler;

            IHttpRoute route;
            if (configuration.Routes.TryGetValue(ServiceName, out route))
            {
                handler = (TimestampServiceHandler)route.Handler;
            }
            else
            {
                handler = new TimestampServiceHandler();
                configuration.Routes.MapHttpRoute(ServiceName, RouteTemplate, null, null, handler);
            }

            handler.TimestampService = timestampService;

            return handler;
        }

        private ITimestampService service;

        private TimestampServiceHandler()
        {
        }

        internal ITimestampService TimestampService
        {
            get { return this.service; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("null");
                }

                this.service = value;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<HttpResponseMessage>();

            string timestamp;
            DateTime expires;
            this.service.GetTimestamp(out timestamp, out expires);

            var response = new
            {
                Timestamp = timestamp,
                Expires = expires,
            };

            var responseMessage = request.CreateResponse(HttpStatusCode.OK, response);
            source.SetResult(responseMessage);

            return source.Task;
        }
    }
}