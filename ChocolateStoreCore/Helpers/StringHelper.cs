using System.Text;
using System.Text.RegularExpressions;

namespace ChocolateStoreCore.Helpers
{
    public static class StringHelper
    {
        private static readonly Regex RxFileTypePattern = new(@"(?<=filetype\s*=\s{1}['""])[\w{3}]*(?=['""])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex RxUrlPattern = new(@"(?<=['""])http[\S]*(?=['""])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxPackageVersionExpression = new(@"^\s*\[version\]\(\[regex\]'(?<pattern>[^']+)'\)\.Match\(\$Env:chocolateyPackageVersion\)\.Value\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxPackageSelector = new(@"^(?<id>[^\s:]+)(?:[\s:]+(?<version>.+))?$", RegexOptions.Compiled);

        public static string GetFileType(string content)
        {
            var fileType = RxFileTypePattern.Match(content)?.Value?.ToLower();
            return fileType;
        }

        public static string ReplaceTokens(string input, string id, string version)
        {
            var result = input;

            if (!string.IsNullOrWhiteSpace(version))
            {
                result = result
                    .Replace("$PackageVersion", version, StringComparison.OrdinalIgnoreCase)
                    .Replace("$Version", version, StringComparison.OrdinalIgnoreCase)
                    .Replace("$Env:chocolateyPackageVersion", version, StringComparison.OrdinalIgnoreCase)
                    .Replace("$Env:ChocolateyPackageVersion", version, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                result = result
                    .Replace("$PackageName", id, StringComparison.OrdinalIgnoreCase)
                    .Replace("$Env:chocolateyPackageName", id, StringComparison.OrdinalIgnoreCase)
                    .Replace("$Env:ChocolateyPackageName", id, StringComparison.OrdinalIgnoreCase);
            }

            return result.Replace("${locale}", "en-US", StringComparison.OrdinalIgnoreCase);
        }

        public static string ReplaceTokensByVariables(string input)
        {
            return ReplaceTokensByVariables(input, string.Empty, string.Empty);
        }

        public static string ReplaceTokensByVariables(string input, string id, string version)
        {
            StringBuilder updatedFileContent = new StringBuilder();
            Dictionary<string, string> variables = new(StringComparer.OrdinalIgnoreCase);
            string varPattern = @"^\$(\w+)\s*=\s*(.*)$";
            string urlPattern = @"https?://\S+";

            foreach (string line in input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string updatedLine = line;
                Match varMatch = Regex.Match(line, varPattern);

                if (varMatch.Success)
                {
                    string variableName = varMatch.Groups[1].Value;
                    string variableValue = varMatch.Groups[2].Value.Trim();
                    string resolvedValue = ResolveVariableValue(variableValue, variables, id, version);

                    variables[variableName] = resolvedValue;

                    if (Regex.IsMatch(resolvedValue, urlPattern, RegexOptions.IgnoreCase))
                    {
                        updatedLine = $"${variableName} = \"{resolvedValue}\"";
                    }
                }

                updatedFileContent.AppendLine(updatedLine);
            }

            return updatedFileContent.ToString();
        }

        private static string ResolveVariableValue(string variableValue, Dictionary<string, string> variables, string id, string version)
        {
            string resolvedValue = variableValue.Trim('"', '\'');

            var versionExpressionMatch = RxPackageVersionExpression.Match(resolvedValue);
            if (versionExpressionMatch.Success && !string.IsNullOrWhiteSpace(version))
            {
                var extractedVersion = Regex.Match(version, versionExpressionMatch.Groups["pattern"].Value).Value;
                if (!string.IsNullOrWhiteSpace(extractedVersion))
                {
                    return extractedVersion;
                }
            }

            resolvedValue = ReplaceTokens(resolvedValue, id, version);

            foreach (var kvp in variables.OrderByDescending(x => x.Key.Length))
            {
                resolvedValue = new Regex($@"\${Regex.Escape(kvp.Key)}\b", RegexOptions.IgnoreCase)
                    .Replace(resolvedValue, _ => kvp.Value);
            }

            return resolvedValue;
        }

        public static List<string> GetOriginalUrls(string content, string id, string version, string notToReplaceUrl)
        {
            var downloads = new List<string>();
            _ = Regex.Replace(content, StringHelper.RxUrlPattern.ToString(), new MatchEvaluator(m =>
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
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return string.Empty;
            }

            var match = RxPackageSelector.Match(packageId.Trim());
            return match.Success ? match.Groups["id"].Value : packageId.Trim();
        }

        public static string GetVersionFromString(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return string.Empty;
            }

            var match = RxPackageSelector.Match(packageId.Trim());
            return match.Success ? match.Groups["version"].Value : string.Empty;
        }
    }
}


