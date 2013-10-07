using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Security.Cryptography
{
    internal sealed class ClientContentCryptoService : IHttpMessageCryptoService
    {
        private readonly HttpClient httpClient;
        private readonly string timestampUri;
        private readonly ICryptoService cryptoService;
        private readonly ITimestampProvider timestampProvider;

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

        internal ClientContentCryptoService(ICryptoService cryptoService, ITimestampProvider timestampProvider)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            this.cryptoService = cryptoService;
            this.timestampProvider = timestampProvider;
        }

        public HttpContent Encrypt(HttpContent plainContent)
        {
            if (plainContent == null)
            {
                return plainContent;
            }

            var plain = plainContent.ReadAsByteArrayAsync().Result;
            var timestamp = this.GetTimestamp();
            byte[] cipher;
            string signature;
            this.cryptoService.Encrypt(plain, timestamp, out cipher, out signature);

            var encrypted = new
            {
                Timestamp = timestamp,
                CipherText = Convert.ToBase64String(cipher),
                Signature = signature,
            };

            var encryptedContent = new ObjectContent(encrypted.GetType(), encrypted, new JsonMediaTypeFormatter());
            encryptedContent.Headers.ContentType = plainContent.Headers.ContentType; // keep source content type

            return encryptedContent;
        }

        public HttpContent Decrypt(HttpContent cipherContent)
        {
            if (cipherContent == null)
            {
                return cipherContent;
            }

            if (cipherContent.Headers.ContentLength == 0 || cipherContent.ReadAsByteArrayAsync().Result.Length == 0)
            {
                return cipherContent;
            }

            var message = cipherContent.ReadAsAsync<JObject>().Result;

            byte[] plain;
            try
            {
                var timestamp = (string)message["Timestamp"];
                var cipher = Convert.FromBase64String((string)message["CipherText"]);
                var signature = (string)message["Signature"];
                var expires = (DateTime)message["Expires"];

                this.cryptoService.Decrypt(cipher, timestamp, signature, out plain);

                this.timestamp = timestamp;
                this.timestampExpires = expires;
            }
            catch (Exception ex)
            {
                throw new InvalidHttpContentException(ex);
            }

            var plainContent = new ByteArrayContent(plain);
            plainContent.Headers.ContentType = cipherContent.Headers.ContentType;

            return plainContent;
        }

        private string GetTimestamp()
        {
            if (this.timestamp == null || DateTime.UtcNow >= this.timestampExpires)
            {
                if (this.httpClient != null)
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, this.timestampUri);
                    HttpResponseMessage responseMessage = null;
                    responseMessage = this.httpClient.SendAsync(requestMessage).Result;
                    var response = responseMessage.Content.ReadAsAsync<JObject>().Result;

                    this.timestamp = (string)response["Timestamp"];
                    this.timestampExpires = (DateTime)response["Expires"];
                }
                else if (this.timestampProvider != null)
                {
                    this.timestampProvider.GetTimestamp(out this.timestamp, out this.timestampExpires);
                }
            }

            return this.timestamp;
        }
    }
}