using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using ApiFoundation.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;

namespace ApiFoundation.Net.Http
{
    [TestClass]
    public class DefaultHttpMessageCryptoServiceTest
    {
        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_EncryptRequest_NullContent()
        {
            var input = new HttpRequestMessage
            {
                Content = null
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            timestampProvider
                .Stub(o => o.GetTimestamp())
                .Return(12345);

            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var actual = target.Encrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Content);

                var data = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(data);
                Assert.AreEqual(12345, data["Timestamp"]);
                Assert.AreEqual("0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==", data["CipherText"]);
                Assert.AreEqual("4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff", data["Signature"]);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptRequest_NullContent()
        {
            var encryptedContent = new
            {
                Timestamp = 12345,
                CipherText = "0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==",
                Signature = "4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff"
            };

            var input = new HttpRequestMessage
            {
                Content = new ObjectContent(encryptedContent.GetType(), encryptedContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            timestampProvider
                .Stub(o => o.Validate(Arg<long>.Is.Equal(12345)));

            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var actual = target.Decrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNull(actual.Content);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_EncryptResponse_NullContent()
        {
            var input = new HttpResponseMessage
            {
                Content = null
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var po = new PrivateObject(target);
                po.SetField("lastTimestamp", 12345);

                var actual = target.Encrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Content);

                var data = actual.Content.ReadAsAsync<JObject>().Result;
                Assert.IsNotNull(data);
                Assert.AreEqual(12345, data["Timestamp"]);
                Assert.AreEqual("0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==", data["CipherText"]);
                Assert.AreEqual("4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff", data["Signature"]);
            }
        }

        [TestMethod]
        public void DefaultHttpMessageCryptoServiceTest_DecryptResponse_NullContent()
        {
            var encryptedContent = new
            {
                Timestamp = 12345,
                CipherText = "0wz/LjHKyXy3zsWb2ny1IhVZ6Q3wmTb2i9G1tYig+RWthxb0lkFkadT/vnZur1t8NeSvGP3eVmS7Z/hm7Fi5WA==",
                Signature = "4033e2085d273ff20441cfc181805de8f5b6f0d4e24ef3eaae595cfaf4973c8353e72d9fdc9865239708683fde7fe70bc3fcef18eea8735e4058660a816181ff"
            };

            var input = new HttpResponseMessage
            {
                Content = new ObjectContent(encryptedContent.GetType(), encryptedContent, new JsonMediaTypeFormatter())
            };

            var timestampProvider = MockRepository.GenerateStub<ITimestampProvider<long>>();
            using (var cryptoService = new DefaultCryptoService("secretKeyPassword", "initialVectorPassword", "hashKeyString"))
            using (var target = new DefaultHttpMessageCryptoService(cryptoService, timestampProvider))
            {
                var po = new PrivateObject(target);
                po.SetField("lastTimestamp", 12345);

                var actual = target.Decrypt(input);
                Assert.IsNotNull(actual);
                Assert.IsNull(actual.Content);
            }
        }
    }
}