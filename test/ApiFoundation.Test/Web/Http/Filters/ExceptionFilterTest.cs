using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiFoundation.Services;
using ApiFoundation.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Web.Http.Filters
{
    [TestClass]
    public class ExceptionFilterTest
    {
        [TestMethod]
        public void ExceptionFilterTest_BusinessError()
        {
            using (var server = new ApiServerWrapper())
            {
                using (ApiClient client = new ApiClientWrapper())
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
                        client.Post<object, object>(
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
        }

        [TestMethod]
        public void ExceptionFilterTest_BusinessError_Encrypted()
        {
            using (var server = new EncryptedApiServerWrapper())
            {
                using (ApiClient client = new EncryptedApiClientWrapper())
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
                        client.Post<object, object>(
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
        }

        [TestMethod]
        public void ExceptionFilterTest_BusinessError_IIS()
        {
            using (ApiClient client = new ApiClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.Post<object, object>(
                        "/api1/ApiServerTest/BusinessErrorTest",
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
            using (ApiClient client = new EncryptedApiClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.Post<object, object>(
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
            using (var server = new ApiServerWrapper())
            {
                using (ApiClient client = new ApiClientWrapper())
                {
                    server.Configuration.Filters.Add(new ExceptionFilter());

                    try
                    {
                        client.Post<object, object>(
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
        }

        [TestMethod]
        public void ExceptionFilterTest_ProgramError_Encrypted()
        {
            using (var server = new EncryptedApiServerWrapper())
            {
                using (ApiClient client = new EncryptedApiClientWrapper())
                {
                    server.Configuration.Filters.Add(new ExceptionFilter());

                    try
                    {
                        client.Post<object, object>(
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
        }

        [TestMethod]
        public void ExceptionFilterTest_ProgramError_IIS()
        {
            using (ApiClient client = new ApiClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.Post<object, object>(
                        "/api1/ApiServerTest/ProgramErrorTest",
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
            using (ApiClient client = new EncryptedApiClientWrapper("http://apifoundation.self.monday:9999"))
            {
                try
                {
                    client.Post<object, object>(
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