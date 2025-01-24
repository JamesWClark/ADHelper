using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ADHelper.Config;

namespace ADHelper.Utility
{
    public static class HeaderIndexer
    {
        public static Dictionary<string, int> GetHeaderIndices(string[] headers)
        {
            var headerIndices = new Dictionary<string, int>();

            for (int i = 0; i < headers.Length; i++) {
                string header = headers[i].Trim().ToLower();

                if (Regex.IsMatch(header, Patterns.ImportID, RegexOptions.IgnoreCase))        headerIndices["ImportID"] = i;
                if (Regex.IsMatch(header, Patterns.FirstName, RegexOptions.IgnoreCase))       headerIndices["FirstName"] = i;
                if (Regex.IsMatch(header, Patterns.LastName, RegexOptions.IgnoreCase))        headerIndices["LastName"] = i;
                if (Regex.IsMatch(header, Patterns.SamAccount, RegexOptions.IgnoreCase))      headerIndices["SamAccountName"] = i;
                if (Regex.IsMatch(header, Patterns.Email, RegexOptions.IgnoreCase))           headerIndices["Email"] = i;
                if (Regex.IsMatch(header, Patterns.Password, RegexOptions.IgnoreCase))        headerIndices["Password"] = i;
            }

            return headerIndices;
        }
    }
}