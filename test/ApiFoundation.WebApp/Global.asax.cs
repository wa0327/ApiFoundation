﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Services;

namespace ApiFoundation.WebApp
{
    public class Global : System.Web.HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {
            // 應用程式啟動時執行的程式碼

            var configuration = GlobalConfiguration.Configuration;
            this.CreateServiceRoute(configuration);
            this.CreateEncryptedServiceRoute(configuration);

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

        private ApiServer CreateServiceRoute(HttpConfiguration configuration)
        {
            return new ApiServer(configuration, "API Default", "api/{controller}/{action}", null, null, null);
        }

        private ApiServer CreateEncryptedServiceRoute(HttpConfiguration configuration)
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

            return new EncryptedApiServer(
                configuration,
                "Encrypted Route",
                "api2/{controller}/{action}",
                null,
                null,
                null,
                new CryptoService(symmetricAlgorithm, hashAlgorithm),
                new DefaultTimestampProvider(TimeSpan.FromMinutes(15)));
        }
    }
}