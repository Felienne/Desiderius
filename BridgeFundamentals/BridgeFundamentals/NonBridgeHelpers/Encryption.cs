
using System.Text;
namespace Sodes.Base
{
    public static class Encryption
    {
        public static string Encrypt(string s)
        {
            return s;
        }

        public static string Decrypt(string s)
        {
            return s;
        }

        public static string Encrypt2(string data, string key)
        {
            StringBuilder encrypted = new StringBuilder(data + data);
            int rotator = 0;
            for (int i = 0; i < data.Length; i++)
            {
                int t = data[i] + key[rotator];
                encrypted[2 * i] = (char)(t / 2 - 5);
                encrypted[2 * i + 1] = (char)(t / 2 + 5 + (t % 2));
                rotator++; if (rotator >= key.Length) rotator = 0;
            }

            return encrypted.ToString();
        }

        public static string Decrypt2(string data, string key)
        {
            StringBuilder decrypted = new StringBuilder("".PadLeft(data.Length / 2));
            int rotator = 0;
            for (int i = 0; i < data.Length / 2; i++)
            {
                int t = data[2 * i] + data[2 * i + 1] - key[rotator];
                decrypted[i] = (char)(t);
                rotator++; if (rotator >= key.Length) rotator = 0;
            }

            return decrypted.ToString();
        }
    }
}
