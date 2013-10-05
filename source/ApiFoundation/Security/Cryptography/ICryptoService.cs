using System;

namespace ApiFoundation.Security.Cryptography
{
    /// <summary>
    /// The encryptor interface
    /// </summary>
    public interface ICryptoService : IDisposable
    {
        /// <summary>
        /// Encrypts the specified source.
        /// </summary>
        /// <param name="plain">The plain.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="cipher">The cipher.</param>
        /// <param name="signature">The signature.</param>
        void Encrypt(byte[] plain, string timestamp, out byte[] cipher, out string signature);

        /// <summary>
        /// Decrypts the specified cipher.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="plain">The plain.</param>
        /// <exception cref="ApiFoundation.Security.Cryptography.InvalidSignatureException">當簽章錯誤時擲出。</exception>
        void Decrypt(byte[] cipher, string timestamp, string signature, out byte[] plain);
    }
}