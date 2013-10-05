using System;
using System.Diagnostics;
using System.Web.Http.SelfHost;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Utility;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Services
{
    [TestClass]
    public class EncryptedServiceRouteTest
    {
        private EncryptedServiceRouteWrapper target;

        [TestInitialize]
        public void TestInitialize()
        {
            this.target = new EncryptedServiceRouteWrapper();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.target.Dispose();
        }

        [TestMethod]
        public void EncryptedServiceRoute_GetTimestamp()
        {
            using (ApiClient client = new ApiClientWrapper())
            {
                client.Get<JObject>(
                    "/!timestamp!/get",
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsNotNull(response["Timestamp"]);
                        Assert.IsNotNull(response["Expires"]);
                    }
                );
            }
        }

        [TestMethod]
        public void EncryptedServiceRoute_ApiFoundation_GetTimestamp()
        {
            using (ApiClient client = new ApiClientWrapper())
            {
                client.Get<JObject>(
                    "http://apifoundation.self.monday:9999/!timestamp!/get",
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsNotNull(response["Timestamp"]);
                        Assert.IsNotNull(response["Expires"]);
                    }
                );
            }
        }

        [TestMethod]
        public void EncryptedServiceRoute_RequestAndResponse()
        {
            using (EncryptedApiClient client = new EncryptedServiceClientWrapper())
            {
                client.Post<SmokeTestRequest, SmokeTestResponse>(
                    "/api/ApiServerTest/RequestAndResponse",
                    () =>
                    {
                        return new SmokeTestRequest
                        {
                            BoolValue = true,
                            ByteValue = byte.MaxValue,
                            IntValue = int.MaxValue,
                            StringValue = "string123中文abc",
                        };
                    },
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsInstanceOfType(response, typeof(SmokeTestResponse));
                        Assert.AreEqual(true, response.BoolValue);
                        Assert.AreEqual(byte.MaxValue, response.ByteValue);
                        Assert.AreEqual(int.MaxValue, response.IntValue);
                        Assert.AreEqual("string123中文abc", response.StringValue);
                    });
            }
        }

        [TestMethod]
        public void EncryptedServiceRoute_RequestOnly()
        {
            using (EncryptedApiClient client = new EncryptedServiceClientWrapper())
            {
                client.Post<SmokeTestRequest, object>(
                    "/api/ApiServerTest/RequestOnly",
                    () =>
                    {
                        return new SmokeTestRequest
                        {
                            BoolValue = true,
                            ByteValue = byte.MaxValue,
                            IntValue = int.MaxValue,
                            StringValue = "string123中文abc",
                        };
                    },
                    response =>
                    {
                        Assert.IsNull(response);
                    });
            }
        }

        [TestMethod]
        public void EncryptedServiceRoute_NullRequest()
        {
            using (EncryptedApiClient client = new EncryptedServiceClientWrapper())
            {
                client.Post<SmokeTestRequest, object>(
                    "/api/ApiServerTest/NullRequest",
                    () => null,
                    null);
            }
        }

        [TestMethod]
        public void EncryptedServiceRoute_ResponseOnly()
        {
            using (EncryptedApiClient client = new EncryptedServiceClientWrapper())
            {
                client.Post<object, SmokeTestResponse>(
                    "/api/ApiServerTest/ResponseOnly",
                    null,
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsInstanceOfType(response, typeof(SmokeTestResponse));
                        Assert.AreEqual(true, response.BoolValue);
                        Assert.AreEqual(byte.MaxValue, response.ByteValue);
                        Assert.AreEqual(int.MaxValue, response.IntValue);
                        Assert.AreEqual("string123中文abc", response.StringValue);
                    });
            }
        }

        [TestMethod]
        public void EncryptedServiceRoute_NullResponse()
        {
            using (EncryptedApiClient client = new EncryptedServiceClientWrapper())
            {
                client.Post<object, SmokeTestResponse>(
                    "/api/ApiServerTest/NullResponse",
                    null,
                    response => Assert.IsNull(response));
            }
        }

        [TestMethod]
        public void EncryptedServiceRoute_Action()
        {
            using (EncryptedApiClient client = new EncryptedServiceClientWrapper())
            {
                client.Post<object, object>("/api/ApiServerTest/Action", null, null);
            }
        }
    }
}