using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ADHelper.Utility;
using ADHelper.Config;
using System.DirectoryServices.AccountManagement;

namespace ADHelper.Tasks {
    class Task_GeneratePasswords : TaskBase {
        private string uniformPassword = String.Empty;

        public Task_GeneratePasswords(Config.Options options, string outputDirectory) : base(options, outputDirectory) { }

        public Task_GeneratePasswords(Config.Options options, string outputDirectory, string password) : base(options, outputDirectory) {
            this.uniformPassword = password;
        }

        public override void Run() {
            Logger.Information("Task_GeneratePasswords Run method called");

            try {
                var (headers, lines) = CsvReader.ReadCsvWithHeaders(opts.CsvPath, opts.InDataHeaders);
                Logger.Debug("CSV Headers: " + string.Join(", ", headers));
                Logger.Debug("First CSV Line: " + string.Join(", ", lines[0]));

                var headerMap = MapHeadersToKeys(headers);
                var headerIndices = HeaderIndexer.GetHeaderIndices(headers);

                string output_file_path = Path.Combine(_outputDirectory, $"generated_passwords.{DateTime.Now.ToFileTime()}.csv");

                bool headersWritten = false;

                int count = 0;
                foreach (var columns in lines) {
                    var userFields = new Dictionary<string, string>();
                    foreach (var header in headerIndices.Keys) {
                        if (headerIndices[header] < columns.Length) {
                            var key = headerMap.ContainsKey(header) ? headerMap[header] : header;
                            userFields[key] = columns[headerIndices[header]].Trim();
                        } else {
                            var key = headerMap.ContainsKey(header) ? headerMap[header] : header;
                            userFields[key] = null; // or an appropriate default value
                        }
                    }

                    string newPassword;
                    if (uniformPassword == String.Empty) {
                        newPassword = PasswordGenerator.WordList5Password(2); // Generate two 5-letter words and two numbers
                    } else {
                        newPassword = uniformPassword;
                    }

                    // Set the new password in Active Directory
                    try {
                        using (var context = new PrincipalContext(ContextType.Domain, opts.Domain)) {
                            using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userFields["SamAccountName"])) {
                                if (user != null) {
                                    user.SetPassword(newPassword);
                                    user.Save();
                                    Logger.Information($"Password set for user: {userFields["SamAccountName"]}");
                                } else {
                                    throw new Exception($"User not found: {userFields["SamAccountName"]}");
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Logger.Error($"Failed to set password for user: {userFields["SamAccountName"]}", ex);
                        badSamAccountNames.Add(userFields["SamAccountName"]);
                        continue;
                    }

                    using (var tw = new StreamWriter(output_file_path, true)) {
                        if (!headersWritten) {
                            var outputHeaders = new List<string>(headers);
                            if (headers.Contains("Password")) {
                                outputHeaders.Remove("Password");
                                outputHeaders.Add("OldPassword");
                            }
                            outputHeaders.Add("NewPassword");
                            tw.WriteLine(string.Join(",", outputHeaders));
                            headersWritten = true;
                        }

                        var outputColumns = new List<string>();
                        foreach (var header in headers) {
                            if (userFields.ContainsKey(header)) {
                                var value = userFields[header];
                                outputColumns.Add(value != null && value.Contains(",") ? $"\"{value}\"" : value);
                            } else {
                                outputColumns.Add(""); // Add empty value for missing columns
                            }
                        }
                        if (headers.Contains("Password")) {
                            outputColumns.Add(userFields["Password"]);
                        }
                        outputColumns.Add(newPassword);

                        tw.WriteLine(string.Join(",", outputColumns));
                    }

                    count++;
                }
            } catch (IOException e) {
                Logger.Error("An IO exception occurred while reading the CSV file", e);
            }

            Logger.Information("\n\nFailed users:");
            foreach (string s in badSamAccountNames) {
                Logger.Information(s);
            }
        }
    }
}