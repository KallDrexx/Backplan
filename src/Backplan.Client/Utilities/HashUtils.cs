using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Utilities
{
    public class HashUtils : IHashUtils
    {
        public string GenerateHash(Stream stream)
        {
            var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(stream);
            return BitConverter.ToString(hashBytes)
                               .Replace("-", "");
        }
    }
}
