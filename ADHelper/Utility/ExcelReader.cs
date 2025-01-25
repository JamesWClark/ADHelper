using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace ADHelper.Utility {
    class ExcelReader {
        public static (string[] headers, List<string[]> data) ReadExcelWithHeaders(string filePath, bool hasHeaders) {
            var lines = new List<string[]>();
            string[] headers = null;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context

            using (var package = new ExcelPackage(new FileInfo(filePath))) {
                var worksheet = package.Workbook.Worksheets[0];
                var startRow = hasHeaders ? 2 : 1;

                if (hasHeaders) {
                    headers = new string[worksheet.Dimension.Columns];
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                        headers[col - 1] = worksheet.Cells[1, col].Text.Trim();
                    }
                }

                for (int row = startRow; row <= worksheet.Dimension.Rows; row++) {
                    var rowData = new List<string>();
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                        rowData.Add(worksheet.Cells[row, col].Text.Trim());
                    }
                    lines.Add(rowData.ToArray());
                }
            }

            return (headers, lines);
        }
    }
}