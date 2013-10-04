using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 當被呼叫端發生非商業邏輯錯誤時擲出。
    /// </summary>
    public sealed class HttpServiceException : Exception
    {
        private readonly HttpStatusCode statusCode;
        private readonly HttpError httpError;

        internal HttpServiceException(HttpStatusCode statusCode, HttpError httpError)
            : base(httpError.Message)
        {
            this.statusCode = statusCode;
            this.httpError = httpError;
        }

        public HttpStatusCode StatusCode
        {
            get { return this.statusCode; }
        }

        public string ErrorType
        {
            get
            {
                object errorType;
                if (this.httpError.TryGetValue("ErrorType", out errorType))
                {
                    return (string)errorType;
                }

                return null;
            }
        }

        public string MessageDetail
        {
            get
            {
                object detail;
                if (this.httpError.TryGetValue("MessageDetail", out detail))
                {
                    return (string)detail;
                }

                return null;
            }
        }
    }
}