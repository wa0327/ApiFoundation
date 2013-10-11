using System;

namespace ApiFoundation.Net.Http
{
    public sealed class InvalidCipherTextException : Exception
    {
        private readonly string cipherText;

        public InvalidCipherTextException(string cipherText)
            : base(string.Format("Invalid cipher text '{0}'.", cipherText))
        {
            this.cipherText = cipherText;
        }

        public InvalidCipherTextException(string cipherText, Exception innerException)
            : base(string.Format("Invalid cipher text '{0}'.", cipherText), innerException)
        {
            this.cipherText = cipherText;
        }

        public string CipherText
        {
            get { return this.cipherText; }
        }
    }
}