using System;
using System.Net.Http;
using ApiFoundation.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Web.Http.Filters
{
    [TestClass]
    public class ExceptionFilterTest
    {
        [TestMethod]
        public void ExceptionFilterTest_BusinessError()
        {
            using (var server = new HttpServerWrapper())
            using (HttpClient client = new HttpClientWrapper())
            {
                var exceptionFilter = new ExceptionFilter();
                exceptionFilter.Exception += (sender, e) =>
                {
                    e.Handled = true;
                    e.ReturnCode = "7533967";
                    e.Message = "中毒太深";
                };
                server.Configuration.Filters.Add(exceptionFilter);

                try
                {
                    client.PostJson<object, object>(
                        "/api/ApiServerTest/BusinessErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception InvocationNotAcceptableException.");
                }
                catch (InvocationNotAcceptableException ex)
                {
                    Assert.AreEqual("中毒太深", ex.Message);
                    Assert.AreEqual("7533967", ex.ErrorCode);
                }
            }
        }

        [TestMethod]
        public void ExceptionFilterTest_BusinessError_Encrypted()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                var exceptionFilter = new ExceptionFilter();
                exceptionFilter.Exception += (sender, e) =>
                {
                    e.Handled = true;
                    e.ReturnCode = "7533967";
                    e.Message = "中毒太深";
                };
                server.Configuration.Filters.Add(exceptionFilter);

                try
                {
                    client.PostJson<object, object>(
                        "/api2/ApiServerTest/BusinessErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception InvocationNotAcceptableException.");
                }
                catch (InvocationNotAcceptableException ex)
                {
                    Assert.AreEqual("中毒太深", ex.Message);
                    Assert.AreEqual("7533967", ex.ErrorCode);
                }
            }
        }

        [TestMethod]
        public void ExceptionFilterTest_BusinessError_IIS()
        {
            using (HttpClient client = new HttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.PostJson<object, object>(
                        "/api/ApiServerTest/BusinessErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception InvocationNotAcceptableException.");
                }
                catch (InvocationNotAcceptableException ex)
                {
                    Assert.AreEqual("中毒太深", ex.Message);
                    Assert.AreEqual("7533967", ex.ErrorCode);
                }
            }
        }

        [TestMethod]
        public void ExceptionFilterTest_BusinessError_IIS_Encrypted()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.PostJson<object, object>(
                        "/api2/ApiServerTest/BusinessErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception InvocationNotAcceptableException.");
                }
                catch (InvocationNotAcceptableException ex)
                {
                    Assert.AreEqual("中毒太深", ex.Message);
                    Assert.AreEqual("7533967", ex.ErrorCode);
                }
            }
        }

        [TestMethod]
        public void ExceptionFilterTest_ProgramError()
        {
            using (var server = new HttpServerWrapper())
            using (HttpClient client = new HttpClientWrapper())
            {
                server.Configuration.Filters.Add(new ExceptionFilter());

                try
                {
                    client.PostJson<object, object>(
                        "/api/ApiServerTest/ProgramErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception HttpServiceException.");
                }
                catch (HttpServiceException ex)
                {
                    Assert.AreEqual("模擬非商業邏輯錯誤。", ex.ExceptionMessage);
                    Assert.AreEqual(typeof(InvalidOperationException).FullName, ex.ExceptionType);
                }
            }
        }

        [TestMethod]
        public void ExceptionFilterTest_ProgramError_Encrypted()
        {
            using (var server = new EncryptedHttpRouteWrapper())
            using (HttpClient client = new EncryptedHttpClientWrapper())
            {
                server.Configuration.Filters.Add(new ExceptionFilter());

                try
                {
                    client.PostJson<object, object>(
                        "/api2/ApiServerTest/ProgramErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception HttpServiceException.");
                }
                catch (HttpServiceException ex)
                {
                    Assert.AreEqual("模擬非商業邏輯錯誤。", ex.ExceptionMessage);
                    Assert.AreEqual(typeof(InvalidOperationException).FullName, ex.ExceptionType);
                }
            }
        }

        [TestMethod]
        public void ExceptionFilterTest_ProgramError_IIS()
        {
            using (HttpClient client = new HttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.PostJson<object, object>(
                        "/api/ApiServerTest/ProgramErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception HttpServiceException.");
                }

                catch (HttpServiceException ex)
                {
                    Assert.AreEqual("模擬非商業邏輯錯誤。", ex.ExceptionMessage);
                    Assert.AreEqual(typeof(InvalidOperationException).FullName, ex.ExceptionType);
                }
            }
        }

        [TestMethod]
        public void ExceptionFilterTest_ProgramError_IIS_Encrypted()
        {
            using (HttpClient client = new EncryptedHttpClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.PostJson<object, object>(
                        "/api2/ApiServerTest/ProgramErrorTest",
                        null,
                        null
                    );

                    Assert.Fail("Did not throw expected exception HttpServiceException.");
                }

                catch (HttpServiceException ex)
                {
                    Assert.AreEqual("模擬非商業邏輯錯誤。", ex.ExceptionMessage);
                    Assert.AreEqual(typeof(InvalidOperationException).FullName, ex.ExceptionType);
                }
            }
        }
    }
}