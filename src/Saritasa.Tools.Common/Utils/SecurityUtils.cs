// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Shortcuts for hash generation to string.
    /// </summary>
    public static class SecurityUtils
    {
#if !PORTABLE && !NETSTANDARD1_2
        const char PasswordMethodHashSeparator = '$';

        /// <summary>
        /// Returns string's MD5 hash.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <returns>MD5 hash bytes array.</returns>
        [DebuggerStepThrough]
        public static byte[] MD5(string target)
        {
            Guard.IsNotNull(target, nameof(target));
            byte[] bytes = Encoding.UTF8.GetBytes(target);
            using (var cryptoServiceProvider = System.Security.Cryptography.MD5.Create())
            {
                return cryptoServiceProvider.ComputeHash(bytes);
            }
        }

        /// <summary>
        /// Returns string's SHA1 hash.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <returns>SHA1 hash bytes array.</returns>
        public static byte[] Sha1(string target)
        {
            Guard.IsNotNull(target, nameof(target));
            using (var hasher = System.Security.Cryptography.SHA1.Create())
            {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(target));
            }
        }

        /// <summary>
        /// Returns string's SHA256 hash.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <returns>SHA256 hash bytes array.</returns>
        public static byte[] Sha256(string target)
        {
            Guard.IsNotNull(target, nameof(target));
            using (var hasher = System.Security.Cryptography.SHA256.Create())
            {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(target));
            }
        }

        /// <summary>
        /// Returns string's SHA384 hash.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <returns>SHA384 hash bytes array.</returns>
        public static byte[] Sha384(string target)
        {
            Guard.IsNotNull(target, nameof(target));
            using (var hasher = System.Security.Cryptography.SHA384.Create())
            {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(target));
            }
        }

        /// <summary>
        /// Returns string's SHA512 hash.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <returns>SHA512 hash bytes array.</returns>
        public static byte[] Sha512(string target)
        {
            Guard.IsNotNull(target, nameof(target));
            using (var hasher = System.Security.Cryptography.SHA512.Create())
            {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(target));
            }
        }

        private const int DefaultSaltSize = 128 / 8;

        private const int DefaultHashSize = 256 / 8;

        private const int DefaultNumberOfIterations = 10000;

        /// <summary>
        /// Returns PBKDF2 compatible hash by using a pseudo-random number generator based on HMACSHA1. Default number
        /// of iterations is 10000.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <returns>Hash.</returns>
        public static byte[] Pbkdf2Sha1(string target) => Pbkdf2Sha1(target, DefaultNumberOfIterations);

        /// <summary>
        /// Returns PBKDF2 compatible hash by using a pseudo-random number generator based on HMACSHA1.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <param name="numOfIterations">Number of iterations.</param>
        /// <returns>Hash.</returns>
        public static byte[] Pbkdf2Sha1(string target, int numOfIterations)
        {
            Guard.IsNotNull(target, nameof(target));
            Guard.IsNotNegativeOrZero(numOfIterations, nameof(numOfIterations));
            using (var rfc2898DeriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(target, DefaultSaltSize, numOfIterations))
            {
                var bytes = rfc2898DeriveBytes.GetBytes(DefaultHashSize);
                var salt = rfc2898DeriveBytes.Salt;

                var output = new byte[bytes.Length + salt.Length];
                Buffer.BlockCopy(salt, 0, output, 0, salt.Length);
                Buffer.BlockCopy(bytes, 0, output, salt.Length, bytes.Length);
                return output;
            }
        }

        /// <summary>
        /// Returns PBKDF2 compatible hash by using a pseudo-random number generator based on HMACSHA1.
        /// </summary>
        /// <param name="target">String to be hashed.</param>
        /// <param name="salt">Salt.</param>
        /// <param name="numOfIterations">Number of iterations.</param>
        /// <returns>Hash.</returns>
        public static byte[] Pbkdf2Sha1(string target, byte[] salt, int numOfIterations)
        {
            Guard.IsNotNull(target, nameof(target));
            Guard.IsNotNull(salt, nameof(salt));
            Guard.IsNotNegativeOrZero(numOfIterations, nameof(numOfIterations));
            using (var rfc2898DeriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(target, salt, numOfIterations))
            {
                var bytes = rfc2898DeriveBytes.GetBytes(DefaultHashSize);

                var output = new byte[bytes.Length + salt.Length];
                Buffer.BlockCopy(salt, 0, output, 0, salt.Length);
                Buffer.BlockCopy(bytes, 0, output, salt.Length, bytes.Length);
                return output;
            }
        }

        /// <summary>
        /// Convert array of bytes to string representation.
        /// </summary>
        /// <param name="bytes">Bytes array.</param>
        /// <returns>Hex string.</returns>
        internal static string ConvertBytesToString(byte[] bytes)
        {
            Guard.IsNotNull(bytes, nameof(bytes));
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// Convert string that contains hex representation of bytes to bytes array.
        /// </summary>
        /// <param name="target">Hex bytes string.</param>
        /// <returns>Bytes array.</returns>
        internal static byte[] ConvertStringToBytes(string target)
        {
            Guard.IsNotNull(target, nameof(target));
            if (target.Length % 2 != 0)
            {
                throw new ArgumentException(Properties.Strings.TargetStringLengthIncorrect, nameof(target));
            }
            var bytes = new byte[target.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(target.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }

        /// <summary>
        /// String hash method.
        /// </summary>
        public enum HashMethod
        {
            /// <summary>
            /// MD5. Is not recommended.
            /// </summary>
            Md5,

            /// <summary>
            /// SHA-1.
            /// </summary>
            Sha1,

            /// <summary>
            /// SHA-256.
            /// </summary>
            Sha256,

            /// <summary>
            /// SHA-384.
            /// </summary>
            Sha384,

            /// <summary>
            /// SHA-512.
            /// </summary>
            Sha512,

            /// <summary>
            /// PBKDF2 with HMAC-SHA1.
            /// </summary>
            Pbkdf2Sha1,
        }

        private static readonly IDictionary<HashMethod, Func<string, byte[]>> hashMethodsMap =
            new Dictionary<HashMethod, Func<string, byte[]>>
            {
                [HashMethod.Md5] = MD5,
                [HashMethod.Sha1] = Sha1,
                [HashMethod.Sha256] = Sha256,
                [HashMethod.Sha384] = Sha384,
                [HashMethod.Sha512] = Sha512,
                [HashMethod.Pbkdf2Sha1] = Pbkdf2Sha1,
            };

        /// <summary>
        /// Hash string with selected hash method. The string will contain method name that was used for hashing.
        /// </summary>
        /// <param name="target">String to hash.</param>
        /// <param name="method">Method to use to hash.</param>
        /// <returns>Hashed string.</returns>
        public static string Hash(string target, HashMethod method)
        {
            Guard.IsNotNull(target, nameof(target));
            return method.ToString().ToUpperInvariant() + PasswordMethodHashSeparator + ConvertBytesToString(hashMethodsMap[method](target));
        }

        /// <summary>
        /// Compares target string with hashed string.
        /// </summary>
        /// <param name="target">Target non-hashed string.</param>
        /// <param name="hashedStringToCheck">Hashed string with hash method. Should be hashed with Hash() method.</param>
        /// <returns>True if string is correct, false otherwise.</returns>
        public static bool CheckHash(string target, string hashedStringToCheck)
        {
            Guard.IsNotNull(target, nameof(target));
            var separatorIndex = hashedStringToCheck.IndexOf(PasswordMethodHashSeparator);
            if (separatorIndex < 0)
            {
                throw new ArgumentException(string.Format(Properties.Strings.NoHashMethod, hashedStringToCheck));
            }

            HashMethod method = HashMethod.Md5;
            if (!Enum.TryParse(hashedStringToCheck.Substring(0, separatorIndex), true, out method))
            {
                throw new ArgumentException(string.Format(Properties.Strings.HashMethodCannotRecognize,
                    hashedStringToCheck));
            }

            // For pbkdf2 we should use another check with salt.
            if (method == HashMethod.Pbkdf2Sha1)
            {
                var hashStr = hashedStringToCheck.Substring(separatorIndex + 1);
                var bytes = ConvertStringToBytes(hashStr);
                var salt = bytes.Take(DefaultSaltSize).ToArray();
                var hashToCompare = Pbkdf2Sha1(target, salt, DefaultNumberOfIterations);
                return bytes.SequenceEqual(hashToCompare);
            }

            return Hash(target, method) == hashedStringToCheck;
        }
#endif

        #region CRC-32

        private static readonly ConcurrentDictionary<uint, uint[]> crc32Tables =
            new ConcurrentDictionary<uint, uint[]>();

        /// <summary>
        /// IEEE is the most common CRC-32 polynomial.
        /// Used by ethernet (IEEE 802.3), v.42, fddi, gzip, zip, png, etc.
        /// </summary>
        [CLSCompliant(false)]
        public const uint Crc32Ieee = 0xedb88320;

        /// <summary>
        /// Castagnoli's polynomial, used in iSCSI. Has better error detection characteristics than IEEE.
        /// </summary>
        /// <remarks>http://dx.doi.org/10.1109/26.231911</remarks>
        [CLSCompliant(false)]
        public const uint Crc32Castagnoli = 0x82f63b78;

        /// <summary>
        /// Koopman's polynomial. Also has better error detection characteristics than IEEE.
        /// </summary>
        /// <remarks>http://dx.doi.org/10.1109/DSN.2002.1028931</remarks>
        [CLSCompliant(false)]
        public const uint Crc32Koopman = 0xeb31d82e;

        private static uint[] GenerateTableForCrc32(uint polynomial)
        {
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                var entry = i;
                for (var j = 0; j < 8; j++)
                {
                    entry = (entry & 1) == 1 ? (entry >> 1) ^ polynomial : entry >> 1;
                }
                table[i] = entry;
            }

            return table;
        }

        /// <summary>
        /// Returns CRC-32 checksum. By default uses IEEE polynomial.
        /// </summary>
        /// <param name="target">Array of bytes to be hashed.</param>
        /// <param name="polynomial">Polynomial.</param>
        /// <param name="seedCrc">Initial value for crc calculation, default is 0.</param>
        /// <returns>CRC-32 hash.</returns>
        [CLSCompliant(false)]
        public static uint Crc32(byte[] target, uint polynomial = Crc32Ieee, uint seedCrc = 0)
        {
            Guard.IsNotNull(target, nameof(target));

            // Get or generate table.
            var table = crc32Tables.GetOrAdd(polynomial, key => GenerateTableForCrc32(polynomial));

            // Hash calculation.
            unchecked
            {
                var crc = ~seedCrc;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < target.Length; i++)
                {
                    crc = table[(byte)crc ^ target[i]] ^ (crc >> 8);
                }

                return ~crc;
            }
        }

        #endregion

        #region CRC-64

        private static readonly ConcurrentDictionary<ulong, ulong[]> crc64Tables =
            new ConcurrentDictionary<ulong, ulong[]>();

        /// <summary>
        /// The ISO polynomial, defined in ISO 3309 and used in HDLC.
        /// </summary>
        [CLSCompliant(false)]
        public const ulong Crc64IsoPolynomial = 0xD800000000000000;

        /// <summary>
        /// The ECMA polynomial, defined in ECMA 182.
        /// </summary>
        [CLSCompliant(false)]
        public const ulong Crc64EcmaPolynomial = 0xC96C5795D7870F42;

        private static ulong[] GenerateTableForCrc64(ulong polynomial)
        {
            var table = new ulong[256];
            for (uint i = 0; i < 256; i++)
            {
                ulong crc = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ polynomial;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }

                table[i] = crc;
            }
            return table;
        }

        /// <summary>
        /// Returns the CRC-64 checksum of data using the polynomial. Default implementation uses ISO polynomial.
        /// </summary>
        /// <param name="target">Array of bytes to be hashed.</param>
        /// <param name="polynomial">Polynomial.</param>
        /// <param name="seedCrc">Initial value for crc calculation, default is 0.</param>
        /// <returns>CRC-64 hash.</returns>
        [CLSCompliant(false)]
        public static ulong Crc64(byte[] target, ulong polynomial = Crc64EcmaPolynomial, ulong seedCrc = 0)
        {
            Guard.IsNotNull(target, nameof(target));

            // Get or generate table.
            var table = crc64Tables.GetOrAdd(polynomial, key => GenerateTableForCrc64(polynomial));

            // Hash calculation.
            unchecked
            {
                ulong crc = ~seedCrc;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < target.Length; i++)
                {
                    crc = table[(byte)crc ^ target[i]] ^ (crc >> 8);
                }
                return ~crc;
            }
        }

        #endregion
    }
}
