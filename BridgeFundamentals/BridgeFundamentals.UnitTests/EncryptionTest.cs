using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodes.Base;
using Sodes.Base.Test.Helpers;

namespace TestRobot
{
    [TestClass]
    public class EncryptionTest : TestBase
    {
        [TestMethod, TestCategory("CI"), TestCategory("Other")]
        public void Encrypt1()
        {
            string data = "Hello world";
            string key = "JobScheffers";
            string e = Encryption.Encrypt2(data, key);
            string d = Encryption.Decrypt2(e, key);
            Assert.AreEqual(data, d);

            data = DateTime.UtcNow.ToString();
            e = Encryption.Encrypt2(data, key);
            d = Encryption.Decrypt2(e, key);
            Assert.AreEqual(data, d);

            key = "AKQbridge";
            e = Encryption.Encrypt2(data, key);
            d = Encryption.Decrypt2(e, key);
            Assert.AreEqual(data, d);
        }
    }
}
