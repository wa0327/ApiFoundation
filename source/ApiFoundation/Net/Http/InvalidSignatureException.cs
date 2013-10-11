using System;

namespace ApiFoundation.Net.Http
{
    public sealed class InvalidSignatureException : Exception
    {
        private readonly string signature;

        public InvalidSignatureException(string signature)
            : base(string.Format("Invalid signature '{0}'.", signature))
        {
            this.signature = signature;
        }

        public string Signature
        {
            get { return this.signature; }
        }
    }
}