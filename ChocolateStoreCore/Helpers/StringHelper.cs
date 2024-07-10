using ChocolateStoreCore.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static NuGet.Packaging.PackagingConstants;

namespace ChocolateStoreCore.Helpers
{
    public static class StringHelper
    {
        private static readonly Regex RxFileTypePattern = new(@"(?<=filetype\s*=\s{1}['""])[\w{3}]*(?=['""])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex RxUrlPattern = new(@"(?<=['""])http[\S]*(?=['""])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetFileType(string content)
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

        public static string ReplaceTokensByVariables(string input)
        {
            StringBuilder updatedFileContent = new StringBuilder();
            Dictionary<string, string> variables = new Dictionary<string, string>();
            string varPattern = @"^\$(\w+)\s*=\s*(.*)$";
            string urlPattern = @"https?://\S+";

            foreach (string line in input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string updatedLine = line;
                Match varMatch = Regex.Match(line, varPattern);

                if (varMatch.Success)
                {
                    string variableName = varMatch.Groups[1].Value;
                    string variableValue = varMatch.Groups[2].Value.Trim('\'', '"');

                    variables[variableName] = variableValue;

                    if (Regex.IsMatch(variableValue, urlPattern))
                    {
                         foreach (var kvp in variables)
                        {
                            variableValue = variableValue.Replace("$" + kvp.Key, kvp.Value);
                        }

                        updatedLine = $"${variableName} = \"{variableValue}\"";
                    }
                }

                updatedFileContent.AppendLine(updatedLine);
            }

            return updatedFileContent.ToString();
        }

        public static List<string> GetOriginalUrls(string content, string id, string version, string notToReplaceUrl)
        {
            var downloads = new List<string>();
            var uris = Regex.Replace(content, StringHelper.RxUrlPattern.ToString(), new MatchEvaluator(m =>
            {
                var url = ReplaceTokens(m.Value, id, version);
                if (string.IsNullOrEmpty(notToReplaceUrl) || !url.StartsWith(notToReplaceUrl))
                {
                    downloads.Add(url);
                }
                return url;
            }), RegexOptions.IgnoreCase);
            return downloads;
        }

        public static string GetPathWithLocal(string root, string path)
        {
            if (!string.IsNullOrEmpty(path) && path[..1] == ".")
            {
                return Path.Combine(root, path[1..].TrimStart('/', '\\'));
            }
            return path;
        }

        public static string GetPackageIdFromString(string packageId)
        {
            if (packageId.Contains(' '))
            {
                return packageId.ToLowerInvariant().Substring(0, packageId.IndexOf(' '));
            }
            return packageId;
        }

        public static string GetVersionFromString(string packageId)
        {
            if (packageId.Contains(' '))
            {
                return packageId.ToLowerInvariant().Substring(packageId.IndexOf(' ') + 1, packageId.Length - packageId.IndexOf(' ') - 1);
            }
            return String.Empty;
        }
    }
}
