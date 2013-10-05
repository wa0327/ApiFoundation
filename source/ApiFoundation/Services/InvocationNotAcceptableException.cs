using System;
using System.Web.Http;

namespace ApiFoundation.Services
{
    /// <summary>
    /// 當被呼叫端發生商業邏輯錯誤時擲出。
    /// </summary>
    public sealed class InvocationNotAcceptableException : Exception
    {
        private readonly HttpError httpError;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationNotAcceptableException" /> class.
        /// </summary>
        /// <param name="httpError">The HTTP error.</param>
        /// <exception cref="System.ArgumentNullException">errorCode</exception>
        public InvocationNotAcceptableException(HttpError httpError)
            : base(httpError.Message)
        {
            this.httpError = httpError;
        }

        public string ErrorCode
        {
            get { return (string)this.httpError["ErrorCode"]; }
        }
    }
}