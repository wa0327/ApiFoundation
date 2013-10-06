using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Security.Cryptography
{
    internal sealed class ServerContentCryptoService : IHttpContentCryptoService
    {
        private readonly ICryptoService cryptoService;
        private readonly ITimestampService timestampService;

        internal ServerContentCryptoService(ICryptoService cryptoService, ITimestampService timestampService)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            if (timestampService == null)
            {
                throw new ArgumentNullException("timestampService");
            }

            this.cryptoService = cryptoService;
            this.timestampService = timestampService;
        }

        public HttpContent Decrypt(HttpContent encrypted)
        {
            if (encrypted == null)
            {
                return encrypted;
            }

            if (encrypted.Headers.ContentLength == 0 || encrypted.ReadAsByteArrayAsync().Result.Length == 0)
            {
                return encrypted;
            }

            var encryptedMessage = encrypted.ReadAsAsync<JObject>().Result;

            byte[] plain;
            try
            {
                var timestamp = (string)encryptedMessage["Timestamp"];
                var cipher = Convert.FromBase64String((string)encryptedMessage["CipherText"]);
                var signature = (string)encryptedMessage["Signature"];

                this.timestampService.Validate(timestamp);
                this.cryptoService.Decrypt(cipher, timestamp, signature, out plain);
            }
            catch
            {
                throw new BadMessageException();
            }

            var origin = new ByteArrayContent(plain);
            origin.Headers.ContentType = encrypted.Headers.ContentType;

            return origin;
        }

        public HttpContent Encrypt(HttpContent origin)
        {
            if (origin == null)
            {
                return origin;
            }

            string timestamp;
            DateTime expires;
            this.timestampService.GetTimestamp(out timestamp, out expires);

            var plain = origin.ReadAsByteArrayAsync().Result;
            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp, out cipher, out signature);

            var encryptedMessage = new
            {
                Timestamp = timestamp,
                CipherText = Convert.ToBase64String(cipher),
                Signature = signature,
                Expires = expires,
            };

            var encrypted = new ObjectContent(encryptedMessage.GetType(), encryptedMessage, new JsonMediaTypeFormatter());
            encrypted.Headers.ContentType = origin.Headers.ContentType;

            return encrypted;
        }
    }
}