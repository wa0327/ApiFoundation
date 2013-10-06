using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ApiFoundation.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Security.Cryptography
{
    [TestClass]
    public class CryptoServiceTest
    {
        private DefaultCryptoService target;

        [TestInitialize]
        public void TestInitialize()
        {
            this.target = new CryptoServiceWrapper();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.target.Dispose();
        }

        [TestMethod]
        public void CryptoServiceTest_Encrypt()
        {
            var timestamp = "201310041855";
            var plainText = "`1234567890-=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz~!@#$%^&*()_+";
            var plain = Encoding.UTF8.GetBytes(plainText);

            byte[] cipher;
            string signature;
            this.target.Encrypt(plain, timestamp, out cipher, out signature);

            var cipherText = Convert.ToBase64String(cipher);

            Assert.AreEqual(
                "xRANotL/km4vWpPwPw6P1H8T2UXptwMGyFShVH/BmXsnv6E+pOTtfeOPfsbfap5TD1DfMTdOn94t1j/sk9NU984ESJn1wi2QEDmgR/SI3B4=",
                cipherText);
            Assert.AreEqual(
                "f24d46a550e50a3e309e3b6aa27f12d9316af9db264c1986ebd9259c62c906f6b1bc7e8b2e3b15974a9c34c13e511cd5e03d24670799026aeb884f8d23746111",
                signature);
        }

        [TestMethod]
        public void CryptoServiceTest_Decrypt()
        {
            var timestamp = "201310041855";
            var cipherText = "xRANotL/km4vWpPwPw6P1H8T2UXptwMGyFShVH/BmXsnv6E+pOTtfeOPfsbfap5TD1DfMTdOn94t1j/sk9NU984ESJn1wi2QEDmgR/SI3B4=";
            string signature = "f24d46a550e50a3e309e3b6aa27f12d9316af9db264c1986ebd9259c62c906f6b1bc7e8b2e3b15974a9c34c13e511cd5e03d24670799026aeb884f8d23746111";
            var cipher = Convert.FromBase64String(cipherText);

            byte[] plain;
            this.target.Decrypt(cipher, timestamp, signature, out plain);

            var plainText = Encoding.UTF8.GetString(plain);

            Assert.AreEqual(
                "`1234567890-=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz~!@#$%^&*()_+",
                plainText);
        }
    }
}