using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiFoundation.Net.Http
{
    [TestClass]
    public class DefaultTimestampProviderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DefaultTimestampProviderTest_ctor_InvalidDuration()
        {
            new DefaultTimestampProvider(TimeSpan.Zero);
        }

        [TestMethod]
        public void DefaultTimestampProviderTest_GetTimestamp()
        {
            ITimestampProvider<long> target = new DefaultTimestampProvider(TimeSpan.FromMinutes(1));
            var actual = target.GetTimestamp();

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void DefaultTimestampProviderTest_Validate()
        {
            var input = DateTime.UtcNow.AddMinutes(-1).Ticks;

            ITimestampProvider<long> target = new DefaultTimestampProvider(TimeSpan.FromMinutes(2));

            target.Validate(input);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTimestampException))]
        public void DefaultTimestampProviderTest_ExpiredTimestamp()
        {
            var input = DateTime.UtcNow.AddMinutes(-1).Ticks;

            ITimestampProvider<long> target = new DefaultTimestampProvider(TimeSpan.FromMinutes(1));

            target.Validate(input);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTimestampException))]
        public void DefaultTimestampProviderTest_InvalidTimestamp()
        {
            var input = DateTime.UtcNow.AddMinutes(2).Ticks;

            ITimestampProvider<long> target = new DefaultTimestampProvider(TimeSpan.FromMinutes(1));

            target.Validate(input);
        }
    }
}