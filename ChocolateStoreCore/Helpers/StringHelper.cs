using ChocolateStoreCore.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static NuGet.Packaging.PackagingConstants;

namespace ChocolateStoreCore.Helpers
{
    public static class StringHelper
    {
        private static readonly Regex RxFileTypePattern = new(@"(?<=filetype\s*=\s{1}['""])[\w{3}]*(?=['""])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex RxUrlPattern = new(@"(?<=['""])http[\S]*(?=['""])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetFileTypen(string content)
        {
            var fileType = RxFileTypePattern.Match(content)?.Value?.ToLower();
            return fileType;
        }

        public static string ReplaceTokens(string input, string id, string version)
        {
            return input
                   .Replace("$PackageVersion", version, StringComparison.OrdinalIgnoreCase)
                   .Replace("$Version", version, StringComparison.OrdinalIgnoreCase)
                   .Replace("$PackageName", id, StringComparison.OrdinalIgnoreCase)
                   .Replace("${locale}", "en-US", StringComparison.OrdinalIgnoreCase)
                   ;
        }

        public static List<string> GetOriginalUrls(string content, string id, string version)
        {
            var downloads = new List<string>();
            var uris = Regex.Replace(content, StringHelper.RxUrlPattern.ToString(), new MatchEvaluator(m =>
            {
                var url = ReplaceTokens(m.Value, id, version);
                downloads.Add(url);
                return url;
            }), RegexOptions.IgnoreCase);
            return downloads;
        }
    }
}
