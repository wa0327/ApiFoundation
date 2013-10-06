using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Services;
using ApiFoundation.Web.Http.Filters;

namespace ApiFoundation.WebApp
{
    public class Global : System.Web.HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {
            // 應用程式啟動時執行的程式碼

            var config = GlobalConfiguration.Configuration;

            this.CreatePlainRoute(config);
            this.CreateEncryptedRoute(config);

            config.Filters.Add(new ModelStateFilter());
            config.Filters.Add(new CustomExceptionFilter());

            AreaRegistration.RegisterAllAreas();
        }

        private void Application_End(object sender, EventArgs e)
        {
            //  應用程式關閉時執行的程式碼
        }

        private void Application_Error(object sender, EventArgs e)
        {
            // 發生未處理錯誤時執行的程式碼
        }

        private void Session_Start(object sender, EventArgs e)
        {
            // 啟動新工作階段時執行的程式碼
        }

        private void Session_End(object sender, EventArgs e)
        {
            // 工作階段結束時執行的程式碼。
            // 注意: 只有在 Web.config 檔將 sessionstate 模式設定為 InProc 時，
            // 才會引發 Session_End 事件。如果將工作階段模式設定為 StateServer
            // 或 SQLServer，就不會引發這個事件。
        }

        private ApiServer CreatePlainRoute(HttpConfiguration configuration)
        {
            var route = new ApiServer(configuration, "Plain Route", "api1/{controller}/{action}", null, null, null);

            route.RequestReceived += (sender, e) =>
            {
                Trace.TraceInformation("RECV from {0}", e.RequestMessage.Headers.From);

                if (e.RequestMessage.Content != null)
                {
                    var header = e.RequestMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };

            route.SendingResponse += (sender, e) =>
            {
                Trace.TraceInformation("SEND to {0}", e.ResponseMessage.RequestMessage.Headers.From);

                if (e.ResponseMessage.Content != null)
                {
                    var header = e.ResponseMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };

            return route;
        }

        private ApiServer CreateEncryptedRoute(HttpConfiguration configuration)
        {
            Func<byte[]> secretKeyCreator = () =>
            {
                var password = "123456789012345678901234";

                byte[] salt = new byte[32];
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 32))
                {
                    return deriveBytes.GetBytes(32);
                }
            };

            Func<byte[]> initialVectorCreator = () =>
            {
                var password = "12345678";

                byte[] salt = new byte[16];
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 16))
                {
                    return deriveBytes.GetBytes(16);
                }
            };

            var symmetricAlgorithm = new AesCryptoServiceProvider
            {
                Key = secretKeyCreator(),
                IV = initialVectorCreator(),
            };

            var hashAlgorithm = new HMACSHA512
            {
                Key = Encoding.UTF8.GetBytes("1234567890"),
            };

            var route = new EncryptedApiServer(
                configuration,
                "Encrypted Route",
                "api2/{controller}/{action}",
                null,
                null,
                null,
                new DefaultCryptoService(symmetricAlgorithm, hashAlgorithm),
                new DefaultTimestampService(TimeSpan.FromMinutes(15)));

            route.DecryptingRequest += (sender, e) =>
            {
                Trace.TraceInformation("DecryptingRequest");

                if (e.RequestMessage.Content != null)
                {
                    var header = e.RequestMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };

            route.RequestDecrypted += (sender, e) =>
            {
                Trace.TraceInformation("RequestDecrypted");

                if (e.RequestMessage.Content != null)
                {
                    var header = e.RequestMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };

            route.EncryptingResponse += (sender, e) =>
            {
                Trace.TraceInformation("EncryptingResponse");

                if (e.ResponseMessage.Content != null)
                {
                    var header = e.ResponseMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };

            route.ResponseEncrypted += (sender, e) =>
            {
                Trace.TraceInformation("ResponseEncrypted");

                if (e.ResponseMessage.Content != null)
                {
                    var header = e.ResponseMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };

            return route;
        }
    }
}