using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Security.Cryptography
{
    internal sealed class ClientContentCryptoService : IHttpContentCryptoService
    {
        private readonly HttpClient httpClient;
        private readonly string timestampUri;
        private readonly ICryptoService cryptoService;

        private string timestamp;
        private DateTime timestampExpires;

        internal ClientContentCryptoService(HttpClient httpClient, string timestampUri, ICryptoService cryptoService)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (timestampUri == null)
            {
                throw new ArgumentNullException("timestampUri");
            }

            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            this.httpClient = httpClient;
            this.timestampUri = timestampUri;
            this.cryptoService = cryptoService;
        }

        public HttpContent Encrypt(HttpContent origin)
        {
            if (origin == null)
            {
                return origin;
            }

            var plain = origin.ReadAsByteArrayAsync().Result;
            var timestamp = this.GetTimestamp();

            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp, out cipher, out signature);

            var encryptedMessage = new
            {
                Timestamp = timestamp,
                CipherText = Convert.ToBase64String(cipher),
                Signature = signature,
            };

            var encrypted = new ObjectContent(encryptedMessage.GetType(), encryptedMessage, new JsonMediaTypeFormatter());
            encrypted.Headers.ContentType = origin.Headers.ContentType; // keep source content type

            return encrypted;
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
                var expires = (DateTime)encryptedMessage["Expires"];

                this.cryptoService.Decrypt(cipher, timestamp, signature, out plain);

                this.timestamp = timestamp;
                this.timestampExpires = expires;
            }
            catch
            {
                throw new BadMessageException();
            }

            var origin = new ByteArrayContent(plain);
            origin.Headers.ContentType = encrypted.Headers.ContentType;

            return origin;
        }

        private string GetTimestamp()
        {
            if (this.timestamp == null || DateTime.UtcNow >= this.timestampExpires)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, this.timestampUri);
                var responseMessage = this.httpClient.SendAsync(requestMessage).Result;
                var response = responseMessage.Content.ReadAsAsync<JObject>().Result;

                this.timestamp = (string)response["Timestamp"];
                this.timestampExpires = (DateTime)response["Expires"];
            }

            return this.timestamp;
        }
    }
}