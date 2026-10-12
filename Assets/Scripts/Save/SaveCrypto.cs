using System;
using System.Security.Cryptography;
using System.Text;

namespace WaterSort.Core
{
    /// <summary>
    /// AES-256 encryption and salted SHA-256 hashing for save data.
    /// Plain C# (no UnityEngine) so it can be unit tested.
    /// Encrypted format: Base64(random 16-byte IV + AES-CBC ciphertext).
    /// </summary>
    public class SaveCrypto
    {
        private const int IvLength = 16;
        private readonly byte[] _key;
        private readonly string _salt;

        public SaveCrypto(string password, string salt)
        {
            _salt = salt;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000))
            {
                _key = deriveBytes.GetBytes(32); // 256-bit key, derived once
            }
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV(); // Fresh random IV per value: equal values never encrypt identically
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    byte[] output = new byte[IvLength + encryptedBytes.Length];
                    Buffer.BlockCopy(aes.IV, 0, output, 0, IvLength);
                    Buffer.BlockCopy(encryptedBytes, 0, output, IvLength, encryptedBytes.Length);
                    return Convert.ToBase64String(output);
                }
            }
        }

        /// <summary>
        /// Throws FormatException / CryptographicException for data that was not produced by Encrypt
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            byte[] input = Convert.FromBase64String(encryptedText);
            if (input.Length <= IvLength)
                throw new CryptographicException("Encrypted value is too short");

            byte[] iv = new byte[IvLength];
            Buffer.BlockCopy(input, 0, iv, 0, IvLength);

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(input, IvLength, input.Length - IvLength);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        public string Hash(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data + _salt));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
