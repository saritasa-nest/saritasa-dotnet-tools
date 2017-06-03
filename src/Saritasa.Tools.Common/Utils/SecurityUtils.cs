// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
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
        /// Returns string's MD5 hash (PHP-compatible).
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

        /// <summary>
        /// Convert array of bytes to string representation.
        /// </summary>
        /// <param name="bytes">Bytes array.</param>
        /// <returns>Hex string.</returns>
        public static string ConvertBytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
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
        }

        private static readonly IDictionary<HashMethod, Func<string, byte[]>> methodsFuncMap =
            new Dictionary<HashMethod, Func<string, byte[]>>
            {
                [HashMethod.Md5] = MD5,
                [HashMethod.Sha1] = Sha1,
                [HashMethod.Sha256] = Sha256,
                [HashMethod.Sha384] = Sha384,
                [HashMethod.Sha512] = Sha512,
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
            return method.ToString().ToUpperInvariant() + PasswordMethodHashSeparator + methodsFuncMap[method](target);
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

            return Hash(target, method) == hashedStringToCheck;
        }
#endif
    }
}
