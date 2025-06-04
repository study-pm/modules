using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HR.Services
{
    internal class Crypto
    {
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
        /// Generates random secret
        /// </summary>
        /// <returns>Base32 secret string</returns>
        public byte[] GenerateSecret()
        {
            return KeyGeneration.GenerateRandomKey(20);
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
