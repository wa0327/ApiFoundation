using System;
using System.Net;
using System.Web.Http;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 當被呼叫端發生錯誤時擲出。
    /// </summary>
    public sealed class HttpServiceException : Exception
    {
        private readonly HttpStatusCode statusCode;
        private readonly HttpError httpError;

        public HttpServiceException(HttpStatusCode statusCode, HttpError httpError)
            : base(httpError.Message)
        {
            this.statusCode = statusCode;
            this.httpError = httpError;
        }

        public string MessageDetail
        {
            get
            {
                object messageDetail;
                if (this.httpError.TryGetValue("MessageDetail", out messageDetail))
                {
                    return (string)messageDetail;
                }

                return null;
            }
        }

        public HttpStatusCode StatusCode
        {
            get { return this.statusCode; }
        }

        public string ExceptionMessage
        {
            get
            {
                object exceptionMessage;
                if (this.httpError.TryGetValue("ExceptionMessage", out exceptionMessage))
                {
                    return (string)exceptionMessage;
                }

                return null;
            }
        }

        public string ExceptionType
        {
            get
            {
                object exceptionType;
                if (this.httpError.TryGetValue("ExceptionType", out exceptionType))
                {
                    return (string)exceptionType;
                }

                return null;
            }
        }

        public string ExceptionStackTrace
        {
            get
            {
                object stackTrace;
                if (this.httpError.TryGetValue("StackTrace", out stackTrace))
                {
                    return (string)stackTrace;
                }

                return null;
            }
        }
    }
}