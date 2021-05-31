using System;
using System.Linq;

namespace TourmalineCore.AspNetCore.Pagination.Extensions
{
    internal static class StringExtensions
    {
        public static string NormalizeString(this string str)
        {
            var trimmedStr = str?.Trim();

            return string.IsNullOrWhiteSpace(trimmedStr)
                ? string.Empty
                : trimmedStr;
        }

        public static string FirstCharToUpper(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : input.First().ToString().ToUpper() + input[1..];
        }
    }
}