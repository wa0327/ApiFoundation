using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Net.Http
{
    [TestClass]
    public class EncryptedHttpClientHandlerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (HttpClient httpClient = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                httpClient.PostJson<SmokeTestRequest, SmokeTestResponse>(
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
    }
}