using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

namespace HamustroNClient.Security
{

    // TODO handle this as dependencies
    public class HashUtil
    {
        private static byte[] Hash(byte[] data, IDigest digestAlgoritm)
        {
            digestAlgoritm.BlockUpdate(data, 0, data.Length);

            var result = new byte[digestAlgoritm.GetDigestSize()];

            digestAlgoritm.DoFinal(result, 0);

            return result;
        }

        private static string ByteToHexString(IEnumerable<byte> data)
        {
            var result = new StringBuilder();

            foreach (var x in data)
            {
                result.Append(String.Format("{0:x2}", x));
            }

            return result.ToString();
        }

        public static byte[] HashSha256(byte[] data)
        {
            var hashAlgoritm = new Sha256Digest();

            return Hash(data, hashAlgoritm);
        }

        public static string HashSha256ToString(byte[] data)
        {
            var b = HashSha256(data);

            return ByteToHexString(b);
        }

        public static byte[] HashMd5(byte[] data)
        {
            var hashAlgoritm = new MD5Digest();

            return Hash(data, hashAlgoritm);
        }

        public static string HashMd5ToString(byte[] data)
        {
            var b = HashMd5(data);

            return ByteToHexString(b);
        }
    }
}
