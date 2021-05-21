using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace SpotKick.Desktop.SpotifyAuth
{
    public class CodeVerifier
    {
        public string Verifier { get; set; }

        public string Challenge { get; set; }

        const int ByteLength = 64;

        public CodeVerifier()
        {
            Verifier = GenerateToken();
            Challenge = GenerateChallenge();
        }

        /// <summary>
        /// Hashes and Base64 encodes the verifier
        /// </summary>
        /// <returns></returns>
        public string GenerateChallenge()
        {
            var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(Verifier));
            return WebEncoders.Base64UrlEncode(hash);
        }


        /// <summary>
        /// Generate a fixed length token that can be used in url without endcoding it
        /// </summary>
        /// <returns></returns>
        public static string GenerateToken()
        {
            // get secure array bytes
            var secureArray = GenerateRandomBytes();

            // convert in an url safe string
            var urlToken = WebEncoders.Base64UrlEncode(secureArray);

            return urlToken;
        }

        /// <summary>
        /// Generate a cryptographically secure array of bytes with a fixed length
        /// </summary>
        /// <returns></returns>
        private static byte[] GenerateRandomBytes()
        {
            using var provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[ByteLength];
            provider.GetBytes(byteArray);

            return byteArray;
        }
    }
}
