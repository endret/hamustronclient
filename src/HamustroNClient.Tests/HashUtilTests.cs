using System;
using System.Text;
using HamustroNClient.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace HamustroNClient.Tests
{
    [TestClass]
    public class HashUtilTests
    {
        const string TestString = "Lorem Ipsum";

        const string TestStringInValidSha256HexString = "030dc1f936c3415aff3f3357163515190d347a28e758e1f717d17bae453541c9";

        [TestMethod]
        public void HashSha256ToString_ReturnValidHexString()
        {
            // Act

            var actual = HashUtil.HashSha256ToString(Encoding.UTF8.GetBytes(TestString));

            // Assert

            Assert.AreEqual(TestStringInValidSha256HexString, actual);

        }      
    }

    [Ignore]
    [TestClass]
    public class MyClass
    {
        [TestMethod]
        public async Task MyTestMethod2()
        {
            var ct = new ClientTracker("http://139.59.132.108:8080", "xxx", "123", "clientId", "systemVersion", "productVersion", "system", "gitHash", 0);


            ct.GenerateSession();

            await ct.TrackEvent("Endre was here", 1u, "param1", true);
        }
    }
}
