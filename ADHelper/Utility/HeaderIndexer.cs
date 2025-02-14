using System.Collections.Generic;
using System.Text.RegularExpressions;
using ADHelper.Config;

namespace ADHelper.Utility
{
    public static class HeaderIndexer
    {
        public static Dictionary<string, int> GetHeaderIndices(string[] headers) {
            var headerIndices = new Dictionary<string, int>();

            foreach (var key in Patterns.GetKeys()) {
                string pattern = Patterns.GetPattern(key);
                for (int i = 0; i < headers.Length; i++) {
                    string header = headers[i].Trim().ToLower();
                    if (Regex.IsMatch(header, pattern, RegexOptions.IgnoreCase)) {
                        headerIndices[key] = i;
                    }
                }
            }

            return headerIndices;
        }
    }
}