using System.Net.Http;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Net.Http
{
    [TestClass]
    public class EncryptedHttpClientTest
    {
        [TestMethod]
        public void EncryptedHttpClientTest_RequestAndResponse()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
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
        public void EncryptedHttpServerTest_RequestOnly()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
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
        public void EncryptedHttpServerTest_NullRequest()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                client.PostJson<SmokeTestRequest, object>(
                    "/api2/ApiServerTest/NullRequest",
                    () => null,
                    null);
            }
        }

        [TestMethod]
        public void EncryptedHttpServerTest_ResponseOnly()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
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
        public void EncryptedHttpServerTest_NullResponse()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                client.PostJson<object, SmokeTestResponse>(
                    "/api2/ApiServerTest/NullResponse",
                    null,
                    response => Assert.IsNull(response));
            }
        }

        [TestMethod]
        public void EncryptedHttpServerTest_ActionOnly()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                client.PostJson<object, object>("/api2/ApiServerTest/ActionOnly", null, null);
            }
        }
    }
}