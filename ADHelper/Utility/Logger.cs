using System;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ADHelper.Utility {
    public static class Logger {
        static Logger() {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var logLevel = configuration["Logging:LogLevel:ADHelper"] ?? "Information";

            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);

            switch (logLevel.ToLower()) {
                case "debug":
                    loggerConfiguration.MinimumLevel.Debug();
                    break;
                case "verbose":
                    loggerConfiguration.MinimumLevel.Verbose();
                    break;
                case "information":
                default:
                    loggerConfiguration.MinimumLevel.Information();
                    break;
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        public static void Debug(string message) {
            Log.Debug(message);
        }

        public static void Verbose(string message) {
            Log.Verbose(message);
        }

        public static void Information(string message) {
            Log.Information(message);
        }

        public static void Warning(string message) {
            Log.Warning(message);
        }

        public static void Error(string message) {
            Log.Error(message);
        }

        public static void Error(string message, Exception ex) {
            Log.Error(ex, message);
        }

        public static void CloseAndFlush() {
            Log.CloseAndFlush();
        }
    }
}