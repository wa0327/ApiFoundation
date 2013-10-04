using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Services
{
    /// <summary>
    /// Secured HTTP client service
    /// </summary>
    public interface ISecuredApiClient : IApiClient
    {
        /// <summary>
        /// Request 加密前
        /// </summary>
        event EventHandler<HttpContentEventArgs> RequestEncrypting;

        /// <summary>
        /// Request 加密後
        /// </summary>
        event EventHandler<HttpContentEventArgs> RequestEncrypted;

        /// <summary>
        /// Response 解密前
        /// </summary>
        event EventHandler<HttpContentEventArgs> ResponseDecrypting;

        /// <summary>
        /// Response 解密後
        /// </summary>
        event EventHandler<HttpContentEventArgs> ResponseDecrypted;
    }
}