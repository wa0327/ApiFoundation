using System;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 當呼叫端送出的格式有問題時擲出。
    /// </summary>
    public sealed class BadInvocationException : Exception
    {
        private readonly HttpError httpError;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadInvocationException"/> class.
        /// </summary>
        /// <param name="httpError">The HTTP error.</param>
        internal BadInvocationException(HttpError httpError)
            : base(httpError.Message)
        {
            this.httpError = httpError;
        }

        public JObject ModelState
        {
            get { return (JObject)this.httpError["ModelState"]; }
        }
    }
}