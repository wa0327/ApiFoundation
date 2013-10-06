using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    internal interface IHttpContentCryptoService
    {
        HttpContent Encrypt(HttpContent origin);

        HttpContent Decrypt(HttpContent encrypted);
    }
}