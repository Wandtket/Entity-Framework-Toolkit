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

        public static string Between(this string text, string start, string end, BetweenOptions options = BetweenOptions.KeepStartEnd)
        {
            var startIndex = text.IndexOf(start);
            var endIndex = text.IndexOf(end);

            //var endIndex = text.LastIndexOf(end);


            if (options == BetweenOptions.KeepStartEnd)
                return text.Substring(startIndex, (endIndex + end.Length) - startIndex);
            else if (options == BetweenOptions.KeepStart)
                return text.Substring(startIndex, (endIndex + end.Length) - startIndex).ReplaceFirstOccurrence(end, "");
            else if (options == BetweenOptions.KeepEnd)
                return text.Substring(startIndex, (endIndex + end.Length) - startIndex).ReplaceFirstOccurrence(start, "");
            else
                return text.Substring(startIndex + start.Length, endIndex - (startIndex + start.Length));        
        }


        public enum BetweenOptions
        {
            KeepStartEnd,
            KeepStart,
            KeepEnd,
            KeepNone,
        }


        public static string ReplaceFirstOccurrence(this string String, string find, string replace)
        {
            int pos = String.IndexOf(find);
            if (pos < 0)
            {
                return String;
            }
            return String.Substring(0, pos) + replace + String.Substring(pos + find.Length);
        }

        public static string ReplaceLastOccurrence(this string String, string find, string replace)
        {
            int pos = String.LastIndexOf(find);

            if (pos == -1)
                return String;

            return String.Remove(pos, find.Length).Insert(pos, replace);
        }


        public static string? FirstCharToLowerCase(this string? String)
        {
            if (!string.IsNullOrEmpty(String) && char.IsUpper(String[0]))
                return String.Length == 1 ? char.ToLower(String[0]).ToString() : char.ToLower(String[0]) + String[1..];

            return String;
        }


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
