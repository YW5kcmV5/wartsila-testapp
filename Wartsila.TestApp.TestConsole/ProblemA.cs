using System.Text;
using System.Text.RegularExpressions;

namespace Wartsila.TestApp.TestConsole
{
    public static class ProblemA
    {
        /// <summary>
        /// Defines the available strategies for compacting an Apaxian name.
        /// </summary>
        public enum Algorithm
        {
            /// <summary>
            /// Uses a single-pass linear scan and appends only characters that differ from the previous one.
            /// </summary>
            Optimized,
            
            /// <summary>
            /// Uses a regular expression to replace each run of repeated lowercase letters with one letter.
            /// </summary>
            Simple
        }
        
        /// <summary>
        /// Compacts an Apaxian name by replacing every consecutive run of the same letter with a single occurrence of that letter.
        /// </summary>
        /// <param name="value">
        /// The Apaxian name to compact. If <c>null</c>, the method returns <c>null</c>.
        /// </param>
        /// <param name="algorithm">
        /// The compaction strategy to use.
        /// </param>
        /// <returns>
        /// The compacted name, or <c>null</c> when <paramref name="value"/> is <c>null</c>.
        /// </returns>
        public static string? Compact(this string? value, Algorithm algorithm = Algorithm.Optimized)
        {
            switch (algorithm)
            {
                case Algorithm.Optimized:
                    return OptimizedCompact(value);
                
                case Algorithm.Simple:
                    return SimpleCompact(value);
                
                default:
                    throw new NotImplementedException($"Unknown algorithm type \"{algorithm}\".");
            }
        }

        public static void Tests()
        {
            Test("robert", "robert", Algorithm.Simple);
            Test("rooobert", "robert", Algorithm.Simple);
            Test("roooooobertapalaxxxxios", "robertapalaxios", Algorithm.Simple);
            
            Test("robert", "robert", Algorithm.Optimized);
            Test("rooobert", "robert", Algorithm.Optimized);
            Test("roooooobertapalaxxxxios", "robertapalaxios", Algorithm.Optimized);
        }

        public static void Invoke()
        {
            string? name = Console.ReadLine();

            if (IsValidInput(name))
            {
                name = name.Compact();
                Console.WriteLine(name);
            }
        }
        
        #region Private
        
        private static bool IsValidInput(string? value, string regex = @"^[a-z]{1,250}$")
        {
            bool isValid =
            (
                (value != null) &&
                (Regex.IsMatch(value, regex, RegexOptions.CultureInvariant))
            );

            return isValid;
        }

        private static void Assert(string? expected, string? result, string? input)
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

        private static void Test(string input, string expected, Algorithm algorithm)
        {
            string? output = input.Compact(algorithm);

            Assert(expected, output, input);
        }

        private static string? OptimizedCompact(this string? value)
        {
            if (value == null)
            {
                return null;
            }
            
            var sb = new StringBuilder();
            
            int length = value.Length;
            char? prev = null;
                
            for (int i = 0; i < length; i++)
            {
                char current = value[i];
                if (current != prev)
                {
                    sb.Append(current);
                }

                prev = current;
            }

            return sb.ToString();
        }

        private static string? SimpleCompact(this string? value)
        {
            return (!string.IsNullOrWhiteSpace(value))
                ? Regex.Replace(value, @"([a-z])\1+", "$1")
                : null;
        }
        
        #endregion
    }
}