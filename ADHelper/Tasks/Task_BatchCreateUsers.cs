using System;
using System.Collections.Generic;
using System.IO;
using ADHelper.Utility;
using ADHelper.Config;

namespace ADHelper.Tasks {
    class Task_BatchCreateUsers : TaskBase {

        public Task_BatchCreateUsers(Config.Options options, string outputDirectory) : base(options, outputDirectory) { }

        public override void Run() {
            Console.WriteLine("Task_BatchCreateUsers Run method called");

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
                        Console.WriteLine($"Processing user: {userFields["Email"]}, {userFields["SamAccountName"]}");
                        switch (opts.Task) {
                            case "create_users":
                                Console.WriteLine($"Creating user: {userFields["Email"]}");
                                userManager.CreateUser(userFields);
                                if (!successHeadersWritten) {
                                    LogSuccess("Import ID,First Name,Last Name,Email,AD Login,Password");
                                    successHeadersWritten = true;
                                }
                                LogSuccess($"{userFields["ImportID"]},{userFields["FirstName"]},{userFields["LastName"]},{userFields["Email"]},{userFields["SamAccountName"]},{userFields["Password"]}");
                                Console.WriteLine("created: " + userFields["Email"]);
                                break;
                            default:
                                throw new ArgumentException($"Unsupported Task: {opts.Task}");
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("failed: " + userFields["Email"]);
                        Console.WriteLine(ex.Message);
                        if (!failHeadersWritten) {
                            LogFailure("Import ID,First Name,Last Name,Email,AD Login,Error Message");
                            failHeadersWritten = true;
                        }
                        LogFailure($"{userFields["ImportID"]},{userFields["FirstName"]},{userFields["LastName"]},{userFields["Email"]},{userFields["SamAccountName"]},{ex.Message.Trim()}");
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