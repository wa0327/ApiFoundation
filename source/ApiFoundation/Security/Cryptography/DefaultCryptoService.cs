using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ApiFoundation.Security.Cryptography
{
    public class DefaultCryptoService : ICryptoService
    {
        private readonly ISymmetricAlgorithm symmetricAlgorithm;
        private readonly IHashAlgorithm hashAlgorithm;

        public DefaultCryptoService(ISymmetricAlgorithm symmetricAlgorithm, IHashAlgorithm hashAlgorithm)
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

        public DefaultCryptoService(string secretKeyPassword, string initialVectorPassword, string hashKeyString)
        {
            if (secretKeyPassword == null)
            {
                throw new ArgumentNullException("secretKeyPassword");
            }

            if (initialVectorPassword == null)
            {
                throw new ArgumentNullException("initialVectorPassword");
            }

            if (hashKeyString == null)
            {
                throw new ArgumentNullException("hashKey");
            }

            this.symmetricAlgorithm = new AES(secretKeyPassword, initialVectorPassword);
            this.hashAlgorithm = new HMACSHA512(hashKeyString);
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

            cipher = this.symmetricAlgorithm.Encrypt(plain);
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

            plain = this.symmetricAlgorithm.Decrypt(cipher);
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