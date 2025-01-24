using System;
using System.IO;

namespace ADHelper.Utility {
    class Logger {
        private string _successFilePath;
        private string _failFilePath;

        public Logger(string outputDirectory) {
            _successFilePath = Path.Combine(outputDirectory, $"succeeded.{DateTime.Now.ToFileTime()}.csv");
            _failFilePath = Path.Combine(outputDirectory, $"failed.{DateTime.Now.ToFileTime()}.csv");
        }

        public void LogSuccess(string message) {
            using (var writer = new StreamWriter(_successFilePath, true)) {
                writer.WriteLine(message);
            }
        }

        public void LogFailure(string message) {
            using (var writer = new StreamWriter(_failFilePath, true)) {
                writer.WriteLine(message);
            }
        }
    }
}