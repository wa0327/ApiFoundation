using System.Net.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Web.Http
{
    [TestClass]
    public class EncryptedHttpRouteTest
    {
        [TestMethod]
        public void EncryptedHttpRouteTest_RequestAndResponse()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                client.PostJson<SmokeTestRequest, SmokeTestResponse>(
                    "/api2/ApiServerTest/RequestAndResponse",
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
        public void EncryptedHttpRouteTest_RequestOnly()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                client.PostJson<SmokeTestRequest, object>(
                    "/api2/ApiServerTest/RequestOnly",
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
        public void EncryptedHttpRouteTest_NullRequest()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                client.PostJson<SmokeTestRequest, object>(
                    "/api2/ApiServerTest/NullRequest",
                    () => null,
                    null);
            }
        }

        [TestMethod]
        public void EncryptedHttpRouteTest_ResponseOnly()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                client.PostJson<object, SmokeTestResponse>(
                    "/api2/ApiServerTest/ResponseOnly",
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
        public void EncryptedHttpRouteTest_NullResponse()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                client.PostJson<object, SmokeTestResponse>(
                    "/api2/ApiServerTest/NullResponse",
                    null,
                    response => Assert.IsNull(response));
            }
        }

        [TestMethod]
        public void EncryptedHttpRouteTest_ActionOnly()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                client.PostJson<object, object>("/api2/ApiServerTest/ActionOnly", null, null);
            }
        }
    }
}