using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using ADHelper.Utility;
using ADHelper.Config;
using System.DirectoryServices.AccountManagement;

namespace ADHelper.Tasks {
    class Task_BatchSetPasswords : TaskBase {

        public Task_BatchSetPasswords(Config.Options options, string outputDirectory) : base(options, outputDirectory) { }

        public override void Run() {
            Logger.Information("Task_BatchSetPasswords Run method called");

            try {
                var (headers, lines) = CsvReader.ReadCsvWithHeaders(opts.CsvPath, opts.InDataHeaders);
                Logger.Debug("CSV Headers: " + string.Join(", ", headers));
                Logger.Debug("First CSV Line: " + string.Join(", ", lines[0]));

                var headerMap = MapHeadersToKeys(headers);
                var headerIndices = HeaderIndexer.GetHeaderIndices(headers);

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

                    // Check for Domain and DistinguishedName in the CSV
                    string domain = userFields.ContainsKey("Domain") && !string.IsNullOrEmpty(userFields["Domain"]) ? userFields["Domain"] : opts.Domain;
                    string distinguishedName = userFields.ContainsKey("DistinguishedName") && !string.IsNullOrEmpty(userFields["DistinguishedName"]) ? userFields["DistinguishedName"] : "OU=Default,DC=domain,DC=com";

                    var userManager = new UserManager(domain, distinguishedName);
                    Logger.Debug($"UserManager initialized with Domain: {domain}, DistinguishedName: {distinguishedName}");

                    try {
                        if (string.IsNullOrEmpty(userFields["Password"])) {
                            throw new Exception("Password field is missing");
                        }

                        Logger.Information($"Setting password for user: {userFields["SamAccountName"]}");
                        bool pwdResetRequired = userFields.ContainsKey("PwdResetRequired") && bool.TryParse(userFields["PwdResetRequired"], out bool resetRequired) && resetRequired;
                        userManager.SetPassword(userFields["SamAccountName"], userFields["Password"], pwdResetRequired);

                        if (!successHeadersWritten) {
                            LogSuccess(string.Join(",", headers));
                            successHeadersWritten = true;
                        }
                        LogSuccess(string.Join(",", columns.Select(c => c.Contains(",") ? $"\"{c}\"" : c)));
                        Logger.Information($"Password set for record: {userFields["FirstName"]},{userFields["LastName"]},{userFields["Email"]},{userFields["SamAccountName"]},{userFields["Password"]}");
                    } catch (Exception ex) {
                        Logger.Error($"Failed to set password for user: {userFields["Email"]}", ex);
                        if (!failHeadersWritten) {
                            LogFailure(string.Join(",", headers) + ",Error Message");
                            failHeadersWritten = true;
                        }
                        LogFailure(string.Join(",", columns.Select(c => c.Contains(",") ? $"\"{c}\"" : c)) + $",\"{ex.Message.Trim()}\"");
                        badSamAccountNames.Add(userFields["SamAccountName"]);
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