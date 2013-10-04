using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Services
{
    public sealed class ExceptionEventArgs : EventArgs
    {
        internal ExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; private set; }

        /// <summary>
        /// 取得或設定此例外是否為商業邏輯錯誤。
        /// </summary>
        /// <value>
        /// true: 是商業邏輯錯誤，應回傳 errorCode 及 errorMessae；
        /// false: 非商業邏輯錯誤。
        /// </value>
        public bool IsBusinessError { internal get; set; }

        /// <summary>
        /// Sets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public string ErrorCode { internal get; set; }

        /// <summary>
        /// Sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { internal get; set; }
    }
}