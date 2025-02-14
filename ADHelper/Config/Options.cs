namespace ADHelper.Config {
    class Options {
        private string uname, pword; // user 
        private string csvPath; // input files
        private string domain; // domain
        private string task;
        private bool inDataHeaders = true;
        private bool generatePasswords = false;
        private bool alphatize = false;

        public Options() { }

        public string Username {
            get { return uname; }
            set { uname = value; }
        }

        public string Password {
            get { return pword; }
            set { pword = value; }
        }

        public string Domain {
            get { return domain; }
            set { domain = value; }
        }

        public string CsvPath {
            get { return csvPath; }
            set { csvPath = value; }
        }

        public string Task {
            get { return task; }
            set { task = value; }
        }

        public bool InDataHeaders {
            get { return inDataHeaders; }
            set { inDataHeaders = value; }
        }

        public bool GeneratePasswords {
            get { return generatePasswords; }
            set { generatePasswords = value; }
        }

        public bool Alphatize {
            get { return alphatize; }
            set { alphatize = value; }
        }
    }
}