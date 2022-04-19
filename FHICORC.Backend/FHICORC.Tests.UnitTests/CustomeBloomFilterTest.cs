using NUnit.Framework;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Linq;

namespace FHICORC.Tests.UnitTests
{
    internal class CustomeBloomFilterTest
    {

        private static Random random = new Random();


        [Test]
        public void CustomeFilter()
        {
            //var m = 47926;
            //var k = 33;
            //var bloomStats = CalcEleErr(m, k);

            var bloomStats = BloomFilterUtils.CalcOptimalMK(1000, 1E-10);
            var m = bloomStats.m;
            var k = bloomStats.k;

            var str = "whatssup";

            var filter = new BitArray(m);

            filter.AddToFilter(str, m, k);
            filter.AddToFilter("hello", m, k);
            filter.AddToFilter("nahanaa", m, k);

            var contains = filter.Contains(str, m, k);

            Assert.IsTrue(contains);
        }


        [TestCase(1000)]
        public void AddToFilterTest(int numberOfHashes) 
        {
            var bloomStats = BloomFilterUtils.CalcOptimalMK(1000, 1E-10);
            var m = bloomStats.m;
            var k = bloomStats.k;
            var filter = new BitArray(m);

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (int i = 0; i < numberOfHashes; i++) {
                var str = new string(Enumerable.Repeat(chars, random.Next(8, 100))
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var element = sha256_hash(str);
                filter.AddToFilter(element, m, k);
            }


            var testStr = "helloWhatsippp";
            var testHash = sha256_hash(testStr);

            filter.AddToFilter(testHash, m, k);

            Assert.IsTrue(filter.Contains(testHash, m, k));
            Assert.IsTrue(!filter.Contains("notThis", m, k));
            Assert.IsTrue(!filter.Contains("orTHis", m, k));
            Assert.IsTrue(!filter.Contains("DeffinetlyNotThis", m, k));

        }


        public static string sha256_hash(string value)
        {
            using var hash = SHA256.Create();
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(byteArray).ToLower();
        }

    }
}
