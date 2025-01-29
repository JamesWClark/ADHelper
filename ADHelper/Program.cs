using System;
using System.IO;
using System.Configuration;
using System.DirectoryServices;
using System.Text.RegularExpressions;

namespace ADHelper {
    /// <summary>
    /// 
    /// </summary>
    class Program {

        /**
         * @TODO read more, learn to use uname/pword auth
         * https://docs.microsoft.com/en-us/dotnet/api/system.directoryservices.accountmanagement?view=dotnet-plat-ext-5.0
         */

        // ADHelper.exe -csv test.csv -config config.xml -task create_users
        // ADHelper.exe -csv test.csv -config config.xml -task set_passwords


        // test data https://docs.google.com/spreadsheets/d/1WjtfjSwoOvFsAnMVRwAyEEczGB7-vM4-ZYbqup4Nv2I/edit#gid=0

        static void Main(string[] args) {

            Config.Options opts = new Config.Options(args);

            if(args.Length == 0) {
                Console.WriteLine("Provide command line arguments, for example: ");
                Console.WriteLine("1) ADHelper.exe -csv users.csv -config config.xml -task create_users");
                Console.WriteLine("2) ADHelper.exe -csv users.csv -config config.xml -task set_passwords");
                Console.ReadLine();
                return;
            }
            if(opts.Task.Length == 0) {
                Console.WriteLine("Task needed.");
                Console.WriteLine("-task create_user or set_password");
                return;
            }

            if(opts.CsvPath.Length == 0) {
                Console.WriteLine("User data file needed.");
                return;
            }

            if(opts.ConfigPath.Length == 0) {
                Console.WriteLine("Config file needed.");
                return;
            }

            string outputDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (Environment.GetEnvironmentVariable("RUN_MODE") == "VSCode") {
                outputDirectory = Path.Combine(outputDirectory, "..", "..", "..", "ADHelper", "TestData", "Receipts");
            }

            if(opts.Task.Length > 0) {
                Console.WriteLine("Attempting task with options: ");
                Console.WriteLine($"Task: {opts.Task}");
                Console.WriteLine($"Config File: {opts.ConfigPath}");
                Console.WriteLine($"Domain: {opts.Domain}");
                Console.WriteLine($"Distinguished Name: {opts.DistinguishedName}");
                Console.WriteLine($"Input File: {opts.CsvPath}");
                Console.WriteLine($"- has headers: {opts.InDataHeaders}");
                Console.WriteLine($"Output Directory: {outputDirectory}");

                switch (opts.Task.ToLower()) {
                    case "create_users":
                        Tasks.Task_BatchCreateUsers createUserTask = new Tasks.Task_BatchCreateUsers(opts, outputDirectory);
                        createUserTask.Run();
                        break;
                    case "set_passwords":
                        Tasks.Task_BatchSetPasswords setPasswordTask = new Tasks.Task_BatchSetPasswords(opts, outputDirectory);
                        setPasswordTask.Run();
                        break;
                    default:
                        Console.WriteLine($"Unsupported Task: {opts.Task}");
                        break;
                }
            }
        }
    }
}