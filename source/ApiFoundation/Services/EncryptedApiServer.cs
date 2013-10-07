using System;
using System.Net.Http;
using System.Web.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Services
{
    public class EncryptedApiServer : ApiServer
    {
        private ICryptoService cryptoService;
        private ITimestampProvider timestampProvider;
        private IHttpMessageCryptoService contentCryptoService;

        public EncryptedApiServer(HttpConfiguration configuration, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler, ICryptoService cryptoService, ITimestampProvider timestampProvider)
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

            TimestampServiceHandler.Register(configuration, timestampProvider);

            this.cryptoService = cryptoService;
            this.timestampProvider = timestampProvider;
            this.contentCryptoService = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider);
        }

        public EncryptedApiServer(HttpConfiguration configuration, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler, string secretKeyPassword, string initialVectorPassword, string hashKeyString)
            : base(configuration, name, routeTemplate, defaults, constraints, handler)
        {
            this.cryptoService = new DefaultCryptoService(secretKeyPassword, initialVectorPassword, hashKeyString);
            this.timestampProvider = new DefaultTimestampProvider();

            TimestampServiceHandler.Register(configuration, this.timestampProvider);

            this.contentCryptoService = new DefaultHttpMessageCryptoService(cryptoService, this.timestampProvider);
        }

        public ICryptoService CryptoService
        {
            get { return this.cryptoService; }
        }

        public ITimestampProvider TimestampProvider
        {
            get { return this.timestampProvider; }
        }

        public event EventHandler<HttpRequestEventArgs> DecryptingRequest;

        public event EventHandler<HttpRequestEventArgs> RequestDecrypted;

        public event EventHandler<HttpResponseEventArgs> EncryptingResponse;

        public event EventHandler<HttpResponseEventArgs> ResponseEncrypted;

        protected override void OnRequestReceived(HttpRequestEventArgs e)
        {
            base.OnRequestReceived(e);

            this.OnDecryptingRequest(e);
            this.OnDecrypt(e);
            this.OnRequestDecrypted(e);
        }

        protected override void OnSendingResponse(HttpResponseEventArgs e)
        {
            this.OnEncryptingResponse(e);
            this.OnEncrypt(e);
            this.OnResponseEncrypted(e);

            base.OnSendingResponse(e);
        }

        protected virtual void OnDecryptingRequest(HttpRequestEventArgs e)
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

        protected virtual void OnEncryptingResponse(HttpResponseEventArgs e)
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
            try
            {
                e.RequestMessage = this.contentCryptoService.Decrypt(e.RequestMessage);
            }
            catch (Exception ex)
            {
                throw new BadMessageException(ex);
            }
        }

        protected virtual void OnEncrypt(HttpResponseEventArgs e)
        {
            e.ResponseMessage = this.contentCryptoService.Encrypt(e.ResponseMessage);
        }
    }
}