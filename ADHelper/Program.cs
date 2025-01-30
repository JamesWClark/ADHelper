using System;
using System.IO;
using System.DirectoryServices.AccountManagement;
using ADHelper.Tasks;
using ADHelper.Config;

namespace ADHelper {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Provide command line arguments, for example: ");
                Console.WriteLine("1) ADHelper.exe -csv users.csv -task create_users");
                Console.WriteLine("2) ADHelper.exe -csv users.csv -task set_passwords");
                Console.WriteLine("3) ADHelper.exe -csv users.csv -task generate_passwords");
                Console.ReadLine();
                return;
            }

            string csvPath = string.Empty;
            string task = string.Empty;

            for (int i = 0; i < args.Length; i += 2) {
                switch (args[i]) {
                    case "-csv":
                        csvPath = args[i + 1];
                        break;
                    case "-task":
                        task = args[i + 1];
                        break;
                    default:
                        throw new ArgumentException($"{args[i]} is not a valid option.");
                }
            }

            if (string.IsNullOrEmpty(task)) {
                Console.WriteLine("Task needed.");
                Console.WriteLine("-task create_user, set_password, or generate_passwords");
                return;
            }

            if (string.IsNullOrEmpty(csvPath)) {
                Console.WriteLine("User data file needed.");
                return;
            }

            string outputDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (Environment.GetEnvironmentVariable("RUN_MODE") == "VSCode") {
                outputDirectory = Path.Combine(outputDirectory, "..", "..", "..", "ADHelper", "TestData", "Receipts");
            }

            string domain = GetCurrentDomain();

            Console.WriteLine("Attempting task with options: ");
            Console.WriteLine($"Task: {task}");
            Console.WriteLine($"Domain: {domain}");
            Console.WriteLine($"Input File: {csvPath}");
            Console.WriteLine($"Output Directory: {outputDirectory}");

            var options = new Options {
                CsvPath = csvPath,
                Domain = domain,
                Task = task
            };

            switch (task.ToLower()) {
                case "create_users":
                    Tasks.Task_BatchCreateUsers createUserTask = new Tasks.Task_BatchCreateUsers(options, outputDirectory);
                    createUserTask.Run();
                    break;
                case "set_passwords":
                    Tasks.Task_BatchSetPasswords setPasswordTask = new Tasks.Task_BatchSetPasswords(options, outputDirectory);
                    setPasswordTask.Run();
                    break;
                case "generate_passwords":
                    Tasks.Task_GeneratePasswords generatePasswordsTask = new Tasks.Task_GeneratePasswords(options, outputDirectory);
                    generatePasswordsTask.Run();
                    break;
                default:
                    Console.WriteLine($"Unsupported Task: {task}");
                    break;
            }
        }

        private static string GetCurrentDomain() {
            using (var context = new PrincipalContext(ContextType.Domain)) {
                return context.ConnectedServer;
            }
        }
    }
}