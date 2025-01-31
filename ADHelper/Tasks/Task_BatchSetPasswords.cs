﻿using System;
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
            Console.WriteLine("Task_BatchSetPasswords Run method called");

            try {
                var (headers, lines) = CsvReader.ReadCsvWithHeaders(opts.CsvPath, opts.InDataHeaders);
                Console.WriteLine("CSV Headers: " + string.Join(", ", headers));
                Console.WriteLine("First CSV Line: " + string.Join(", ", lines[0]));

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
                    Console.WriteLine();
                    Console.WriteLine($"-- UserManager initialized with Domain: {domain}, DistinguishedName: {distinguishedName}");

                    try {
                        if (string.IsNullOrEmpty(userFields["Password"])) {
                            throw new Exception("Password field is missing");
                        }

                        Console.WriteLine($"Setting password for user: {userFields["SamAccountName"]}");
                        using (var context = new PrincipalContext(ContextType.Domain, domain)) {
                            using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userFields["SamAccountName"])) {
                                if (user != null) {
                                    user.SetPassword(userFields["Password"]);
                                    user.Save();
                                    Console.WriteLine($"Password set for user: {userFields["SamAccountName"]}");
                                } else {
                                    throw new Exception($"User not found: {userFields["SamAccountName"]}");
                                }
                            }
                        }

                        if (!successHeadersWritten) {
                            LogSuccess(string.Join(",", headers));
                            successHeadersWritten = true;
                        }
                        LogSuccess(string.Join(",", columns.Select(c => c.Contains(",") ? $"\"{c}\"" : c)));
                        Console.WriteLine($"Setting password for record: {userFields["FirstName"]},{userFields["LastName"]},{userFields["Email"]},{userFields["SamAccountName"]},{userFields["Password"]}");
                    } catch (Exception ex) {
                        Console.WriteLine("failed: " + userFields["Email"]);
                        Console.WriteLine(ex.Message);
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
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("\n\nfails:");
            foreach (string s in badSamAccountNames) {
                Console.WriteLine(s);
            }
            Console.WriteLine();
        }
    }
}