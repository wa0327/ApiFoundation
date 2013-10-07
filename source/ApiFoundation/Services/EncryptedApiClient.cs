using System;
using System.Net.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Services
{
    public class EncryptedApiClient : ApiClient
    {
        private readonly IHttpMessageCryptoService contentCryptoService;

        public EncryptedApiClient(HttpClient messageInvoker, ICryptoService cryptoService)
            : base(messageInvoker)
        {
            if (cryptoService == null)
            {
                throw new ArgumentNullException("cryptoService");
            }

            //this.contentCryptoService = new ClientContentCryptoService(messageInvoker, TimestampServiceHandler.GetUri, cryptoService);
        }

        public EncryptedApiClient(HttpClient messageInvoker, string secretKeyPassword, string initialVectorPassword, string hashKeyString)
            : base(messageInvoker)
        {
            var cryptoService = new DefaultCryptoService(secretKeyPassword, initialVectorPassword, hashKeyString);

            //this.contentCryptoService = new ClientContentCryptoService(messageInvoker, TimestampServiceHandler.GetUri, cryptoService);
        }

        public event EventHandler<HttpRequestEventArgs> EncryptingRequest;

        public event EventHandler<HttpRequestEventArgs> RequestEncrypted;

        public event EventHandler<HttpResponseEventArgs> DecryptingResponse;

        public event EventHandler<HttpResponseEventArgs> ResponseDecrypted;

        protected override void OnSendingRequest(HttpRequestEventArgs e)
        {
            base.OnSendingRequest(e);

            var requestMessage = e.RequestMessage;
            if (requestMessage.RequestUri.LocalPath != TimestampServiceHandler.GetUri)
            {
                this.OnEncryptingRequest(e);
                this.OnEncrypt(e);
                this.OnRequestEncrypted(e);
            }
        }

        protected override void OnResponseReceived(HttpResponseEventArgs e)
        {
            var responseMessage = e.ResponseMessage;
            var requestMessage = responseMessage.RequestMessage;
            if (requestMessage.RequestUri.LocalPath != TimestampServiceHandler.GetUri)
            {
                this.OnDecryptingResponse(e);
                this.OnDecrypt(e);
                this.OnResponseDecrypted(e);
            }

            base.OnResponseReceived(e);
        }

        protected virtual void OnEncryptingRequest(HttpRequestEventArgs e)
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

        protected virtual void OnDecryptingResponse(HttpResponseEventArgs e)
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
            //e.RequestMessage.Content = this.contentCryptoService.Encrypt(e.RequestMessage.Content);
        }

        protected virtual void OnDecrypt(HttpResponseEventArgs e)
        {
            try
            {
                //e.ResponseMessage.Content = this.contentCryptoService.Decrypt(e.ResponseMessage.Content);
            }
            catch (Exception ex)
            {
                throw new BadMessageException(ex);
            }
        }
    }
}