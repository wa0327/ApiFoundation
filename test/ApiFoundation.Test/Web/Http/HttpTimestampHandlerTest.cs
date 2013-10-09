using System.Net.Http;
using System.Web.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Web.Http
{
    [TestClass]
    public class HttpTimestampHandlerTest
    {
        [TestMethod]
        public void HttpTimestampHandlerTest_RouteHandler()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new HttpClientWrapper())
            {
                client.GetJson<JObject>(
                    "/api3/!timestamp!/get",
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsNotNull(response["Timestamp"]);
                    }
                );
            }
        }

        [TestMethod]
        public void HttpTimestampHandlerTest_GlobalHandler()
        {
            using (var server = new EncryptedHttpServerWrapper())
            using (HttpClient client = new HttpClientWrapper())
            {
                client.GetJson<JObject>(
                    "/api2/!timestamp!/get",
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsNotNull(response["Timestamp"]);
                    }
                );
            }
        }

        [TestMethod]
        public void HttpTimestampHandlerTest_RouteHandler_IIS()
        {
            using (HttpClient client = new HttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                client.GetJson<JObject>(
                    "/api3/!timestamp!/get",
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsNotNull(response["Timestamp"]);
                    }
                );
            }
        }

        [TestMethod]
        public void HttpTimestampHandlerTest_GlobalHandler_IIS()
        {
            using (HttpClient client = new HttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                client.GetJson<JObject>(
                    "/api2/!timestamp!/get",
                    response =>
                    {
                        Assert.IsNotNull(response);
                        Assert.IsNotNull(response["Timestamp"]);
                    }
                );
            }
        }
    }
}