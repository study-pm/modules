using HR.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Test
{
    [TestClass]
    public class CryptoTests
    {
        private static readonly Encoding Utf8 = Encoding.UTF8;

        [TestMethod]
        public void Encrypt_Then_Decrypt_ReturnsOriginalText()
        {
            var key = Crypto.GenerateAesKey();
            var iv = Crypto.GenerateAesIV();
            string original = "Test string for encryption";

            string encrypted = Crypto.Encrypt(original, key, iv);
            Assert.IsFalse(string.IsNullOrWhiteSpace(encrypted), "Encrypted string should not be empty");

            string decrypted = Crypto.Decrypt(encrypted, key, iv);
            Assert.AreEqual(original, decrypted, "Decrypted text should match original");
        }

        [TestMethod]
        public void GenerateAesIV_Returns16Bytes()
        {
            var iv = Crypto.GenerateAesIV();
            Assert.IsNotNull(iv);
            Assert.AreEqual(16, iv.Length, "AES IV length must be 16 bytes");
        }

        [TestMethod]
        public void GenerateAesKey_DefaultSize_Returns32Bytes()
        {
            var key = Crypto.GenerateAesKey();
            Assert.IsNotNull(key);
            Assert.AreEqual(32, key.Length, "Default AES key size must be 32 bytes (256 bits)");
        }

        [TestMethod]
        public void GenerateAesKey_CustomSize_ReturnsCorrectLength()
        {
            var key128 = Crypto.GenerateAesKey(128);
            Assert.IsNotNull(key128);
            Assert.AreEqual(16, key128.Length, "AES 128-bit key length must be 16 bytes");

            var key192 = Crypto.GenerateAesKey(192);
            Assert.IsNotNull(key192);
            Assert.AreEqual(24, key192.Length, "AES 192-bit key length must be 24 bytes");
        }

        [TestMethod]
        public void GenerateSalt_ReturnsCorrectSize()
        {
            var saltDefault = Crypto.GenerateSalt();
            Assert.IsNotNull(saltDefault);
            Assert.AreEqual(16, saltDefault.Length, "Default salt size must be 16 bytes");

            var saltCustom = Crypto.GenerateSalt(32);
            Assert.IsNotNull(saltCustom);
            Assert.AreEqual(32, saltCustom.Length, "Custom salt size must be 32 bytes");
        }

        [TestMethod]
        public void GenerateSecret_ReturnsSecretAndBase32String()
        {
            var (secretBytes, base32Secret) = Crypto.GenerateSecret();

            Assert.IsNotNull(secretBytes);
            Assert.AreEqual(20, secretBytes.Length, "Secret byte array length must be 20 bytes");

            Assert.IsFalse(string.IsNullOrWhiteSpace(base32Secret), "Base32 secret string must not be empty");
            // Check if base32Secret consists of allowed legal Base32 symbols (A-Z, 2-7)
            foreach (var c in base32Secret)
            {
                Assert.IsTrue(
                    (c >= 'A' && c <= 'Z') || (c >= '2' && c <= '7'),
                    $"Base32 secret contains invalid character: {c}");
            }
        }

        [TestMethod]
        public void GetOtpAuthUri_ReturnsCorrectFormat()
        {
            var secret = Crypto.GenerateSecret().SecretBytes;
            string user = "testuser";

            string uri = Crypto.GetOtpAuthUri(secret, user);

            Assert.IsTrue(uri.StartsWith("otpauth://totp/HR:testuser?secret="), "URI should start with correct prefix");
            Assert.IsTrue(uri.Contains("&issuer=HR"), "URI should contain issuer parameter");
        }

        [TestMethod]
        public void HashPassword_Returns32ByteHash()
        {
            string password = "MyPassword123!";
            byte[] salt = Crypto.GenerateSalt();

            byte[] hash = Crypto.HashPassword(password, salt);

            Assert.IsNotNull(hash);
            Assert.AreEqual(32, hash.Length, "Password hash length must be 32 bytes");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Decrypt_InvalidBase64_ThrowsFormatException()
        {
            var key = Crypto.GenerateAesKey();
            var iv = Crypto.GenerateAesIV();

            // Pass invalid base64 string
            Crypto.Decrypt("InvalidBase64$$$", key, iv);
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public void Decrypt_WrongKey_ThrowsCryptographicException()
        {
            var key = Crypto.GenerateAesKey();
            var iv = Crypto.GenerateAesIV();
            string original = "Secret text";

            string encrypted = Crypto.Encrypt(original, key, iv);

            var wrongKey = Crypto.GenerateAesKey();
            // Decrypt attempt with invalid key must throw exception
            Crypto.Decrypt(encrypted, wrongKey, iv);
        }
    }
}
