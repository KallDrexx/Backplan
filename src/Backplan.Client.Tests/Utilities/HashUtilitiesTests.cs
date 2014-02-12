using Backplan.Client.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Tests.Utilities
{
    [TestClass]
    public class HashUtilitiesTests
    {
        [TestMethod]
        public void Generates_Correct_Hash()
        {
            var testString = "Testing 12345";
            var expectedHash = GenerateHash(testString);
            string resultHash;

            using (Stream stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(testString);
                writer.Flush();
                stream.Position = 0;

                resultHash = HashUtilities.GenerateHash(stream);
            }

            Assert.AreEqual(expectedHash, resultHash, "Generated hash was incorrect");
        }

        private string GenerateHash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var encodedBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(encodedBytes);
                return BitConverter.ToString(hashBytes)
                                   .Replace("-", "");
            }
        }
    }
}
