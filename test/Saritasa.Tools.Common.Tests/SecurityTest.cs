// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Xunit;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// Security tests.
    /// </summary>
    public class SecurityTest
    {
        [Fact]
        public void Validate_md5_hash()
        {
            Assert.Equal("34819d7beeabb9260a5c854bc85b3e44".ToUpperInvariant(),
                SecurityUtils.ConvertBytesToString(SecurityUtils.MD5("mypassword")));
        }

        [Fact]
        public void Validate_sha1_hash()
        {
            Assert.Equal("91dfd9ddb4198affc5c194cd8ce6d338fde470e2".ToUpperInvariant(),
                SecurityUtils.ConvertBytesToString(SecurityUtils.Sha1("mypassword")));
        }

        [Fact]
        public void Validate_sha2_hash()
        {
            Assert.Equal("89e01536ac207279409d4de1e5253e01f4a1769e696db0d6062ca9b8f56767c8".ToUpperInvariant(),
                SecurityUtils.ConvertBytesToString(SecurityUtils.Sha256("mypassword")));
        }

        [Fact]
        public void Validate_sha382_hash()
        {
            Assert.Equal(
                "95b2d3b2ad7c2759bf3daa53424e2a472bc932798dae30b982621833a449492883b7ae9d31d30d32372f98abdbb256ae".ToUpperInvariant(),
                SecurityUtils.ConvertBytesToString(SecurityUtils.Sha384("mypassword"))
            );
        }

        [Fact]
        public void Validate_sha514_hash()
        {
            Assert.Equal(
                "a336f671080fbf4f2a230f313560ddf0d0c12dfcf1741e49e8722a234673037dc493caa8d291d8025f71089d63cea809cc8ae53e5b17054806837dbe4099c4ca".ToUpperInvariant(),
                SecurityUtils.ConvertBytesToString(SecurityUtils.Sha512("mypassword"))
            );
        }

        [Theory]
        [InlineData("mypassword", SecurityUtils.HashMethod.Sha1)]
        [InlineData("mypassword", SecurityUtils.HashMethod.Sha256)]
        [InlineData("mypassword", SecurityUtils.HashMethod.Md5)]
        [InlineData("mypassword", SecurityUtils.HashMethod.Sha384)]
        [InlineData("mypassword", SecurityUtils.HashMethod.Pbkdf2Sha1)]
        public void Test_hash_string_should_contain_correct_method(string target, SecurityUtils.HashMethod method)
        {
            // Act
            var hash = SecurityUtils.Hash(target, method);

            // Assert
            var isCorrect = SecurityUtils.CheckHash(target, hash);
            Assert.True(isCorrect);
        }

        [Fact]
        public void Convert_BytesToString_should_match_StringToBytes()
        {
            // Arrange
            var bytes = new byte[] { 0x12, 0x64, 0x12, 0x05, 0x64, 0x53, 0x34, 0x77, 0x66, 0x33, 0x12, 0x64, 0x12, 0x05, 0x64,
                0x53, 0x34, 0x77, 0x66, 0x33 };

            // Act
            var str1 = SecurityUtils.ConvertBytesToString(bytes);
            var bytes1 = SecurityUtils.ConvertStringToBytes(str1);

            // Assert
            Assert.Equal(bytes, bytes1);
        }

        [Fact]
        public void Test_crc32_Ieee()
        {
            // Arrange & act
            var crc = SecurityUtils.Crc32(Encoding.ASCII.GetBytes("1234567"), SecurityUtils.Crc32Ieee);

            // Assert
            Assert.Equal(0x5003699fu, crc);
        }

        [Fact]
        public void Test_crc32_Castagnoli()
        {
            // Arrange & act
            var crc = SecurityUtils.Crc32(Encoding.ASCII.GetBytes("1234567"), SecurityUtils.Crc32Castagnoli);

            // Assert
            Assert.Equal(0x124297eau, crc);
        }

        [Fact]
        public void Test_crc32_Koopman()
        {
            // Arrange & act
            var crc = SecurityUtils.Crc32(Encoding.ASCII.GetBytes("1234567"), SecurityUtils.Crc32Koopman);

            // Assert
            Assert.Equal(0xf058755u, crc);
        }

        [Fact]
        public void Test_crc64_Iso()
        {
            // Arrange & act
            var output = SecurityUtils.Crc64(Encoding.ASCII.GetBytes("1234567"), SecurityUtils.Crc64IsoPolynomial);

            // Assert
            Assert.Equal(0x46c775a41001b320u, output);
        }

        [Fact]
        public void Test_crc64_Ecma()
        {
            // Arrange & act
            var output = SecurityUtils.Crc64(Encoding.ASCII.GetBytes("1234567"), SecurityUtils.Crc64EcmaPolynomial);

            // Assert
            Assert.Equal(0x70509f3661923da0u, output);
        }
    }
}
