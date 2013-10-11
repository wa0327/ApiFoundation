using System;

namespace ApiFoundation.Net.Http
{
    public sealed class SignatureNullException : Exception
    {
        public SignatureNullException()
            : base("Signature is null.")
        {
        }
    }
}