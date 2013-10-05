using System;
using System.Net.Http;
using System.Reflection;

namespace ApiFoundation.Net.Http
{
    public static class HttpMessageInvokerExtensions
    {
        private static FieldInfo handlerFieldInfo;

        static HttpMessageInvokerExtensions()
        {
            handlerFieldInfo = typeof(HttpMessageInvoker).GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static HttpMessageHandler GetMessageHandler(this HttpMessageInvoker source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (HttpMessageHandler)handlerFieldInfo.GetValue(source);
        }

        public static void SetMessageHandler(this HttpMessageInvoker source, HttpMessageHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            handlerFieldInfo.SetValue(source, handler);
        }

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

            var current = source.GetMessageHandler();
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

            source.SetMessageHandler(handler);
        }
    }
}