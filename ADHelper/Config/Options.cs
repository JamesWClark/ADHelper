using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ADHelper.Config {
    class Options {
        private string uname, pword; // user 
        private string xmlPath, csvPath; // input files
        private string distinguishedName, domain; // domain
        private string task;
        private bool inDataHeaders = true;
        private bool generatePasswords = false;
        private bool alphatize = false;

        public Options(string[] args) {
            for(int i = 0; i < args.Length; i += 2) {
                switch(args[i]) {
                    case "-u":
                        uname = args[i + 1];
                        break;
                    case "-p":
                        pword = args[i + 1];
                        break;
                    case "-x":
                    case "-xml":
                    case "-config":
                        xmlPath = args[i + 1];
                        LoadXML();
                        break;
                    case "-dat":
                    case "-data":
                    case "-csv":
                        csvPath = args[i + 1];
                        break;
                    case "-t":
                    case "-task":
                        task = args[i + 1];
                        break;
                    default:
                        throw new ArgumentException($"{args[i]} is not a valid option. ");
                }
            }
        }

        private void LoadXML() {
            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(xmlPath);
            } catch (FileNotFoundException) {
                throw new ArgumentException($"File not found: {xmlPath}. ");
            }
            domain              = tryReadNode(doc, "/configuration/domain");
            distinguishedName   = tryReadNode(doc, "/configuration/distinguishedName");
            inDataHeaders       = Convert.ToBoolean(tryReadNode(doc, "/configuration/csv/headers"));
            generatePasswords   = Convert.ToBoolean(tryReadNode(doc, "/configuration/password/generator"));
        }

        private string tryReadNode(XmlDocument doc, string nodePath) {
            XmlNode node = doc.SelectSingleNode(nodePath);
            return node?.InnerText ?? string.Empty;
        }

        public string Username {
            get { return uname; }
        }

        public string Password {
            get { return pword; }
        }

        public string Domain {
            get { return domain; }
        }

        public string DistinguishedName {
            get { return distinguishedName; }
        }

        public string CsvPath {
            get { return csvPath; }
        }

        public string ConfigPath {
            get { return xmlPath; }
        }

        public string Task {
            get { return task; }
        }

        public bool InDataHeaders {
            get { return inDataHeaders; }
        }

        public bool GeneratePasswords {
            get { return generatePasswords; }
        }

        public bool Alphatize {
            get { return alphatize; }
        }
    }
}