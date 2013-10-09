using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    /// <summary>
    /// HTTP message 加/解密服務。
    /// 屬於本組件核心，不對外開放擴充。
    /// </summary>
    internal interface IHttpMessageCryptoService : IDisposable
    {
        HttpRequestMessage Encrypt(HttpRequestMessage plainRequest);

        HttpRequestMessage Decrypt(HttpRequestMessage cipherRequest);

        HttpResponseMessage Encrypt(HttpResponseMessage plainResponse);

        HttpResponseMessage Decrypt(HttpResponseMessage cipherResponse);
    }
}