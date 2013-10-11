using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public sealed class InvalidSignatureException : Exception
    {
        public InvalidSignatureException()
            : base("The signature is not a legal hashed string.")
        {
        }
    }
}