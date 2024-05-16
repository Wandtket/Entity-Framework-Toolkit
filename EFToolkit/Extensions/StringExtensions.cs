using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EFToolkit.Extensions
{
    public static class StringExtensions
    {

        public static IEnumerable<int> AllIndicesOf(this string text, string pattern)
        {
            int M = pattern.Length;
            int N = text.Length;

            int[] lps = LongestPrefixSuffix(pattern);
            int i = 0, j = 0;

            while (i < N)
            {
                if (pattern[j] == text[i])
                {
                    j++;
                    i++;
                }
                if (j == M)
                {
                    yield return i - j;
                    j = lps[j - 1];
                }

                else if (i < N && pattern[j] != text[i])
                {
                    if (j != 0)
                    {
                        j = lps[j - 1];
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        private static int[] LongestPrefixSuffix(string pattern)
        {
            int[] lps = new int[pattern.Length];
            int length = 0;
            int i = 1;

            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[length])
                {
                    length++;
                    lps[i] = length;
                    i++;
                }
                else
                {
                    if (length != 0)
                    {
                        length = lps[length - 1];
                    }
                    else
                    {
                        lps[i] = length;
                        i++;
                    }
                }
            }
            return lps;
        }


        public static string ConvertToSnakeCase(this string text)
        {
            return Regex.Replace(text, @"(\p{Ll})(\p{Lu})", "$1_$2").ToUpperInvariant();
        }

        public static string ConvertToCamelCase(this string text)
        {
            var words = text.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = words[0].ToLower();
            var tailWords = words.Skip(1)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .ToArray();

            return $"{leadWord}{string.Join(string.Empty, tailWords)}";
        }
    }
}
