using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public interface IHttpContentEncryptor : IDisposable
    {
        /// <summary>
        /// Encrypt HTTP content.
        /// </summary>
        /// <param name="origin">Origin content.</param>
        /// <returns>Encrypted content.</returns>
        HttpContent Encrypt(HttpContent originContent);

        /// <summary>
        /// Decrypt HTTP content.
        /// </summary>
        /// <param name="encrypted">Encrypted content.</param>
        /// <returns>Origin content.</returns>
        HttpContent Decrypt(HttpContent encryptedContent);
    }
}