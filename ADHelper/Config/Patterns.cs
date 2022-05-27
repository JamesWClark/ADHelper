using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADHelper.Config {
    class Patterns {
        private static List<string> patternImportID = new List<string> {
            "id",
            "import", "importid", "import id",
            "external id", "externalid",
            "userid", "user id",
            "unique id", "uniqueid",
            "system id", "systemid"
        };

        private static List<string> patternFirstName = new List<string> {
            "first", "firstname", "first name", 
            "givenname", "given name",
            "preferred name", "preferred"
        };

        private static List<string> patternLastName = new List<string> {
            "last", "lastname", "last name",
            "surname", "familyname", "family name"
        };

        private static List<string> patternEmail = new List<string> {
            "email", "email address", "emailaddress"
        };

        private static List<string> patternSamAccountName = new List<string> {
            "active directory", "directory name", "ad login", "activedirectory",
            "sam", "samaccount", "sam account", "samaccountname", "sam account name", "samaccount name"
        };

        private static List<string> patternPassword = new List<string> {
            "pass", "password", "pword", "secret"
        };

        public static string ImportID {
            get { return "(" + String.Join("|", patternImportID.ToArray()) + ")"; }
        }

        public static string FirstName {
            get { return "(" + String.Join("|", patternFirstName.ToArray()) + ")"; }
        }

        public static string LastName {
            get { return "(" + String.Join("|", patternLastName.ToArray()) + ")";  }
        }

        public static string Email {
            get { return "(" + String.Join("|", patternEmail.ToArray()) + ")";  }
        }

        public static string SamAccount {
            get { return "(" + String.Join("|", patternSamAccountName.ToArray()) + ")"; }
            
        }

        public static string Password {
            get { return "(" + String.Join("|", patternPassword.ToArray()) + ")";  }
        }
    }
}
