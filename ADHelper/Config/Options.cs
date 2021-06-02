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
            Console.WriteLine("\nUser: " + uname);
            Console.WriteLine("\nDomain: " + domain + "\nDN: " + distinguishedName);

        }

        private void LoadXML() {
            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(xmlPath);
            } catch (FileNotFoundException) {
                throw new ArgumentException($"File not found: {xmlPath}. ");
            }
            try {
                XmlNode domainNode = doc.DocumentElement.SelectSingleNode("/configuration/domain");
                XmlNode dnNode = doc.DocumentElement.SelectSingleNode("/configuration/dn");
                distinguishedName = dnNode.InnerText;
                domain = domainNode.InnerText;
            } catch(NullReferenceException) {
                throw new ArgumentException($"Malformed XML. ");
            }
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

        public string Task {
            get { return task; }
        }
    }
}
