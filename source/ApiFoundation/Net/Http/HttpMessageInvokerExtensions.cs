using System;
using System.Net.Http;

namespace ApiFoundation.Net.Http
{
    public static class HttpMessageInvokerExtensions
    {
        /// <summary>
        /// 在 HttpMessageHandler 之前插入一個 DelegatingHandler
        /// </summary>
        /// <param name="source"></param>
        /// <param name="handler"></param>
        public static void InsertHandler(this HttpMessageInvoker source, DelegatingHandler handler)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            var current = typeof(HttpMessageInvoker).GetField<HttpMessageHandler>(source, "handler");
            DelegatingHandler delegatingHandler = null;
            while (true)
            {
                if (current is DelegatingHandler)
                {
                    delegatingHandler = (DelegatingHandler)current;
                    current = delegatingHandler.InnerHandler;
                    continue;
                }

                break;
            }

            handler.InnerHandler = current;
            if (delegatingHandler != null)
            {
                delegatingHandler.InnerHandler = handler;
                handler = delegatingHandler;
            }

            typeof(HttpMessageInvoker).SetField(source, "handler", handler);
        }
    }
}