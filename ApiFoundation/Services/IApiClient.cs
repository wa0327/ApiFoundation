using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;

namespace ApiFoundation.Services
{
    /// <summary>
    /// HTTP client service
    /// </summary>
    public interface IApiClient : IDisposable
    {
        /// <summary>
        /// Gets or sets the log writer.
        /// </summary>
        /// <value>
        /// The log writer.
        /// </value>
        ILogWriter LogWriter { get; set; }

        /// <summary>
        /// Gets or sets the media formatter.
        /// </summary>
        /// <value>
        /// The media formatter.
        /// </value>
        MediaTypeFormatter MediaFormatter { get; set; }

        /// <summary>
        /// Gets or sets the base address of Uniform Resource Identifier (URI) of the Internet resource used when sending requests.
        /// </summary>
        /// <value>
        /// Returns System.Uri.The base address of Uniform Resource Identifier (URI) of the Internet resource used when sending requests.
        /// </value>
        Uri BaseAddress { set; get; }

        /// <summary>
        /// Gets the headers which should be sent with each request.
        /// </summary>
        /// <value>
        /// Returns System.Net.Http.Headers.HttpRequestHeaders.The headers which should be sent with each request.
        /// </value>
        HttpRequestHeaders DefaultRequestHeaders { get; }

        /// <summary>
        /// Gets or sets the maximum number of bytes to buffer when reading the response content.
        /// </summary>
        /// <value>
        /// Returns the maximum number of bytes to buffer when reading the response content.
        /// </value>
        long MaxResponseContentBufferSize { set; get; }

        /// <summary>
        /// Gets or sets the number of milliseconds to wait before the request times out.
        /// </summary>
        /// <value>
        /// Returns System.TimeSpan.The number of milliseconds to wait before the request times out.
        /// </value>
        TimeSpan Timeout { set; get; }

        /// <summary>
        /// 送出 HTTP 要求
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        /// HTTP 回應
        /// </returns>
        /// <exception cref="ApiFoundation.Services.BadInvocationException">當呼叫端送出的格式有問題時擲出。</exception>
        /// <exception cref="ApiFoundation.Services.InvocationNotAcceptableException">當被呼叫端發生商業邏輯錯誤時擲出。</exception>
        /// <exception cref="ApiFoundation.Services.HttpServiceException">當被呼叫端發生非商業邏輯錯誤時擲出。</exception>
        TResponse Send<TRequest, TResponse>(HttpMethod method, string requestUri, TRequest request)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        /// Request 已送出
        /// </summary>
        event EventHandler<HttpRequestEventArgs> SendingRequest;

        /// <summary>
        /// Response 已接收
        /// </summary>
        event EventHandler<HttpResponseEventArgs> ResponseReceived;
    }
}