﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Services
{
    /// <summary>
    /// HTTP server service
    /// </summary>
    public interface IApiServer
    {
        /// <summary>
        /// Gets or sets the log writer.
        /// </summary>
        /// <value>
        /// The log writer.
        /// </value>
        ILogWriter LogWriter { get; set; }

        /// <summary>
        /// Gets or sets the exception handler.
        /// </summary>
        /// <value>
        /// The exception handler.
        /// </value>
        IExceptionHandler ExceptionHandler { get; set; }

        /// <summary>
        /// Request 已接收
        /// </summary>
        event EventHandler<HttpRequestEventArgs> RequestReceived;

        /// <summary>
        /// Response 已回傳
        /// </summary>
        event EventHandler<HttpResponseEventArgs> SendingResponse;
    }
}