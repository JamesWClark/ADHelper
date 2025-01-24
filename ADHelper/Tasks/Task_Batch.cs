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

        public string alphatize(string dirty) {
            /*
            string mapDirty = "ŠŽšžŸÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖÙÚÛÜÝàáâãäåçèéêëìíîïðñòóôõöùúûüýÿ";
            string mapClean = "SZszYAAAAAACEEEEIIIIDNOOOOOUUUUYaaaaaaceeeeiiiidnooooouuuuyy";
            string clean = "";
            bool isCleanCharacter = true;
            for(int i = 0; i < dirty.Length; i++) {
                for(int k = 0; i < mapDirty.Length; k++) {
                    if (dirty[i] == mapDirty[k]) {
                        clean += mapClean[k];
                        isCleanCharacter = false;
                        break;
                    }
                }
                if(!isCleanCharacter) {
                    isCleanCharacter = true;
                } else {
                    clean += dirty[i]; 
                }
            }
            return clean;
            */
            return dirty;
        }

        public static string WordListPassword(int n) {
            string pw = "";
            Random rand = new Random();
            for (int i = 0; i < n; i++) {
                pw += Config.WordList5.Words[rand.Next(0, Config.WordList5.Words.Length)];
            }
            String end = "69"; // because high school students
            while (end == "69") {
                end = "" + rand.Next(0, 10) + rand.Next(0, 10);
            }
            return pw + end;
        }

        public void Run() {
            try {
                var (headers, lines) = CsvReader.ReadCsvWithHeaders(opts.CsvPath, opts.InDataHeaders);
                var headerIndices = HeaderIndexUtil.GetHeaderIndices(headers);

                string success_file_path = Path.Combine(_outputDirectory, $"succeeded.{DateTime.Now.ToFileTime()}.csv");
                string fail_file_path = Path.Combine(_outputDirectory, $"failed.{DateTime.Now.ToFileTime()}.csv");

                var userManager = new UserManager(opts.Domain, opts.DistinguishedName);

                int count = 0;
                foreach (var columns in lines) {
                    if (count == 0 && opts.InDataHeaders) {
                        count++;
                        continue;
                    }

                    string importID = alphatize(columns[headerIndices["ImportID"]].Trim());
                    string fname = alphatize(columns[headerIndices["FirstName"]].Trim());
                    string lname = alphatize(columns[headerIndices["LastName"]].Trim());
                    string samAccountName = alphatize(columns[headerIndices["SamAccountName"]].Trim());
                    string email = alphatize(columns[headerIndices["Email"]].Trim());

                    string domain = email.Split('@')[1];
                    string password = opts.GeneratePasswords ? WordListPassword(2) : columns[headerIndices["Password"]].Trim();

                    try {
                        switch (opts.Task) {
                            case "create_users":
                                userManager.CreateUser(fname, lname, samAccountName, email, password);
                                using (var tw = new StreamWriter(success_file_path, true)) {
                                    if (count == 0) {
                                        tw.WriteLine("Import ID,First Name,Last Name,Email,AD Login,Password");
                                    }
                                    tw.WriteLine($"{importID},{fname},{lname},{email},{samAccountName},{password}");
                                    Console.WriteLine("created: " + email);
                                }
                                break;
                            case "set_passwords":
                                userManager.SetPassword(samAccountName, password);
                                using (var tw = new StreamWriter(success_file_path, true)) {
                                    tw.WriteLine($"{fname},{lname},{email},{samAccountName},{password}");
                                    Console.WriteLine($"Setting password for record: {fname},{lname},{email},{samAccountName},{password}");
                                }
                                break;
                            default:
                                throw new ArgumentException($"Unsupported Task: {opts.Task}");
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("failed: " + email);
                        Console.WriteLine(ex.Message);
                        using (var tw = new StreamWriter(fail_file_path, true)) {
                            tw.WriteLine($"{samAccountName},{email},{ex.Message}");
                        }
                        badSamAccountNames.Add(samAccountName);
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
        }
    }
}