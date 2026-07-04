using System.Text.RegularExpressions;

namespace Wartsila.TestApp.TestConsole
{
    public static class ProblemB
    {
        public static void Tests()
        {
            // Base test
            Test("kaia", 1);
            Test("abcdefgded", 4);
            
            // Extra test
            Test("a", 0);
            Test("aa", 0);
            Test("ab", 1);
            Test("aba", 0);
            Test("abc", 1);
            Test("abcd", 2);
            Test("abcde", 2);
            Test("abcdef", 3);
            Test("abcdefg", 3);
            Test("zzzz", 0);
            Test("zzzy", 1);
            Test("abca", 1);
            Test("abcda", 1);
            Test("aab", 1);
            Test("aabc", 2);
            Test("abcba", 0);
            Test("abccbx", 1);
            Test("abcdefghij", 5);
            Test("abab", 1);
            Test("abac", 2);
            Test("abcab", 2);
            Test("aaaaab", 1);
        }

        public static void Invoke()
        {
            string? name = Console.ReadLine();

            if (IsValidInput(name))
            {
                int steps = StepsToPalindrome(name);
                Console.WriteLine(steps);
            }
        }

        /// <summary>
        /// Determines whether the specified value is already a palindrome.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        /// <c>true</c> when the value reads the same from left to right and right to left;
        /// otherwise, <c>false</c>. Returns <c>false</c> for <c>null</c>.
        /// </returns>
        public static bool IsPalindrome(this string? value)
        {
            if (value == null)
            {
                return false;
            }

            if (value.Length == 0)
            {
                return true;
            }

            int lastIndex = value.Length - 1;
            
            for (int i = 0, j = lastIndex; i < j; i++, j--)
            {
                if (value[i] != value[j])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calculates the minimum number of operations needed to transform the value into a palindrome.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>
        /// The minimum number of letter changes and/or letters appended to the end required to make the value a palindrome.
        /// </returns>
        public static int StepsToPalindrome(string? value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return 0;
            }

            int steps = StepsToPalindromeByReplace(value);

            int length = value.Length;
            for (int i = 1; i < length; i++)
            {
                value += "?";
                int stepsWithAdd = StepsToPalindromeByReplace(value);
                if (stepsWithAdd < steps)
                {
                    steps = stepsWithAdd;
                }
            }

            return steps;
        }

        /// <summary>
        /// Builds one possible palindrome using the same operation model as the step counter.
        /// </summary>
        /// <param name="value">The input value to transform.</param>
        /// <returns>
        /// A palindrome based on the input value, or <c>null</c> when the input value is <c>null</c>.
        /// </returns>
        public static string? ToPalindrome(this string? value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Length == 0)
            {
                return string.Empty;
            }

            const char wildСhar = '?';

            (string palindrome, int steps) = ToPalindromeByReplace(value, wildСhar);

            int length = value.Length;
            for (int i = 1; i < length; i++)
            {
                value += wildСhar;

                (string palindromeWithAdd, int stepsWithAdd) = ToPalindromeByReplace(value, wildСhar);

                // "replace" has priority then "add"
                if (stepsWithAdd < steps)
                {
                    palindrome = palindromeWithAdd;
                    steps = stepsWithAdd;
                }
            }

            return palindrome;
        }
        
        #region Private
        
        private static bool IsValidInput(string? value, string regex = @"^[a-z]{1,100}$")
        {
            bool isValid =
            (
                (value != null) &&
                (Regex.IsMatch(value, regex, RegexOptions.CultureInvariant))
            );

            return isValid;
        }

        private static void Assert(int expected, int result, string? input)
        {
            bool passed = (result == expected);

            if (passed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Test PASSED: expected=\"{expected}\", result=\"{result}\", input==\"{input}\".");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test FAILED: expected=\"{expected}\", result=\"{result}\", input==\"{input}\".");
            }
            
            Console.ResetColor();
        }

        private static void AssertTrue(bool result, string? input, string? comment)
        {
            bool passed = result;

            if (passed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Test PASSED: result=\"{result}\", input==\"{input}\", comment=\"{comment}\".");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test FAILED: result=\"{result}\", input==\"{input}\", comment=\"{comment}\".");
            }
            
            Console.ResetColor();
        }

        private static void Test(string input, int expected)
        {
            int steps = StepsToPalindrome(input);

            Assert(expected, steps, input);

            string? palindrome = input.ToPalindrome();
            bool isPalindrome = palindrome.IsPalindrome();
            
            AssertTrue(isPalindrome, input, palindrome);
        }
        
        private static int StepsToPalindromeByReplace(string value)
        {
            int lastIndex = value.Length - 1;
            int steps = 0;
            for (int i = 0, j = lastIndex; i < j; i++, j--)
            {
                if (value[i] != value[j])
                {
                    steps++;
                }
            }

            return steps;
        }

        private static (string Palindrome, int Steps) ToPalindromeByReplace(string value, char wildСhar)
        {
            char[] chars = value.ToCharArray();

            int lastIndex = chars.Length - 1;
            int steps = 0;

            for (int i = 0, j = lastIndex; i < j; i++, j--)
            {
                char left = chars[i];
                char right = chars[j];
                if (left != right)
                {
                    if (left == wildСhar)
                    {
                        chars[i] = right;
                    }
                    else
                    {
                        chars[j] = left;
                    }

                    steps++;
                }
            }

            return (new string(chars), steps);
        }

        #endregion
    }
}