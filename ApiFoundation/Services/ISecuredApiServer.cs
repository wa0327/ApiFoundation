using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Services
{
    /// <summary>
    /// Secured HTTP server service
    /// </summary>
    public interface ISecuredApiServer : IApiServer
    {
        /// <summary>
        /// 時戳有效時間。
        /// </summary>
        TimeSpan TimestampTimeout { get; set; }

        /// <summary>
        /// Request 解密前
        /// </summary>
        event EventHandler<HttpContentEventArgs> RequestDecrypting;

        /// <summary>
        /// Request 解密後
        /// </summary>
        event EventHandler<HttpContentEventArgs> RequestDecrypted;

        /// <summary>
        /// Response 加密前
        /// </summary>
        event EventHandler<HttpContentEventArgs> ResponseEncrypting;

        /// <summary>
        /// Response 加密後
        /// </summary>
        event EventHandler<HttpContentEventArgs> ResponseEncrypted;
    }
}