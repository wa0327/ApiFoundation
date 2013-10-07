using System;
using System.Web.Http;

namespace ApiFoundation.Net.Http
{
    /// <summary>
    /// 當呼叫端的要求無法讓被呼叫端接受處理誤時擲出。
    /// 例如：商業邏輯錯誤。
    /// </summary>
    public sealed class InvocationNotAcceptableException : Exception
    {
        private readonly HttpError httpError;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationNotAcceptableException" /> class.
        /// </summary>
        /// <param name="httpError">The HTTP error.</param>
        /// <exception cref="System.ArgumentNullException">errorCode</exception>
        internal InvocationNotAcceptableException(HttpError httpError)
            : base(httpError.Message)
        {
            this.httpError = httpError;
        }

        public string ErrorCode
        {
            get { return (string)this.httpError["ReturnCode"]; }
        }
    }
}