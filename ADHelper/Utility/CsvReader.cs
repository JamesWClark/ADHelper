using System;
using System.Collections.Generic;
using System.IO;

namespace ADHelper.Utility {
    class CsvReader {
        public static (string[] headers, List<string[]> data) ReadCsvWithHeaders(string filePath, bool hasHeaders) {
            var lines = new List<string[]>();
            string[] headers = null;

            using (var reader = new StreamReader(filePath)) {
                if (hasHeaders) {
                    string headerLine = reader.ReadLine(); // Read header line
                    if (headerLine != null) {
                        headers = headerLine.Split(',');
                    }
                }
                string line;
                while ((line = reader.ReadLine()) != null) {
                    lines.Add(line.Split(','));
                }
            }

            return (headers, lines);
        }
    }
}