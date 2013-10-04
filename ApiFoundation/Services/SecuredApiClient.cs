using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Services
{
    public class SecuredApiClient : ApiClient, ISecuredApiClient
    {
        private readonly IEncryptor contentEncryptor;

        public SecuredApiClient()
        {
        }

        public event EventHandler<HttpContentEventArgs> RequestEncrypting;

        public event EventHandler<HttpContentEventArgs> RequestEncrypted;

        public event EventHandler<HttpContentEventArgs> ResponseDecrypting;

        public event EventHandler<HttpContentEventArgs> ResponseDecrypted;

        protected override void OnSendingRequest(HttpRequestMessage request)
        {
            this.OnRequestEncrypting(request.Content);

            // TODO: 執行加密

            this.OnRequestEncrypted(request.Content);
            base.OnSendingRequest(request);
        }

        protected override void OnResponseReceived(HttpResponseMessage response)
        {
            base.OnResponseReceived(response);
            this.OnResponseDecrypting(response.Content);

            // TODO: 執行解密

            this.OnResponseDecrypted(response.Content);
        }

        protected virtual void OnRequestEncrypting(HttpContent content)
        {
            if (this.RequestEncrypting != null)
            {
                var e = new HttpContentEventArgs(content);
                this.RequestEncrypting(this, e);
            }
        }

        protected virtual void OnRequestEncrypted(HttpContent content)
        {
            if (this.RequestEncrypted != null)
            {
                var e = new HttpContentEventArgs(content);
                this.RequestEncrypted(this, e);
            }
        }

        protected virtual void OnResponseDecrypting(HttpContent content)
        {
            if (this.ResponseDecrypting != null)
            {
                var e = new HttpContentEventArgs(content);
                this.ResponseDecrypting(this, e);
            }
        }

        protected virtual void OnResponseDecrypted(HttpContent content)
        {
            if (this.ResponseDecrypted != null)
            {
                var e = new HttpContentEventArgs(content);
                this.ResponseDecrypted(this, e);
            }
        }
    }
}