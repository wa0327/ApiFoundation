using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using ApiFoundation.Net.Http;
using ApiFoundation.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    [TestClass]
    public class ApiClientTest
    {
        [TestMethod]
        public void ApiClient_NoAction()
        {
            using (var server = new ApiServerWrapper())
            using (ApiClient client = new ApiClientWrapper())
            {
                try
                {
                    client.Post<object, object>(
                        "/api/ApiServerTest/NoAction",
                        null,
                        null);

                    Assert.Fail("Did not throw expected exception HttpServiceException.");
                }
                catch (HttpServiceException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Message.StartsWith("No HTTP resource was found"));
                    Assert.IsTrue(ex.MessageDetail.StartsWith("No action was found"));
                }
            }
        }

        [TestMethod]
        public void ApiClient_NoController()
        {
            using (var server = new ApiServerWrapper())
            using (ApiClient client = new ApiClientWrapper())
            {
                try
                {
                    client.Post<object, object>(
                        "/api/NoController/NoAction",
                        null,
                        null);

                    Assert.Fail("Did not throw expected exception HttpServiceException.");
                }
                catch (HttpServiceException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Message.StartsWith("No HTTP resource was found"));
                    Assert.IsTrue(ex.MessageDetail.StartsWith("No type was found"));
                }
            }
        }

        [TestMethod]
        public void ApiClient_NoServer()
        {
            using (ApiClient target = new ApiClientWrapper())
            {
                try
                {
                    target.Post<object, object>(
                        "http://localhost:9876/api/AnyController/AnyAction",
                        null,
                        null);
                }
                catch (HttpRequestException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsNotNull(ex.InnerException);

                    if (ex.InnerException is WebException)
                    {
                        Assert.AreEqual("Unable to connect to the remote server", ex.InnerException.Message);
                        return;
                    }
                }
            }

            Assert.Fail("Did not throw expected exception HttpRequestException.");
        }

        /// <summary>
        /// LMS API server 憑證有問題，所以應當收到 WebException 例外。
        /// </summary>
        [TestMethod]
        public void ApiClient_LmsApi_InvalidCertificate()
        {
            using (ApiClient target = new ApiClientWrapper())
            {
                try
                {
                    target.Post<object, string>(
                        "https://lmsapi.eos.net.tw:8443/AnyController/AnyAction",
                        null,
                        null);
                }
                catch (HttpRequestException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsNotNull(ex.InnerException);

                    if (ex.InnerException is WebException)
                    {
                        var inner = ex.InnerException;
                        if (inner.InnerException is AuthenticationException)
                        {
                            return;
                        }
                    }
                }
            }

            Assert.Fail("Did not throw expected exception AuthenticationException.");
        }

        /// <summary>
        /// LMS API 的 GetTimeStamp 應該用 GET，所以應當收到 HttpServiceException 例外。
        /// </summary>
        [TestMethod]
        public void ApiClient_LmsApi_GetTimestamp_InvalidMethod()
        {
            // 略過憑證檢查
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            using (ApiClient target = new ApiClientWrapper())
            {
                try
                {
                    target.Post<object, string>(
                        "https://lmsapi.eos.net.tw:8443/api/TimeStampService/GetTimeStamp",
                        null,
                        null);

                    Assert.Fail("Did not throw expected exception HttpServiceException.");
                }
                catch (HttpServiceException ex)
                {
                    Assert.AreEqual("The requested resource does not support http method 'POST'.", ex.Message);
                }
            }

            // 復原
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [TestMethod]
        public void ApiClient_LmsApi_GetTimestamp()
        {
            // 略過憑證檢查
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            using (ApiClient target = new ApiClientWrapper())
            {
                target.Get<string>(
                    "https://lmsapi.eos.net.tw:8443/api/TimeStampService/GetTimeStamp",
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsInstanceOfType(response, typeof(string));
                    }
                );
            }

            // 復原
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [TestMethod]
        public void ApiClient_ErpApi_GetUserAuthority()
        {
            using (ApiClient target = new ApiClientWrapper())
            {
                target.Post<object, JObject>(
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
}