using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using ApiFoundation.Configuration;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Web.Http;
using ApiFoundation.Web.Http.Filters;

namespace ApiFoundation.WebApp
{
    public class Global : System.Web.HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {
            // 應用程式啟動時執行的程式碼

            var config = GlobalConfiguration.Configuration;

            this.CreateRoute(config);
            this.CreateEncryptedRoute(config);

            config.Filters.Add(new ModelStateFilter());
            config.Filters.Add(new CustomExceptionFilter());

            // register global timestamp service.
            config.MessageHandlers.Add(new ServerMessageDumper());
            config.MessageHandlers.Add(new TimestampHandler(config, "!timestamp!/get", new DefaultTimestampProvider()));

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

        private void CreateRoute(HttpConfiguration configuration)
        {
            // register HTTP route.
            configuration.Routes.MapHttpRoute("API Default", "api/{controller}/{action}");
        }

        private void CreateEncryptedRoute(HttpConfiguration configuration)
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection("Api2CryptoGraphySettings");
            if (section == null)
            {
                throw new ApplicationException("Config section 'RestfulWebService' has not been set.");
            }

            // arrange.
            var settings = new CryptoGraphySettings(section);
            var timestampProvider = new DefaultTimestampProvider();
            var cryptoHandler = new ServerCryptoHandler(settings.SecretKey, settings.InitialVector, settings.HashKey, timestampProvider);
            var injection = new DelegatingHandler[] { cryptoHandler, new ServerMessageDumper() };
            var handler = HttpClientFactory.CreatePipeline(new HttpControllerDispatcher(configuration), injection);

            // register encrypted HTTP route.
            configuration.Routes.MapHttpRoute("Encrypted Route", "api2/{controller}/{action}", null, null, handler);

            // register timestamp service.
            var timestampHandler = new TimestampHandler(timestampProvider);
            configuration.Routes.MapHttpRoute("Timestamp Route", "api3/!timestamp!/get", null, null, timestampHandler);
        }
    }
}