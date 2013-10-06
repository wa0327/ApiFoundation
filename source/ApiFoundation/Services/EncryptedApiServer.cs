using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Routing;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    public class EncryptedApiServer : ApiServer
    {
        private readonly ICryptoService cryptoService;
        private readonly ITimestampService timestampProvider;

        public EncryptedApiServer(HttpConfiguration configuration, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler, ICryptoService cryptoService, ITimestampService timestampProvider)
            : base(configuration, name, routeTemplate, defaults, constraints, handler)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            IHttpRoute route;
            if (configuration.Routes.TryGetValue("TimestampService", out route))
            {
                var timestampService = (TimestampServiceHandler)route.Handler;
                timestampService.TimestampProvider = timestampProvider;
            }
            else
            {
                var timestampService = new TimestampServiceHandler
                {
                    TimestampProvider = timestampProvider,
                };

                configuration.Routes.MapHttpRoute("TimestampService", "!timestamp!/{action}", null, null, timestampService);
            }

            this.cryptoService = cryptoService;
            this.timestampProvider = timestampProvider;
        }

        public event EventHandler<HttpRequestEventArgs> DecryptingRequest;

        public event EventHandler<HttpRequestEventArgs> RequestDecrypted;

        public event EventHandler<HttpResponseEventArgs> EncryptingResponse;

        public event EventHandler<HttpResponseEventArgs> ResponseEncrypted;

        protected override void OnRequestReceived(HttpRequestEventArgs e)
        {
            base.OnRequestReceived(e);

            this.OnRequestDecrypting(e);
            this.OnDecrypt(e);
            this.OnRequestDecrypted(e);
        }

        protected override void OnSendingResponse(HttpResponseEventArgs e)
        {
            this.OnResponseEncrypting(e);
            this.OnEncrypt(e);
            this.OnResponseEncrypted(e);

            base.OnSendingResponse(e);
        }

        protected virtual void OnRequestDecrypting(HttpRequestEventArgs e)
        {
            if (this.DecryptingRequest != null)
            {
                this.DecryptingRequest(this, e);
            }
        }

        protected virtual void OnRequestDecrypted(HttpRequestEventArgs e)
        {
            if (this.RequestDecrypted != null)
            {
                this.RequestDecrypted(this, e);
            }
        }

        protected virtual void OnResponseEncrypting(HttpResponseEventArgs e)
        {
            if (this.EncryptingResponse != null)
            {
                this.EncryptingResponse(this, e);
            }
        }

        protected virtual void OnResponseEncrypted(HttpResponseEventArgs e)
        {
            if (this.ResponseEncrypted != null)
            {
                this.ResponseEncrypted(this, e);
            }
        }

        protected virtual void OnDecrypt(HttpRequestEventArgs e)
        {
            var origin = e.RequestMessage.Content;

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

                this.timestampProvider.Validate(timestamp);
                this.cryptoService.Decrypt(cipher, timestamp, signature, out plain);
            }
            catch
            {
                throw new BadMessageException();
            }

            e.RequestMessage.Content = new ByteArrayContent(plain);
            e.RequestMessage.Content.Headers.ContentType = origin.Headers.ContentType;
        }

        protected virtual void OnEncrypt(HttpResponseEventArgs e)
        {
            var origin = e.ResponseMessage.Content;

            if (origin == null)
            {
                return;
            }

            string timestamp;
            DateTime expires;
            this.timestampProvider.GetTimestamp(out timestamp, out expires);

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

            e.ResponseMessage.Content = new ObjectContent(encryptedMessage.GetType(), encryptedMessage, new JsonMediaTypeFormatter());
            e.ResponseMessage.Content.Headers.ContentType = origin.Headers.ContentType; // keep source content type
        }
    }
}