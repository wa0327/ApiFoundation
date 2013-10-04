using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ApiFoundation.Services
{
    public class SecuredApiServer : ApiServer, ISecuredApiServer
    {
        public SecuredApiServer(HttpConfiguration configuration)
            : base(configuration)
        {
        }

        public TimeSpan TimestampTimeout { get; set; }

        public event EventHandler<HttpContentEventArgs> RequestDecrypting;

        public event EventHandler<HttpContentEventArgs> RequestDecrypted;

        public event EventHandler<HttpContentEventArgs> ResponseEncrypting;

        public event EventHandler<HttpContentEventArgs> ResponseEncrypted;

        protected override void OnRequestReceived(HttpRequestMessage request)
        {
            base.OnRequestReceived(request);
            this.OnRequestDecrypting(request.Content);

            // TODO: 執行解密

            this.OnRequestDecrypted(request.Content);
        }

        protected override void OnSendingResponse(HttpResponseMessage response)
        {
            this.OnResponseEncrypting(response.Content);

            // TODO: 執行加密

            this.OnResponseEncrypted(response.Content);
            base.OnSendingResponse(response);
        }

        protected virtual void OnRequestDecrypting(HttpContent content)
        {
            if (this.RequestDecrypting != null)
            {
                var e = new HttpContentEventArgs(content);
                this.RequestDecrypting(this, e);
            }
        }

        protected virtual void OnRequestDecrypted(HttpContent content)
        {
            if (this.RequestDecrypted != null)
            {
                var e = new HttpContentEventArgs(content);
                this.RequestDecrypted(this, e);
            }
        }

        protected virtual void OnResponseEncrypting(HttpContent content)
        {
            if (this.ResponseEncrypting != null)
            {
                var e = new HttpContentEventArgs(content);
                this.ResponseEncrypting(this, e);
            }
        }

        protected virtual void OnResponseEncrypted(HttpContent content)
        {
            if (this.ResponseEncrypted != null)
            {
                var e = new HttpContentEventArgs(content);
                this.ResponseEncrypted(this, e);
            }
        }
    }
}