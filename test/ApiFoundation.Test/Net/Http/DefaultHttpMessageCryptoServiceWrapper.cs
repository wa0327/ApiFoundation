using System;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Net.Http
{
    internal sealed class DefaultHttpMessageCryptoServiceWrapper : IDisposable
    {
        private readonly DefaultHttpMessageCryptoService inner;

        internal DefaultHttpMessageCryptoServiceWrapper(ITimestampProvider<string> timestampProvider)
        {
            var symmetricAlgorithm = new AES("secretKeyPassword", "initialVectorPassword");
            var hashAlgorithm = new HMACSHA512("hashKeyString");

            this.inner = new DefaultHttpMessageCryptoService(symmetricAlgorithm, hashAlgorithm, timestampProvider);
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        public static implicit operator DefaultHttpMessageCryptoService(DefaultHttpMessageCryptoServiceWrapper source)
        {
            return source.inner;
        }
    }
}