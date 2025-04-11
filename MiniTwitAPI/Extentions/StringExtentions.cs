using System.Security.Cryptography;
using System.Text;

namespace MiniTwitAPI.Extentions
{
    public static class StringExtensions
    {
        public static string Sha256Hash(this string value)
        {
            var sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
