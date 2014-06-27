using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using ApiFoundation.Configuration;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Web.Http;
using ApiFoundation.Web.Http.Filters;

namespace ApiFoundation.Web
{
    public class Global : HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {
            // 應用程式啟動時執行的程式碼

            var config = GlobalConfiguration.Configuration;

            // dump first-hand message
            config.MessageHandlers.Add(new ServerMessageDumper());

            this.CreateRoute(config);
            this.CreateEncryptedRoute(config);

            config.Filters.Add(new ModelStateFilter());
            config.Filters.Add(new CustomExceptionFilter());

            AreaRegistration.RegisterAllAreas();
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
            var cryptoHandler = new ServerCryptoHandler(settings.SecretKey, settings.InitialVector, settings.HashKey);
            var injection = new DelegatingHandler[] { new ServerMessageDumper(), cryptoHandler }; // dump decrypted request & plain response.
            var handler = HttpClientFactory.CreatePipeline(new HttpControllerDispatcher(configuration), injection);

            // register encrypted HTTP route.
            configuration.Routes.MapHttpRoute("Encrypted Route", "api2/{controller}/{action}", null, null, handler);

            // register timestamp as a route.
            var timestampProvider = new DefaultTimestampProvider(TimeSpan.FromMinutes(15)) as ITimestampProvider<string>;
            var timestampHandler = new HttpTimestampHandler<string>(timestampProvider);
            configuration.Routes.MapHttpRoute("Timestamp Route", "api3/!timestamp!/get", null, null, timestampHandler);

            // register global timestamp service, it should align with encrypted HTTP route or will not work.
            configuration.MessageHandlers.Add(new HttpTimestampHandler<string>(configuration, "api2/!timestamp!/get", timestampProvider));
        }
    }
}