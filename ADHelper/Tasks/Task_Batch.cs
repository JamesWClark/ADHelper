using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Text.RegularExpressions;
using ADHelper.Utility;

namespace ADHelper.Tasks {
    class Task_Batch {

        private List<string> badSamAccountNames = new List<string>();
        private Config.Options opts;
        private string _outputDirectory;

        public Task_Batch(Config.Options options, string outputDirectory) {
            opts = options;
            _outputDirectory = outputDirectory;
        }

        public void Run() {
            Console.WriteLine("Task_Batch Run method called");

            try {
                var (headers, lines) = CsvReader.ReadCsvWithHeaders(opts.CsvPath, opts.InDataHeaders);
                Console.WriteLine("CSV Headers: " + string.Join(", ", headers));
                Console.WriteLine("First CSV Line: " + string.Join(", ", lines[0]));

                var headerIndices = HeaderIndexer.GetHeaderIndices(headers);

                string success_file_path = Path.Combine(_outputDirectory, $"succeeded.{DateTime.Now.ToFileTime()}.csv");
                string fail_file_path = Path.Combine(_outputDirectory, $"failed.{DateTime.Now.ToFileTime()}.csv");

                var userManager = new UserManager(opts.Domain, opts.DistinguishedName);
                Console.WriteLine($"UserManager initialized with Domain: {opts.Domain}, DistinguishedName: {opts.DistinguishedName}");

                int count = 0;
                foreach (var columns in lines) {
                    var userFields = new Dictionary<string, string>();
                    foreach (var header in headerIndices.Keys) {
                        userFields[header] = columns[headerIndices[header]].Trim();
                    }

                    // Debugging statement to check if SamAccountName is present
                    if (!userFields.ContainsKey("SamAccountName")) {
                        Console.WriteLine("SamAccountName key is missing in userFields dictionary.");
                        Console.WriteLine("Available keys: " + string.Join(", ", userFields.Keys));
                        continue;
                    }

                    try {
                        Console.WriteLine($"Processing user: {userFields["Email"]}, {userFields["SamAccountName"]}");
                        switch (opts.Task) {
                            case "create_users":
                                Console.WriteLine($"Creating user: {userFields["Email"]}");
                                userManager.CreateUser(userFields);
                                using (var tw = new StreamWriter(success_file_path, true)) {
                                    if (count == 0) {
                                        tw.WriteLine("Import ID,First Name,Last Name,Email,AD Login,Password");
                                    }
                                    tw.WriteLine($"{userFields["ImportID"]},{userFields["FirstName"]},{userFields["LastName"]},{userFields["Email"]},{userFields["SamAccountName"]},{userFields["Password"]}");
                                    Console.WriteLine("created: " + userFields["Email"]);
                                }
                                break;
                            case "set_passwords":
                                Console.WriteLine($"Setting password for user: {userFields["SamAccountName"]}");
                                userManager.SetPassword(userFields["SamAccountName"], userFields["Password"]);
                                using (var tw = new StreamWriter(success_file_path, true)) {
                                    tw.WriteLine($"{userFields["FirstName"]},{userFields["LastName"]},{userFields["Email"]},{userFields["SamAccountName"]},{userFields["Password"]}");
                                    Console.WriteLine($"Setting password for record: {userFields["FirstName"]},{userFields["LastName"]},{userFields["Email"]},{userFields["SamAccountName"]},{userFields["Password"]}");
                                }
                                break;
                            default:
                                throw new ArgumentException($"Unsupported Task: {opts.Task}");
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("failed: " + userFields["Email"]);
                        Console.WriteLine(ex.Message);
                        using (var tw = new StreamWriter(fail_file_path, true)) {
                            tw.WriteLine($"{userFields["SamAccountName"]},{userFields["Email"]},{ex.Message}");
                        }
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