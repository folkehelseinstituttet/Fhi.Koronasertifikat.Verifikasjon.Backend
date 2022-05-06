using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FHICORC.Tests.ComponentTests
{
    public class CheckIfUciExistInTestDccg : SeedDb
    {
        //[TestCase("URN:UVCI:01DE/IZ12345A/5CWLU12RNOB9RXSEOP6FG8#W")]
        //[TestCase("URN:UVCI:01DE/T23234243/50CUH2UMZUJW382VP58G2Q#F")]
        //[TestCase("URN:UVCI:01DE/A23234242/50CUH2UMZUJW382VP58G2Q#F")]
        //[TestCase("URN:UVCI:01DE/A80013335/50CUH2UMZUJW382VP58G2Q#F")]
        //[TestCase("URN:UVCI:V1:DE:RKMTXFFUA1X7VPKELA9KN9W625")]
        //[TestCase("URN:UVCI:V1:DE:RKMTXFFUA1X7VPKELA9KN9W625")]
        //[TestCase("URN:UVCI:V1:DE:9LUPGOGOG21G323R0LQSC4AL6J")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/2F88YXJNKHZTYAYG361E9C4B#M")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/1UO4FNMOXA5YX8BH8QFR3QE6#R")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/7L3DDS6L01K1AVUNDFVE1J3N#G")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/81Q722UFMG4BC3FG0CPGYG4F#U")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/2WX04SKIFCJWEVKMRW2MRIJM#A")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/1Y4YMOSN3K9OFE84FDT00MJP#H")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/42K4VE9X7S8LW3NCVSXUTWAT#8")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/C1QL7LIGUO88HDX15IPZH3MH#Z")]
        //[TestCase("URN:UVCI:01:RO:VPG0X4YKM29ZEPLK1LXJ81LWO7Q6ER#C")]
        //[TestCase("URN:UVCI:01:RO:82NVE092TEQU07XSF9O6W34JV#S")]

        //[TestCase("URN:UVCI:01DE/IZ12345A/5CWLU12RNOB9RXSEOP6FG8")]
        //[TestCase("URN:UVCI:01DE/T23234243/50CUH2UMZUJW382VP58G2Q")]
        //[TestCase("URN:UVCI:01DE/A23234242/50CUH2UMZUJW382VP58G2Q")]
        //[TestCase("URN:UVCI:01DE/A80013335/50CUH2UMZUJW382VP58G2Q")]
        //[TestCase("URN:UVCI:V1:DE:RKMTXFFUA1X7VPKELA9KN9W625")]
        //[TestCase("URN:UVCI:V1:DE:RKMTXFFUA1X7VPKELA9KN9W625")]
        //[TestCase("URN:UVCI:V1:DE:9LUPGOGOG21G323R0LQSC4AL6J")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/2F88YXJNKHZTYAYG361E9C4B")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/1UO4FNMOXA5YX8BH8QFR3QE6")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/7L3DDS6L01K1AVUNDFVE1J3N")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/81Q722UFMG4BC3FG0CPGYG4F")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/2WX04SKIFCJWEVKMRW2MRIJM")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/1Y4YMOSN3K9OFE84FDT00MJP")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/42K4VE9X7S8LW3NCVSXUTWAT")]
        //[TestCase("URN:UVCI:01DE/IZDEMOX01/C1QL7LIGUO88HDX15IPZH3MH")]
        //[TestCase("URN:UVCI:01:RO:VPG0X4YKM29ZEPLK1LXJ81LWO7Q6ER")]
        //[TestCase("URN:UVCI:01:RO:82NVE092TEQU07XSF9O6W34JV")]

        [TestCase("URN:UVCI:01:RO:82NVE092TEQU07XSF9O6W34JV#S")]
        [TestCase("URN:UVCI:01:RO:MXG127DO85RZLXK2E9EZYKW043E9Q6#R")]

        [TestCase("URN:UVCI:01:RO:82NVE092TEQU07XSF9O6W34JV")]
        [TestCase("URN:UVCI:01:RO:MXG127DO85RZLXK2E9EZYKW043E9Q6")]

        public void ChecIfUciExists(string uci) {
            for (int idx = 0; idx < uci.Length; idx++)
            {
                var uciSubstring = uci.Substring(idx);
                var base64Encoded = Base64EncodeUCI(uciSubstring);

                var revocationHash = _coronapassContext.RevocationHash.Where(x => x.Hash.Equals(base64Encoded)).FirstOrDefault();

                if (revocationHash is not null) {
                    var yeah = revocationHash;

                }
            }
        }

        //[TestCase("82NVE092TEQU07XSF9O6W34JV#S")]
        [TestCase("J4PCK4sFs63kH/EeP7+C3A==")]
        public void CheckSingleUciExists(string uci)
        {

            //var base64Encoded = Base64EncodeUCI(uci);


        }


        public string Base64EncodeUCI(string uciSubstring)
        {

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Computing Hash - returns here byte array
                var sha256Bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(uciSubstring));
                var sha256B64 = Convert.ToBase64String(sha256Bytes);

                var sha256Bytes2 = new byte[32];
                Array.Copy(sha256Bytes, sha256Bytes2, sha256Bytes2.Length);

                var sha256B642 = Convert.ToBase64String(sha256Bytes2);

                return sha256B642;
            }
        }
    }
}
