// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Security
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
#if !PORTABLE
    using System.Security.Cryptography;
#endif

    /// <summary>
    /// Class to be used for password generation.
    /// </summary>
    public class PasswordGenerator
    {
        /// <summary>
        /// Enum specifies what characters to use for password generation.
        /// </summary>
        [Flags]
        public enum CharacterClass
        {
            /// <summary>
            /// Lower letters.
            /// </summary>
            LowerLetters = 0x1,

            /// <summary>
            /// Upper letters.
            /// </summary>
            UpperLetters = 0x2,

            /// <summary>
            /// Digits.
            /// </summary>
            Digits = 0x4,

            /// <summary>
            /// Special characters except space.
            /// </summary>
            SpecialCharacters = 0x8,

            /// <summary>
            /// Space character.
            /// </summary>
            Space = 0x16,

            /// <summary>
            /// Combination of LowerLetters and UpperLetters.
            /// </summary>
            AllLetters = LowerLetters | UpperLetters,

            /// <summary>
            /// Combination of AllLetters and Digits.
            /// </summary>
            AlphaNumeric = AllLetters | Digits,

            /// <summary>
            /// Combination of all elements.
            /// </summary>
            All = LowerLetters | UpperLetters | Digits | SpecialCharacters | Space,
        }

        /// <summary>
        /// Enums specifies special processing for password generation.
        /// </summary>
        [Flags]
        public enum GeneratorFlag
        {
            /// <summary>
            /// No special generation flags.
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Exclude conflict characters.
            /// </summary>
            ExcludeLookAlike = 0x1,

            /// <summary>
            /// Shuffle pool characters before generation.
            /// </summary>
            ShuffleChars = 0x2,

            /// <summary>
            /// Makes secure string as read only.
            /// </summary>
            MakeReadOnly = 0x4,
        }

        /// <summary>
        /// Password length. Default is 10.
        /// </summary>
        public int PasswordLength { get; set; }

        /// <summary>
        /// Character classes for password generation. Default is All.
        /// </summary>
        public CharacterClass CharacterClasses { get; set; }

        /// <summary>
        /// Generator flags. Default is None.
        /// </summary>
        public GeneratorFlag GeneratorFlags { get; set; }

        /// <summary>
        /// Instance of random generation class.
        /// </summary>
#if !PORTABLE
        public static RandomNumberGenerator RandomService { get; protected set; }
#else
        public static Random RandomService { get; protected set; }
#endif

        /// <summary>
        /// Lock object for RandomService.
        /// </summary>
        private static readonly object RandomServiceLock = new object();

        /// <summary>
        /// Characters pool that is use for password generation.
        /// </summary>
        private string CharactersPool { get; set; }

        /// <summary>
        /// Lower case characters pool.
        /// </summary>
        protected const string PoolLowerCase = "abcdefghjkmnpqrstuvwxyz";

        /// <summary>
        /// Lower case conflict characters pool.
        /// </summary>
        protected const string PoolLowerCaseConflict = "ilo";

        /// <summary>
        /// Upper case characters pool.
        /// </summary>
        protected const string PoolUpperCase = "ABCDEFGHJKLMNPQRSTUVWXYZ";

        /// <summary>
        /// Upper case characters conflict pool.
        /// </summary>
        protected const string PoolUpperCaseConflict = "OI";

        /// <summary>
        /// Digits characters pool.
        /// </summary>
        protected const string PoolDigits = "23456789";

        /// <summary>
        /// Digits conflict characters pool.
        /// </summary>
        protected const string PoolDigitsConflict = "10";

        /// <summary>
        /// Special characters pool.
        /// </summary>
        protected const string PoolSpecial = @"~@#$%^&*()_-+=[]|\:;""'<>.?/";

        /// <summary>
        /// Special characters conflict pool.
        /// </summary>
        protected const string PoolSpecialConflict = @"`{}!,";

        /// <summary>
        /// Space character.
        /// </summary>
        protected const string PoolSpace = " ";

        /// <summary>
        /// .ctor
        /// </summary>
        public PasswordGenerator()
        {
            this.PasswordLength = 10;
            this.CharacterClasses = CharacterClass.All;
            this.GeneratorFlags = GeneratorFlag.None;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="passwordLength">Password length to generate.</param>
        /// <param name="characterClasses">What characters classes to use.</param>
        /// <param name="generatorFlags">Special generator flags.</param>
        public PasswordGenerator(int passwordLength, CharacterClass characterClasses, GeneratorFlag generatorFlags) : this()
        {
            if (passwordLength < 2)
            {
                throw new ArgumentException("Password length should be at least 2 characters");
            }

            this.PasswordLength = passwordLength;
            this.CharacterClasses = characterClasses;
            this.GeneratorFlags = generatorFlags;
        }

        /// <summary>
        /// .cctor
        /// </summary>
        static PasswordGenerator()
        {
#if !PORTABLE
            RandomService = System.Security.Cryptography.RandomNumberGenerator.Create();
#else
            RandomService = new Random();
#endif
        }

        /// <summary>
        /// Set characters pool for password generation. If set it will be used for password generation.
        /// Instead default characters pool will be generated.
        /// </summary>
        /// <param name="pool"></param>
        public void SetCharactersPool(string pool)
        {
            if (string.IsNullOrEmpty(pool))
            {
                throw new ArgumentException("Characters pool cannot be empty");
            }
            this.CharactersPool = pool;
        }

        /// <summary>
        /// Generates custom characters pool. The pool set by SetCharactersPool will be ignored.
        /// </summary>
        public void UseDefaultCharactersPool()
        {
            this.CharactersPool = string.Empty;
        }

        /// <summary>
        /// Generates new password to String.
        /// </summary>
        /// <returns>Password.</returns>
        public String Generate()
        {
            var pool = string.IsNullOrEmpty(this.CharactersPool) ? CreateCharactersPool() : this.CharactersPool.ToCharArray();
            StringBuilder sb = new StringBuilder(PasswordLength);

            if (this.GeneratorFlags.HasFlag(GeneratorFlag.ShuffleChars))
            {
                ShuffleCharsArray(pool);
            }

            for (int i = 0; i < PasswordLength; i++)
            {
                int random = GetNextRandom(pool.Length);
                sb.Append(pool[random]);
            }

            return sb.ToString();
        }

#if !PORTABLE && !NETCOREAPP1_0 && !NETSTANDARD1_6
        /// <summary>
        /// Generates new password to SecureString.
        /// </summary>
        /// <returns>Password.</returns>
        public SecureString GenerateSecure()
        {
            var pool = string.IsNullOrEmpty(this.CharactersPool) ? CreateCharactersPool() : this.CharactersPool.ToCharArray();
            var secureString = new SecureString();

            if (this.GeneratorFlags.HasFlag(GeneratorFlag.ShuffleChars))
            {
                ShuffleCharsArray(pool);
            }

            for (int i = 0; i < PasswordLength; i++)
            {
                int random = GetNextRandom(pool.Length);
                secureString.AppendChar(pool[random]);
            }

            if (GeneratorFlags.HasFlag(GeneratorFlag.MakeReadOnly))
            {
                secureString.MakeReadOnly();
            }

            return secureString;
        }
#endif

        /// <summary>
        /// Estimate password strength. See documentation for more details.
        /// </summary>
        /// <param name="password">Password to estimate.</param>
        /// <returns>Estimate score.</returns>
        public static int EstimatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty");
            }

            var passwordLower = password.ToLowerInvariant();
            int score = 0;
            int alphasUpperCount = 0, alphasLowerCount = 0, digitsCount = 0, symbolsCount = 0, middleCharsCount = 0,
                requirements = 0, alphasOnlyCount = 0, numbersOnlyCount = 0, uniqueCharsCount = 0, repeatCharsCount = 0,
                consequenceAlphasUpperCount = 0, consequenceAlphasLowerCount = 0, consequenceDigitsCount = 0, consequenceSymbolsCount = 0, consequenceCharsTypeCount = 0,
                sequenceAlphasCount = 0, sequenceNumbersCount = 0, sequenceSymbolsCount = 0, sequenceCharsCount = 0, requiredCharsCount = 0;
            double repeatIncrement = 0;
            int tempAlphaUpperIndex = -1, tempAlphaLowerIndex = -1, tempNumberIndex = -1, tempSymbolIndex = -1;

            const int FactorMiddleChar = 2, FactorConsequenceAlphaUpper = 2, FactorConsequenceAlphaLower = 2, FactorConsequenceNumber = 2;
            const int FactorSequenceAlpha = 3, FactorSequenceNumber = 3, FactorSequenceSymbol = 3;
            const int FactorLength = 4, FactorNumber = 4;
            const int FactorSymbol = 6;
            const string PoolAlphas = "abcdefghijklmnopqrstuvwxyz";
            const string PoolNumerics = "01234567890";
            const string PoolSymbols = ")!@#$%^&*()";
            const int MinimumPasswordLength = 8;

            score = password.Length * FactorLength;

            // loop through password to check for Symbol, Numeric, Lowercase and Uppercase pattern matches
            for (var a = 0; a < password.Length; a++)
            {
                if (char.IsUpper(password[a]))
                {
                    if (tempAlphaUpperIndex > -1 && tempAlphaUpperIndex + 1 == a)
                    {
                        consequenceAlphasUpperCount++;
                        consequenceCharsTypeCount++;
                    }
                    tempAlphaUpperIndex = a;
                    alphasUpperCount++;
                }
                else if (char.IsLower(password[a]))
                {
                    if (tempAlphaLowerIndex > -1 && tempAlphaLowerIndex + 1 == a)
                    {
                        consequenceAlphasLowerCount++;
                        consequenceCharsTypeCount++;
                    }
                    tempAlphaLowerIndex = a;
                    alphasLowerCount++;
                }
                else if (char.IsNumber(password[a]))
                {
                    if (a > 0 && a < password.Length - 1)
                    {
                        middleCharsCount++;
                    }
                    if (tempNumberIndex > -1 && tempNumberIndex + 1 == a)
                    {
                        consequenceDigitsCount++;
                        consequenceCharsTypeCount++;
                    }
                    tempNumberIndex = a;
                    digitsCount++;
                }
                else if (Regex.IsMatch(password[a].ToString(), @"[^a-zA-Z0-9_]"))
                {
                    if (a > 0 && a < (password.Length - 1))
                    {
                        middleCharsCount++;
                    }
                    if (tempSymbolIndex > -1 && tempSymbolIndex + 1 == a)
                    {
                        consequenceSymbolsCount++;
                        consequenceCharsTypeCount++;
                    }
                    tempSymbolIndex = a;
                    symbolsCount++;
                }
                // internal loop through password to check for repeat characters
                var charExists = false;
                for (var b = 0; b < password.Length; b++)
                {
                    // repeat character exists
                    if (password[a] == password[b] && a != b)
                    {
                        charExists = true;
                        /* Calculate icrement deduction based on proximity to identical characters
                           Deduction is incremented each time a new match is discovered
                           Deduction amount is based on total password length divided by the
                           difference of distance between currently selected match */
                        repeatIncrement += Math.Abs(password.Length / (b - a));
                    }
                }
                if (charExists)
                {
                    repeatCharsCount++;
                    uniqueCharsCount = password.Length - repeatCharsCount;
                    repeatIncrement = uniqueCharsCount > 0 ? Math.Ceiling(repeatIncrement / uniqueCharsCount) : Math.Ceiling(repeatIncrement);
                }
            }

            // check for sequential alpha string patterns (forward and reverse)
            for (var s = 0; s < PoolAlphas.Length - 3; s++)
            {
                var forward = PoolAlphas.Substring(s, 3);
                var reverse = Reverse(forward);
                if (passwordLower.IndexOf(forward) != -1 || passwordLower.IndexOf(reverse) != -1)
                {
                    sequenceAlphasCount++;
                    sequenceCharsCount++;
                }
            }

            // check for sequential numeric string patterns (forward and reverse)
            for (var s = 0; s < PoolNumerics.Length - 3; s++)
            {
                var forward = PoolNumerics.Substring(s, 3);
                var reverse = Reverse(forward);
                if (passwordLower.IndexOf(forward) != -1 || passwordLower.IndexOf(reverse) != -1)
                {
                    sequenceNumbersCount++;
                    sequenceCharsCount++;
                }
            }

            // check for sequential symbol string patterns (forward and reverse)
            for (var s = 0; s < PoolSymbols.Length - 3; s++)
            {
                var forward = PoolSymbols.Substring(s, 3);
                var reverse = Reverse(forward);
                if (passwordLower.IndexOf(forward) != -1 || passwordLower.IndexOf(reverse) != -1)
                {
                    sequenceSymbolsCount++;
                    sequenceCharsCount++;
                }
            }

            // Modify overall score value based on usage vs requirements //

            // General point assignment
            if (alphasUpperCount > 0 && alphasUpperCount < password.Length)
            {
                score += (password.Length - alphasUpperCount) * 2;
            }
            if (alphasLowerCount > 0 && alphasLowerCount < password.Length)
            {
                score += (password.Length - alphasLowerCount) * 2;
            }
            if (digitsCount > 0 && digitsCount < password.Length)
            {
                score += digitsCount * FactorNumber;
            }
            if (symbolsCount > 0)
            {
                score += symbolsCount * FactorSymbol;
            }
            if (middleCharsCount > 0)
            {
                score += middleCharsCount * FactorMiddleChar;
            }

            // Point deductions for poor practices //

            // only Letters
            if ((alphasLowerCount > 0 || alphasUpperCount > 0) && symbolsCount == 0 && digitsCount == 0)
            {
                score = score - password.Length;
                alphasOnlyCount = password.Length;
            }
            // only Numbers
            if (alphasLowerCount == 0 && alphasUpperCount == 0 && symbolsCount == 0 && digitsCount > 0)
            {
                score = score - password.Length;
                numbersOnlyCount = password.Length;
            }
            // same character exists more than once
            if (repeatCharsCount > 0)
            {
                score = (int)(score - repeatIncrement);
            }
            // consecutive uppercase letters exist
            if (consequenceAlphasUpperCount > 0)
            {
                score -= consequenceAlphasUpperCount * FactorConsequenceAlphaUpper;
            }
            // consecutive lowercase letters exist
            if (consequenceAlphasLowerCount > 0)
            {
                score -= consequenceAlphasLowerCount * FactorConsequenceAlphaLower;
            }
            // consecutive numbers exist
            if (consequenceDigitsCount > 0)
            {
                score -= consequenceDigitsCount * FactorConsequenceNumber;
            }
            // sequential alpha strings exist (3 characters or more)
            if (sequenceAlphasCount > 0)
            {
                score -= sequenceAlphasCount * FactorSequenceAlpha;
            }
            // sequential numeric strings exist (3 characters or more)
            if (sequenceNumbersCount > 0)
            {
                score -= sequenceNumbersCount * FactorSequenceNumber;
            }
            // sequential symbol strings exist (3 characters or more)
            if (sequenceSymbolsCount > 0)
            {
                score -= sequenceSymbolsCount * FactorSequenceSymbol;
            }

            // determine if mandatory requirements have been met and set image indicators accordingly
            var arrChars = new int[] { password.Length, alphasUpperCount, alphasLowerCount, digitsCount, symbolsCount };
            for (var c = 0; c < arrChars.Length; c++)
            {
                var minValue = 0;
                // password length
                if (c == 0)
                {
                    minValue = MinimumPasswordLength - 1;
                }
                else
                {
                    minValue = 0;
                }
                if (arrChars[c] == minValue + 1)
                {
                    requiredCharsCount++;
                }
                else if (arrChars[c] > minValue + 1)
                {
                    requiredCharsCount++;
                }
            }
            requirements = requiredCharsCount;
            int minRequiredChars = password.Length >= MinimumPasswordLength ? 3 : 4;
            // one or more required characters exist
            if (requirements > minRequiredChars)
            {
                score += requirements * 2;
            }

            score = score > 100 ? 100 : score;
            score = score < 0 ? 0 : score;

            return score;
        }

        /// <summary>
        /// Get password entropy.
        /// </summary>
        /// <returns>Password's entropy.</returns>
        public double GetEntropy()
        {
            var pool = string.IsNullOrEmpty(this.CharactersPool) ? CreateCharactersPool() : this.CharactersPool.ToCharArray();
            return Math.Log(Math.Pow(this.PasswordLength, pool.Length), 2);
        }

        private char[] CreateCharactersPool()
        {
            var chars = new List<char>(65);

            if (CharacterClasses.HasFlag(CharacterClass.UpperLetters))
            {
                chars.AddRange(PoolUpperCase.ToCharArray());
                if (GeneratorFlags.HasFlag(GeneratorFlag.ExcludeLookAlike) == false)
                {
                    chars.AddRange(PoolUpperCaseConflict.ToCharArray());
                }
            }
            if (this.CharacterClasses.HasFlag(CharacterClass.LowerLetters))
            {
                chars.AddRange(PoolLowerCase.ToCharArray());
                if (GeneratorFlags.HasFlag(GeneratorFlag.ExcludeLookAlike) == false)
                {
                    chars.AddRange(PoolLowerCaseConflict.ToCharArray());
                }
            }
            if (this.CharacterClasses.HasFlag(CharacterClass.Digits))
            {
                chars.AddRange(PoolDigits.ToCharArray());
                if (GeneratorFlags.HasFlag(GeneratorFlag.ExcludeLookAlike) == false)
                {
                    chars.AddRange(PoolDigitsConflict.ToCharArray());
                }
            }
            if (this.CharacterClasses.HasFlag(CharacterClass.SpecialCharacters))
            {
                chars.AddRange(PoolSpecial.ToCharArray());
                if (GeneratorFlags.HasFlag(GeneratorFlag.ExcludeLookAlike) == false)
                {
                    chars.AddRange(PoolSpecialConflict.ToCharArray());
                }
            }
            if (CharacterClasses.HasFlag(CharacterClass.Space))
            {
                chars.AddRange(PoolSpace.ToCharArray());
            }

            return chars.ToArray();
        }

        private static void ShuffleCharsArray(char[] chars)
        {
            for (int i = chars.Length - 1; i >= 1; i--)
            {
                int j = GetNextRandom(i + 1);
                var tmp = chars[i];
                chars[i] = chars[j];
                chars[j] = tmp;
            }
        }

        /// <summary>
        /// Get next random number using RandomService.
        /// </summary>
        /// <param name="maxValue">Maximum value for number.</param>
        /// <returns>The random number between zero and maxValue.</returns>
        private static int GetNextRandom(int maxValue)
        {
#if !PORTABLE
            byte[] bytes = new byte[4];
            lock (RandomServiceLock)
            {
                RandomService.GetBytes(bytes);
            }
            return (int)Math.Round(((double)BitConverter.ToUInt32(bytes, 0) / uint.MaxValue) * (maxValue - 1));
#else
            return RandomService.Next(maxValue);
#endif
            }

        private static string Reverse(string target)
        {
            var arr = target.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
    }
}
