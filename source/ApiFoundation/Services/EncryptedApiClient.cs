using System;
using System.Net.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Services
{
    public class EncryptedApiClient : ApiClient
    {
        private const string TimestampService_Uri = "/!timestamp!/get";

        private readonly IHttpContentCryptoService contentCryptoService;

        public EncryptedApiClient(HttpClient messageInvoker, ICryptoService cryptoService)
            : base(messageInvoker)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            this.contentCryptoService = new ClientContentCryptoService(messageInvoker, TimestampServiceHandler.GetUri, cryptoService);
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

        protected virtual void OnEncrypt(HttpRequestEventArgs e)
        {
            e.RequestMessage.Content = this.contentCryptoService.Encrypt(e.RequestMessage.Content);
        }

        protected virtual void OnDecrypt(HttpResponseEventArgs e)
        {
            e.ResponseMessage.Content = this.contentCryptoService.Decrypt(e.ResponseMessage.Content);
        }
    }
}