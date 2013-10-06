using System;
using System.Net.Http;
using System.Web.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Services
{
    public class EncryptedApiServer : ApiServer
    {
        private readonly IHttpContentCryptoService contentCryptoService;

        public EncryptedApiServer(HttpConfiguration configuration, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler, ICryptoService cryptoService, ITimestampService timestampService)
            : base(configuration, name, routeTemplate, defaults, constraints, handler)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            if (timestampService == null)
            {
                throw new ArgumentNullException("timestampService");
            }

            TimestampServiceHandler.Register(configuration, timestampService);

            this.contentCryptoService = new ServerContentCryptoService(cryptoService, timestampService);
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
            e.RequestMessage.Content = this.contentCryptoService.Decrypt(e.RequestMessage.Content);
        }

        protected virtual void OnEncrypt(HttpResponseEventArgs e)
        {
            e.ResponseMessage.Content = this.contentCryptoService.Encrypt(e.ResponseMessage.Content);
        }
    }
}