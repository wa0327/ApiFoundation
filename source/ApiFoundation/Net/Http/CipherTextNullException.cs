using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Net.Http
{
    public sealed class CipherTextNullException : Exception
    {
        public CipherTextNullException()
            : base("Cipher text is null.")
        {
        }
    }
}