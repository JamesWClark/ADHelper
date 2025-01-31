using System;
using System.Collections.Generic;
using System.IO;
using ADHelper.Config;

namespace ADHelper.Tasks {
    abstract class TaskBase {
        protected List<string> badSamAccountNames = new List<string>();
        protected Config.Options opts;
        protected string _outputDirectory;
        protected string success_file_path;
        protected string fail_file_path;
        protected bool successHeadersWritten = false;
        protected bool failHeadersWritten = false;

        public TaskBase(Config.Options options, string outputDirectory) {
            opts = options;
            _outputDirectory = outputDirectory;
            success_file_path = Path.Combine(_outputDirectory, $"succeeded.{DateTime.Now.ToFileTime()}.csv");
            fail_file_path = Path.Combine(_outputDirectory, $"failed.{DateTime.Now.ToFileTime()}.csv");
        }

        protected Dictionary<string, string> MapHeadersToKeys(string[] headers) {
            var headerMap = new Dictionary<string, string>();
            foreach (var key in Patterns.GetKeys()) {
                var patterns = Patterns.GetPatterns(key);
                foreach (var header in headers) {
                    if (patterns.Contains(header.ToLower())) {
                        headerMap[header] = key;
                        break;
                    }
                }
            }
            return headerMap;
        }

        public abstract void Run();

        protected void LogSuccess(string message) {
            using (var tw = new StreamWriter(success_file_path, true)) {
                tw.WriteLine(message);
            }
        }

        protected void LogFailure(string message) {
            using (var tw = new StreamWriter(fail_file_path, true)) {
                tw.WriteLine(message);
            }
        }
    }
}