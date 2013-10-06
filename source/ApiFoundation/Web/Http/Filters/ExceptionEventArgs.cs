using System;

namespace ApiFoundation.Web.Http.Filters
{
    public sealed class ExceptionEventArgs : EventArgs
    {
        internal ExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; private set; }

        /// <summary>
        /// 取得或設定此例外是否已處理。
        /// </summary>
        /// <value>
        /// true: 已處理；
        /// false: 未處理。
        /// </value>
        /// <remarks>
        /// ExceptionFilter 會依據此值來回傳不同的內容給 client；
        /// 已處理: 回傳 errorMessae 及 errorCode；
        /// 未處理: 回傳詳細的 exception 內容。
        /// </remarks>
        public bool Handled { internal get; set; }

        /// <summary>
        /// Gets or sets the return code.
        /// </summary>
        /// <value>
        /// The return code.
        /// </value>
        public string ReturnCode { internal get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { internal get; set; }
    }
}