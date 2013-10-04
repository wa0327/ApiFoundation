using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    internal interface ICryptoProvider
    {
        string Encrypt(string content);

        string Decrypt(string content);
    }
}