using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    public class EncryptedApiClient : ApiClient
    {
        private const string TimestampService_Uri = "/!timestamp!/get";

        private readonly ICryptoService cryptoService;

        private string timestamp;
        private DateTime timestampExpires;

        public EncryptedApiClient(HttpClient messageInvoker, ICryptoService cryptoService)
            : base(messageInvoker)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            this.cryptoService = cryptoService;
        }

        public event EventHandler<HttpRequestEventArgs> EncryptingRequest;

        public event EventHandler<HttpRequestEventArgs> RequestEncrypted;

        public event EventHandler<HttpResponseEventArgs> DecryptingResponse;

        public event EventHandler<HttpResponseEventArgs> ResponseDecrypted;

        protected override void OnSendingRequest(HttpRequestEventArgs e)
        {
            base.OnSendingRequest(e);

            var requestMessage = e.RequestMessage;
            if (requestMessage.RequestUri.LocalPath != TimestampService_Uri)
            {
                this.OnRequestEncrypting(e);
                this.OnEncrypt(e);
                this.OnRequestEncrypted(e);
            }
        }

        protected override void OnResponseReceived(HttpResponseEventArgs e)
        {
            var responseMessage = e.ResponseMessage;
            var requestMessage = responseMessage.RequestMessage;
            if (requestMessage.RequestUri.LocalPath != TimestampService_Uri)
            {
                this.OnResponseDecrypting(e);
                this.OnDecrypt(e);
                this.OnResponseDecrypted(e);
            }

            base.OnResponseReceived(e);
        }

        protected virtual void OnRequestEncrypting(HttpRequestEventArgs e)
        {
            if (this.EncryptingRequest != null)
            {
                this.EncryptingRequest(this, e);
            }
        }

        protected virtual void OnRequestEncrypted(HttpRequestEventArgs e)
        {
            if (this.RequestEncrypted != null)
            {
                this.RequestEncrypted(this, e);
            }
        }

        protected virtual void OnResponseDecrypting(HttpResponseEventArgs e)
        {
            if (this.DecryptingResponse != null)
            {
                this.DecryptingResponse(this, e);
            }
        }

        protected virtual void OnResponseDecrypted(HttpResponseEventArgs e)
        {
            if (this.ResponseDecrypted != null)
            {
                this.ResponseDecrypted(this, e);
            }
        }

        /// <summary>
        /// Encrypts the specified content.
        /// </summary>
        /// <param name="content">The source content.</param>
        /// <returns>The encrypted content.</returns>
        protected virtual void OnEncrypt(HttpRequestEventArgs e)
        {
            var origin = e.RequestMessage.Content;

            if (origin == null)
            {
                return;
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

            e.RequestMessage.Content = new ObjectContent(encryptedMessage.GetType(), encryptedMessage, new JsonMediaTypeFormatter());
            e.RequestMessage.Content.Headers.ContentType = origin.Headers.ContentType; // keep source content type
        }

        /// <summary>
        /// Decrypts the specified content.
        /// </summary>
        /// <param name="content">The source content.</param>
        /// <returns>The decrypted content.</returns>
        protected virtual void OnDecrypt(HttpResponseEventArgs e)
        {
            var origin = e.ResponseMessage.Content;

            if (origin == null)
            {
                return;
            }

            if (origin.Headers.ContentLength == 0 || origin.ReadAsByteArrayAsync().Result.Length == 0)
            {
                return;
            }

            var encryptedMessage = origin.ReadAsAsync<JObject>().Result;

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

            e.ResponseMessage.Content = new ByteArrayContent(plain);
            e.ResponseMessage.Content.Headers.ContentType = origin.Headers.ContentType;
        }

        protected virtual string GetTimestamp()
        {
            if (this.timestamp == null || DateTime.UtcNow >= this.timestampExpires)
            {
                this.Get<JObject>(
                    "/!timestamp!/get",
                    response =>
                    {
                        this.timestamp = (string)response["Timestamp"];
                        this.timestampExpires = (DateTime)response["Expires"];
                    });
            }

            return this.timestamp;
        }
    }
}