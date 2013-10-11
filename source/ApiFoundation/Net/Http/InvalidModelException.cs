using System;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Net.Http
{
    /// <summary>
    /// 當呼叫端送出的格式有問題時擲出。
    /// </summary>
    public sealed class InvalidModelException : Exception
    {
        private readonly HttpError httpError;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidModelException"/> class.
        /// </summary>
        /// <param name="httpError">The HTTP error.</param>
        internal InvalidModelException(HttpError httpError)
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