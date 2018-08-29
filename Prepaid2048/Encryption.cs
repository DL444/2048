using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Prepaid2048
{
    public static class EncryptionHelper
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        //private const int Keysize = 128;

        // This constant determines the number of iterations for the password bytes generation function.
        //private const int DerivationIterations = 100000;

        public static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(plainText);
            byte[] result;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }
            return Convert.ToBase64String(result);
        }

        private static byte[] GenerateRandomEntropy(int bits)
        {
            var randomBytes = new byte[bits / 8]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}