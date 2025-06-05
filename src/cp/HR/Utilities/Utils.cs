using HR.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HR.Utilities
{
    internal static class Utils
    {
        /// <summary>
        /// Generates a QR code image as a byte array representing the OTP Auth URI for a given secret and user login.
        /// </summary>
        /// <param name="secret">The secret key used for generating the OTP Auth URI.</param>
        /// <param name="login">The user login or identifier to include in the OTP Auth URI.</param>
        /// <returns>A byte array containing the PNG image data of the generated QR code.</returns>
        /// <remarks>
        /// The generated QR code encodes the OTP Auth URI, which can be scanned by authenticator apps to set up time-based one-time password (TOTP) authentication.
        /// </remarks>
        public static byte[] GetQrCode(byte[] secret, string login)
        {
            // Generate OTP Auth URI
            string pathUrl = Crypto.GetOtpAuthUri(secret, login);
            // Generate QR code
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(pathUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
