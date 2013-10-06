using ApiFoundation.Services;
using ApiFoundation.Utility;
using ApiFoundation.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Web.Http.Filters
{
    [TestClass]
    public class ModelStateFilterTest
    {
        private ApiServerWrapper server;
        private ApiClient client;

        [TestInitialize]
        public void TestInitialize()
        {
            this.server = new ApiServerWrapper();
            this.client = new ApiClientWrapper();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.client.Dispose();
            this.server.Dispose();
        }

        [TestMethod]
        public void ModelStateFilterTest_InvalidModel()
        {
            this.server.Configuration.Filters.Add(new ModelStateFilter());

            try
            {
                this.client.Post<InvalidModelRequest, object>(
                    "/api/ApiServerTest/InvalidModelTest",
                    () =>
                    {
                        return new InvalidModelRequest
                        {
                            UserId = null,
                            BackyardId = null,
                            CatSubIds = null,
                        };
                    },
                    null
                );

                Assert.Fail("Did not throw expected exception BadInvocationException.");
            }
            catch (BadInvocationException ex)
            {
                Assert.IsNotNull(ex.ModelState);
                Assert.AreEqual("The UserId field is required.", ex.ModelState["request.UserId"][0]);
                Assert.AreEqual("The BackyardId field is required.", ex.ModelState["request.BackyardId"][0]);
                Assert.AreEqual("The CatSubIds field is required.", ex.ModelState["request.CatSubIds"][0]);
            }
        }
    }
}