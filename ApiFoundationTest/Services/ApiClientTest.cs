using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using ApiFoundation.Utilities.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Services
{
    [TestClass]
    public class ApiClientTest
    {
        private IApiClient target;

        [TestInitialize]
        public void TestInitialize()
        {
            this.target = new ApiClient();

            target.SendingRequest += (sender, e) =>
            {
                if (e.Request.Content != null)
                {
                    var content = e.Request.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("Request: {0}", content);
                }
            };

            target.ResponseReceived += (sender, e) =>
            {
                if (e.Response.Content != null)
                {
                    var content = e.Response.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("Response: {0}", content);
                }
            };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.target.Dispose();
        }

        /// <summary>
        /// LMS API server 憑證有問題，所以應當收到 WebException 例外。
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void LmsApi_GetTimestamp_InvalidCertificate()
        {
            target.Post<object, string>(
                "https://lmsapi.eos.net.tw:8443/api/TimeStampService/GetTimeStamp",
                () => null,
                response => { }
            );
        }

        /// <summary>
        /// LMS API 的 GetTimeStamp 應該用 GET，所以應當收到 HttpServiceException 例外。
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(HttpServiceException))]
        public void LmsApi_GetTimestamp_InvalidMethod()
        {
            // 略過憑證檢查
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            target.Post<object, string>(
                "https://lmsapi.eos.net.tw:8443/api/TimeStampService/GetTimeStamp",
                () => null,
                response => { }
            );

            // 復原
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [TestMethod]
        public void LmsApi_GetTimestamp()
        {
            // 略過憑證檢查
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            target.Get<object, string>(
                "https://lmsapi.eos.net.tw:8443/api/TimeStampService/GetTimeStamp",
                () => null,
                response =>
                {
                    Assert.IsNotNull(response);
                    Assert.IsInstanceOfType(response, typeof(string));
                }
            );

            // 復原
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        private class SomeRequest
        {
            public string BackyardId { get; set; }

            public string Url { get; set; }
        }

        private class SomeResponse
        {
            public string BackyardId { get; set; }

            public string Url { get; set; }
        }

        [TestMethod]
        public void ErpApi_GetUserAuthority()
        {
            target.Post<SomeRequest, SomeResponse>(
                "http://erpapi.self.monday:9999/api/User/GetAuthority",
                () =>
                {
                    return new SomeRequest
                    {
                        BackyardId = "jacktsai",
                        Url = "/test.aspx",
                    };
                },
                response =>
                {
                    Assert.IsInstanceOfType(response, typeof(SomeResponse));
                    Assert.AreEqual("jacktsai", response.BackyardId);
                    Assert.AreEqual("/test.aspx", response.Url);
                }
            );
        }
    }
}