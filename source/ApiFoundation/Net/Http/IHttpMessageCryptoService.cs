using System;
using System.Net.Http;

namespace ApiFoundation.Net.Http
{
    /// <summary>
    /// HTTP message 加/解密服務。
    /// 屬於本組件核心，不對外開放擴充。
    /// </summary>
    public interface IHttpMessageCryptoService : IDisposable
    {
        /// <summary>
        /// Encrypts the specified plain request.
        /// </summary>
        /// <param name="plainRequest">The plain request.</param>
        /// <returns>The cipher request.</returns>
        HttpRequestMessage Encrypt(HttpRequestMessage plainRequest);

        /// <summary>
        /// Decrypts the specified cipher request.
        /// </summary>
        /// <param name="cipherRequest">The cipher request.</param>
        /// <returns>The plain request.</returns>
        HttpRequestMessage Decrypt(HttpRequestMessage cipherRequest);

        /// <summary>
        /// Encrypts the specified plain response.
        /// </summary>
        /// <param name="plainResponse">The plain response.</param>
        /// <returns>The cipher response.</returns>
        HttpResponseMessage Encrypt(HttpResponseMessage plainResponse);

        /// <summary>
        /// Decrypts the specified cipher response.
        /// </summary>
        /// <param name="cipherResponse">The cipher response.</param>
        /// <returns>The plain response.</returns>
        HttpResponseMessage Decrypt(HttpResponseMessage cipherResponse);
    }
}