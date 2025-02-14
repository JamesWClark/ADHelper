using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;

namespace ADHelper.Utility {
    class CsvReader {
        public static (string[] headers, List<string[]> data) ReadCsvWithHeaders(string filePath, bool hasHeaders) {
            var lines = new List<string[]>();
            string[] headers = null;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = hasHeaders,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvHelper.CsvReader(reader, config)) {
                if (hasHeaders) {
                    csv.Read();
                    csv.ReadHeader();
                    headers = csv.HeaderRecord;
                }

                while (csv.Read()) {
                    var row = new List<string>();
                    for (int i = 0; csv.TryGetField(i, out string field); i++) {
                        row.Add(field);
                    }
                    lines.Add(row.ToArray());
                }
            }

            return (headers, lines);
        }
    }
}