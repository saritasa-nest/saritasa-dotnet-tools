// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using NUnit.Framework;
    using Security;

    /// <summary>
    /// Security tests.
    /// </summary>
    [TestFixture]
    public class SecurityTests
    {
        [Test]
        public void Password_generator_with_specific_length_digits_should_match()
        {
            PasswordGenerator passwordGenerator = new PasswordGenerator(
                25,
                PasswordGenerator.CharacterClass.Digits,
                PasswordGenerator.GeneratorFlag.ShuffleChars
            );
            var password = passwordGenerator.Generate();
            Assert.That(password.Length, Is.EqualTo(25));

            PasswordGenerator passwordGenerator2 = new PasswordGenerator();
            passwordGenerator2.PasswordLength = 15;
            passwordGenerator2.SetCharactersPool("1");
            Assert.That(passwordGenerator2.Generate(), Is.EqualTo(new string('1', 15)));
        }

        [Test]
        public void Test_password_entropy_match()
        {
            PasswordGenerator passwordGenerator = new PasswordGenerator();
            passwordGenerator.PasswordLength = 10;
            passwordGenerator.SetCharactersPool("0123456789"); // 10 symbols
            var entropy = passwordGenerator.GetEntropy();
            Assert.That(entropy, Is.InRange(33.119, 33.319));
        }

        [Test]
        public void Test_password_strenght_matches()
        {
            Assert.That(PasswordGenerator.EstimatePasswordStrength("1111"), Is.EqualTo(0));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("2222222222"), Is.EqualTo(0));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("123456789"), Is.EqualTo(4));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("123456789A"), Is.EqualTo(73));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("123456789AB"), Is.EqualTo(75));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("CCCCCCCCC"), Is.EqualTo(0));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("8m6y2L2WhalkPDa"), Is.EqualTo(100));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("AA11bb00__"), Is.EqualTo(68));
            Assert.That(PasswordGenerator.EstimatePasswordStrength("123456_789AB"), Is.EqualTo(89));
        }
    }
}
