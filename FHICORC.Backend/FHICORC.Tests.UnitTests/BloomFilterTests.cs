using BloomFilter;
using BloomFilter.HashAlgorithms;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace FHICORC.Tests.UnitTests
{
    internal class BloomFilterTests
    {
        [Test]
        public void workingExample()
        {

            var hashMethod = HashMethod.SHA256;

            IBloomFilter bf = FilterBuilder.Build(1000, 0.0000000001, hashMethod);

            bf.Add("Value");

            Console.WriteLine(bf.ToString());

            var a = bf.Contains("Value");

            var bloomFilterBitArray = bf.GetPrivateFieldValue<System.Collections.BitArray>("_hashBits");

            var c = bf.ToString();
            Console.WriteLine(bf.Contains("Value"));

           
            IBloomFilter bf2 = FilterBuilder.Build(1000, 0.0000000001, hashMethod);
            bf2.SetPrivateFieldValue("_hashBits", bloomFilterBitArray);

            var pppp = bf2.Contains("Value");


            byte[] str = Encoding.UTF8.GetBytes("Value");
            //var manualCompteHash = new Murmur3KirschMitzenmacher().ComputeHash(str, 47926, 34);
            var manualCompteHash = new HashCryptoSHA256().ComputeHash(str, 47926, 34);
            var manualBoolArr = new bool[47926];

            foreach (var m in manualCompteHash) {
                manualBoolArr[m] = true;
            }

            var manualBitArr = new BitArray(manualBoolArr);
            IBloomFilter bf3 = FilterBuilder.Build(1000, 0.0000000001, hashMethod);
            
            bf3.SetPrivateFieldValue("_hashBits", manualBitArr);
            var pppp2 = bf3.Contains("Value");

        }

        [Test]
        public void CustomeFilter() 
        {
            var m = 47926;
            var k = 34;

            var filer = new bool[m];
        
        }

        public bool[] AddToFilter(bool[] filter, string str, int m=47926, int k=34)
        {
            var hash = hashData(Encoding.UTF8.GetBytes(str), m, k);

            foreach (int i in hash)
                filter[i] = true;

            return filter;

        }

        public bool Contains(bool[] filter, string str, int m = 47926, int k = 34)
        {
            var hash = hashData(Encoding.UTF8.GetBytes(str), m, k);

            foreach (int i in hash) {
                if (!filter[i])
                    return false;
            }
                

            return true;
        }




        [Test]
        public void PhoneMock() 
        {
            var bf = FilterBuilder.Build(1000, 0.0000000001, HashMethod.SHA256);
            bf.Add("Value");
            var bloomFilterBitArray = bf.GetPrivateFieldValue<System.Collections.BitArray>("_hashBits");
            
            var b = ContainsMockPhone("Value", bloomFilterBitArray);

            Assert.That(b, Is.True);
        }

        public bool ContainsMockPhone(string str, BitArray filter, int m=47926, int k=34) 
        {
            byte[] byteStr = Encoding.UTF8.GetBytes(str);
            var hash = hashData(byteStr, m, k);

            foreach (int i in hash) {
                if (!filter[i])
                    return false;
            }
            return true;
        }

        public int[] hashData(byte[] data, int m, int k) 
        {

            var hashAlgorithm = SHA256.Create();

            int[] array = new int[k];
            int num = 0;
            byte[] array2 = new byte[0];
            byte[] outputBuffer = new byte[hashAlgorithm.HashSize / 8];

            while (num < k)
            {
                hashAlgorithm.TransformBlock(array2, 0, array2.Length, outputBuffer, 0);
                array2 = hashAlgorithm.ComputeHash(data, 0, data.Length);
                BitArray bit = new BitArray(array2);
                int num2 = (int)(32 - NumberOfLeadingZeros((uint)m));
                int num3 = array2.Length * 8;
                for (int i = 0; i < num3 / num2; i++)
                {
                    if (num >= k)
                    {
                        break;
                    }

                    int from = i * num2;
                    int to = (i + 1) * num2;
                    int num4 = BitToIntOne(bit, from, to);
                    if (num4 < m)
                    {
                        array[num] = num4;
                        num++;
                    }
                }
            }

            return array;

        }

        public uint NumberOfLeadingZeros(uint i)
        {
            if (i == 0)
            {
                return 32u;
            }

            uint num = 1u;
            if (i >> 16 == 0)
            {
                num += 16;
                i <<= 16;
            }

            if (i >> 24 == 0)
            {
                num += 8;
                i <<= 8;
            }

            if (i >> 28 == 0)
            {
                num += 4;
                i <<= 4;
            }

            if (i >> 30 == 0)
            {
                num += 2;
                i <<= 2;
            }

            return num - (i >> 31);
        }

        public int BitToIntOne(BitArray bit, int from, int to)
        {
            int num = to - from;
            int count = bit.Count;
            int num2 = 0;
            for (int i = 0; i < num && i < count && i < 32; i++)
            {
                num2 = (bit[i + from] ? (num2 + (1 << i)) : num2);
            }

            return num2;
        }
    }
}
