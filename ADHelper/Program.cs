using System;
using System.IO;
using System.DirectoryServices.AccountManagement;
using ADHelper.Tasks;
using ADHelper.Config;
using ADHelper.Utility;

namespace ADHelper {
    class Program {
        static void Main(string[] args) {
            Logger.Information("Program started");

            if (args.Length == 0) {
                Logger.Information("No command line arguments provided");
                Console.WriteLine("Provide command line arguments, for example: ");
                Console.WriteLine("1) ADHelper.exe -csv users.csv -task create_users");
                Console.WriteLine("2) ADHelper.exe -csv users.csv -task set_passwords");
                Console.WriteLine("3) ADHelper.exe -csv users.csv -task generate_passwords");
                Console.ReadLine();
                return;
            }

            string csvPath = string.Empty;
            string task = string.Empty;

            try {
                for (int i = 0; i < args.Length; i += 2) {
                    switch (args[i]) {
                        case "-csv":
                            csvPath = args[i + 1];
                            Logger.Debug($"CSV path set to: {csvPath}");
                            break;
                        case "-task":
                            task = args[i + 1];
                            Logger.Debug($"Task set to: {task}");
                            break;
                        default:
                            throw new ArgumentException($"{args[i]} is not a valid option.");
                    }
                }

                if (string.IsNullOrEmpty(task)) {
                    Logger.Information("Task needed.");
                    Console.WriteLine("Task needed.");
                    Console.WriteLine("-task create_user, set_password, or generate_passwords");
                    return;
                }

                if (string.IsNullOrEmpty(csvPath)) {
                    Logger.Information("User data file needed.");
                    Console.WriteLine("User data file needed.");
                    return;
                }

                string outputDirectory = AppDomain.CurrentDomain.BaseDirectory;

                if (Environment.GetEnvironmentVariable("RUN_MODE") == "VSCode") {
                    outputDirectory = Path.Combine(outputDirectory, "..", "..", "..", "ADHelper", "TestData", "Receipts");
                }

                string domain = GetCurrentDomain();

                Logger.Information($"Attempting task with options: Task: {task}, Domain: {domain}, Input File: {csvPath}, Output Directory: {outputDirectory}");

                var options = new Options {
                    CsvPath = csvPath,
                    Domain = domain,
                    Task = task
                };

                switch (task.ToLower()) {
                    case "create_users":
                        Logger.Debug("Starting create_users task");
                        Tasks.Task_BatchCreateUsers createUserTask = new Tasks.Task_BatchCreateUsers(options, outputDirectory);
                        createUserTask.Run();
                        Logger.Information("create_users task completed successfully");
                        break;
                    case "set_passwords":
                        Logger.Debug("Starting set_passwords task");
                        Tasks.Task_BatchSetPasswords setPasswordTask = new Tasks.Task_BatchSetPasswords(options, outputDirectory);
                        setPasswordTask.Run();
                        Logger.Information("set_passwords task completed successfully");
                        break;
                    case "generate_passwords":
                        Logger.Debug("Starting generate_passwords task");
                        Tasks.Task_GeneratePasswords generatePasswordsTask = new Tasks.Task_GeneratePasswords(options, outputDirectory);
                        generatePasswordsTask.Run();
                        Logger.Information("generate_passwords task completed successfully");
                        break;
                    default:
                        Logger.Information($"Unsupported Task: {task}");
                        Console.WriteLine($"Unsupported Task: {task}");
                        break;
                }
            } catch (Exception ex) {
                Logger.Error("An error occurred while processing the task", ex);
                Console.WriteLine($"An error occurred: {ex.Message}");
            } finally {
                Logger.CloseAndFlush();
            }
        }

        private static string GetCurrentDomain() {
            using (var context = new PrincipalContext(ContextType.Domain)) {
                return context.ConnectedServer;
            }
        }
    }
}