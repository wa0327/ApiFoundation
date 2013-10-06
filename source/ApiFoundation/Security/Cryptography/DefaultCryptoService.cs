using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public class DefaultCryptoService : ICryptoService
    {
        private readonly SymmetricAlgorithm symmetricAlgorithm;
        private readonly HashAlgorithm hashAlgorithm;

        public DefaultCryptoService(SymmetricAlgorithm symmetricAlgorithm, HashAlgorithm hashAlgorithm)
        {
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }

            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            this.symmetricAlgorithm = symmetricAlgorithm;
            this.hashAlgorithm = hashAlgorithm;
        }

        public void Dispose()
        {
            this.symmetricAlgorithm.Dispose();
            this.hashAlgorithm.Dispose();
        }

        public void Encrypt(byte[] plain, string timestamp, out byte[] cipher, out string signature)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            if (timestamp == null)
            {
                throw new ArgumentNullException("timestamp");
            }

            var encryptor = this.symmetricAlgorithm.CreateEncryptor();
            cipher = this.Transform(plain, encryptor);
            signature = this.ComputeSignature(cipher, timestamp);
        }

        public void Decrypt(byte[] cipher, string timestamp, string signature, out byte[] plain)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            if (timestamp == null)
            {
                throw new ArgumentNullException("timestamp");
            }

            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }

            var signature2 = this.ComputeSignature(cipher, timestamp);
            if (signature != signature2)
            {
                throw new InvalidSignatureException();
            }

            var decryptor = this.symmetricAlgorithm.CreateDecryptor();
            plain = this.Transform(cipher, decryptor);
        }

        private byte[] Transform(byte[] input, ICryptoTransform transform)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (transform == null)
            {
                throw new ArgumentNullException("transform");
            }

            using (var outputBuffer = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(outputBuffer, transform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(input, 0, input.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return outputBuffer.ToArray();
            }
        }

        private string ComputeSignature(byte[] cipher, string timestamp)
        {
            var hashBase = Convert.ToBase64String(cipher) + timestamp;
            var hashBaseBytes = Encoding.UTF8.GetBytes(hashBase);
            var hash = this.hashAlgorithm.ComputeHash(hashBaseBytes);

            return string.Join(string.Empty, hash.Select(o => o.ToString("x2")));
        }
    }
}