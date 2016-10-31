// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using NUnit.Framework;
    using Extensions;
    using Utils;

    /// <summary>
    /// Security tests.
    /// </summary>
    [TestFixture]
    public class SecurityTest
    {
#if !PORTABLE
        [Test]
        public void Test_all_hash_calls()
        {
            Assert.That(SecurityUtils.MD5("mypassword"), Is.EqualTo("34819d7beeabb9260a5c854bc85b3e44".ToUpper()));
            Assert.That(SecurityUtils.Sha1("mypassword"), Is.EqualTo("91dfd9ddb4198affc5c194cd8ce6d338fde470e2".ToUpper()));
            Assert.That(SecurityUtils.Sha256("mypassword"), Is.EqualTo("89e01536ac207279409d4de1e5253e01f4a1769e696db0d6062ca9b8f56767c8".ToUpper()));
            Assert.That(
                SecurityUtils.Sha384("mypassword"),
                Is.EqualTo("95b2d3b2ad7c2759bf3daa53424e2a472bc932798dae30b982621833a449492883b7ae9d31d30d32372f98abdbb256ae".ToUpper())
            );
            Assert.That(
                SecurityUtils.Sha512("mypassword"),
                Is.EqualTo("a336f671080fbf4f2a230f313560ddf0d0c12dfcf1741e49e8722a234673037dc493caa8d291d8025f71089d63cea809cc8ae53e5b17054806837dbe4099c4ca".ToUpper())
            );
            Assert.That(SecurityUtils.Crc32(@"This is test string."), Is.EqualTo(0xab6a9ba9));
        }

        [Test]
        public void Test_hash_string_should_contain_correct_method()
        {
            var hash = SecurityUtils.Hash("mypassword", SecurityUtils.HashMethods.Sha1);
            var isCorrect = SecurityUtils.CheckHash("mypassword", hash);
            Assert.That(isCorrect, Is.True);
        }
#endif
    }
}
