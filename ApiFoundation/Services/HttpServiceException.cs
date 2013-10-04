using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 當被呼叫端發生非商業邏輯錯誤時擲出。
    /// </summary>
    public sealed class HttpServiceException : Exception
    {
        private readonly HttpResponseMessage responseMessage;
        private readonly string message;

        internal HttpServiceException(HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
            {
                throw new ArgumentNullException("responseMessage");
            }

            this.responseMessage = responseMessage;

            if (responseMessage.Content != null)
            {
                this.message = responseMessage.Content.ReadAsStringAsync().Result;
            }
        }

        public override string Message
        {
            get { return this.message; }
        }

        public HttpStatusCode StatusCode
        {
            get { return this.responseMessage.StatusCode; }
        }
    }
}