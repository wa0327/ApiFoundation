using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using ApiFoundation.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    [TestClass]
    public class ApiClientTest
    {
        private ApiClient target;

        [TestInitialize]
        public void TestInitialize()
        {
            this.target = new ApiClientWrapper();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.target.Dispose();
        }

        [TestMethod]
        public void ApiClientTest_NoServer()
        {
            try
            {
                this.target.Post<object, object>(
                    "http://localhost:9876/api/AnyController/AnyAction",
                    null,
                    null);

                Assert.Fail("Did not throw expected exception HttpRequestException.");
            }
            catch (HttpRequestException ex)
            {
                Assert.IsNotNull(ex.InnerException);
                Assert.IsInstanceOfType(ex.InnerException, typeof(WebException));
            }
        }

        /// <summary>
        /// LMS API server 憑證有問題，所以應當收到 WebException 例外。
        /// </summary>
        [TestMethod]
        public void ApiClientTest_LmsApi_InvalidCertificate()
        {
            try
            {
                this.target.Post<object, string>(
                    "https://lmsapi.eos.net.tw:8443/AnyController/AnyAction",
                    null,
                    null);
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException)
                {
                    var inner = ex.InnerException;
                    if (inner.InnerException is AuthenticationException)
                    {
                        return;
                    }
                }
            }

            Assert.Fail("Did not throw expected exception AuthenticationException.");
        }

        /// <summary>
        /// LMS API 的 GetTimeStamp 應該用 GET，所以應當收到 HttpServiceException 例外。
        /// </summary>
        [TestMethod]
        public void ApiClientTest_LmsApi_GetTimestamp_InvalidMethod()
        {
            // 略過憑證檢查
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                this.target.Post<object, string>(
                    "https://lmsapi.eos.net.tw:8443/api/TimeStampService/GetTimeStamp",
                    null,
                    null);

                Assert.Fail("Did not throw expected exception HttpServiceException.");
            }
            catch (HttpServiceException ex)
            {
                Assert.AreEqual("The requested resource does not support http method 'POST'.", ex.Message);
            }

            // 復原
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [TestMethod]
        public void ApiClientTest_LmsApi_GetTimestamp()
        {
            // 略過憑證檢查
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            this.target.Get<string>(
                "https://lmsapi.eos.net.tw:8443/api/TimeStampService/GetTimeStamp",
                response =>
                {
                    Assert.IsNotNull(response);
                    Assert.IsInstanceOfType(response, typeof(string));
                }
            );

            // 復原
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [TestMethod]
        public void ApiClientTest_ErpApi_GetUserAuthority()
        {
            this.target.Post<object, JObject>(
                "http://erpapi.self.monday:9999/api/User/GetAuthority",
                () =>
                {
                    return new
                    {
                        BackyardId = "jacktsai",
                        Url = "/test.aspx",
                    };
                },
                response =>
                {
                    Assert.IsInstanceOfType(response, typeof(JObject));
                    Assert.AreEqual("jacktsai", response["BackyardId"]);
                    Assert.AreEqual("/test.aspx", response["Url"]);
                }
            );
        }
    }
}