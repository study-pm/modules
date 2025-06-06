using OtpNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HR.Services
{
    internal static class Crypto
    {
        /// <summary>
        /// AES IV storage relative path
        /// </summary>
        public static readonly string keysPath = "Static\\Keys";
        /// <summary>
        /// Decrypts a Base64-encoded ciphertext string using AES encryption with the specified key and initialization vector (IV).
        /// </summary>
        /// <param name="cipherTextBase64">The ciphertext encoded as a Base64 string.</param>
        /// <param name="key">The AES encryption key as a byte array.</param>
        /// <param name="iv">The AES initialization vector (IV) as a byte array.</param>
        /// <returns>The decrypted plaintext string.</returns>
        /// <remarks>
        /// The method uses AES in CBC mode with PKCS7 padding to perform the decryption.
        /// </remarks>
        public static string Decrypt(string cipherTextBase64, byte[] key, byte[] iv)
        {
            byte[] cipherText = Convert.FromBase64String(cipherTextBase64);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        /// <summary>
        /// Encrypts the specified plaintext string using AES encryption with the provided key and initialization vector (IV),
        /// and returns the result as a Base64-encoded string.
        /// </summary>
        /// <param name="plainText">The plaintext string to encrypt.</param>
        /// <param name="key">The AES encryption key as a byte array.</param>
        /// <param name="iv">The AES initialization vector (IV) as a byte array.</param>
        /// <returns>A Base64-encoded string representing the encrypted ciphertext.</returns>
        /// <remarks>
        /// The method uses AES in CBC mode with PKCS7 padding to perform the encryption.
        /// The output is Base64-encoded for safe storage or transmission, such as saving in a database.
        /// </remarks>
        public static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    byte[] encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted); // Сохраняйте в базе в Base64
                }
            }
        }
        /// <summary>
        /// Generates a new random initialization vector (IV) for AES encryption.
        /// </summary>
        /// <param name="filePath">This parameter is not used in the current implementation.</param>
        /// <returns>A byte array containing the generated 16-byte AES IV.</returns>
        /// <remarks>
        /// The initialization vector (IV) is always 16 bytes for AES and is generated using a cryptographically secure random number generator.
        /// </remarks>
        /// <example>
        /// <code>
        /// filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aes_iv.bin");
        /// File.WriteAllBytes(filePath, Crypto.GenerateAesIV());
        /// </code>
        /// </example>
        public static byte[] GenerateAesIV()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateIV(); // IV is always 16 bytes for AES
                return aes.IV;
            }
        }
        /// <summary>
        /// Generates a random AES encryption key of the specified size.
        /// </summary>
        /// <param name="size">The size of the AES key in bits. Default is 256 bits (32 bytes).</param>
        /// <returns>A byte array containing the generated AES key.</returns>
        /// <remarks>
        /// The method creates a new AES instance, sets the key size, and generates a cryptographically secure random key.
        /// </remarks>
        /// <example>
        /// <code>
        /// string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aes_key.bin");
        /// File.WriteAllBytes(filePath, Crypto.GenerateAesKey());
        /// </code>
        /// </example>
        public static byte[] GenerateAesKey(int size = 256)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = size;
                aes.GenerateKey();
                return aes.Key;
            }
        }
        /// <summary>
        /// Generates random salt
        /// </summary>
        /// <param name="size">The size of the salt in bytes (default is 16 bytes)</param>
        /// <returns>Randomly generated salt as a byte array</returns>
        public static byte[] GenerateSalt(int size = 16)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        /// <summary>
        /// Generates a new cryptographic secret key and its Base32-encoded string representation.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>SecretBytes</c> - The raw secret key as a byte array.</description></item>
        /// <item><description><c>Base32Secret</c> - The Base32-encoded string representation of the secret key.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The secret key is generated as a 20-byte random key suitable for cryptographic use.
        /// The Base32 encoding facilitates easy sharing or storage of the secret in a human-readable format,
        /// commonly used in authentication scenarios such as TOTP (Time-based One-Time Password).
        /// </remarks>
        public static (byte[] SecretBytes, string Base32Secret) GenerateSecret()
        {
            byte[] secret = KeyGeneration.GenerateRandomKey(20);
            string base32Secret = Base32Encoding.ToString(secret);
            return (secret, base32Secret);
        }
        /// <summary>
        /// Forms URI for QR code
        /// </summary>
        /// <returns>OTP Auth URI</returns>
        public static string GetOtpAuthUri(byte[] secret, string user)
        {
            string issuer = "HR";
            string base32Secret = Base32Encoding.ToString(secret);
            return $"otpauth://totp/{issuer}:{user}?secret={base32Secret}&issuer={issuer}";
        }
        /// <summary>
        /// Gets hash for password and salt combination
        /// </summary>
        /// <param name="password">The password string to hash</param>
        /// <param name="salt">The salt byte array to combine with the password</param>
        /// <returns>Hashed password as a byte array of length 32 bytes</returns>
        public static byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return pbkdf2.GetBytes(32); // 32 bytes — hash length
            }
        }
    }
}
