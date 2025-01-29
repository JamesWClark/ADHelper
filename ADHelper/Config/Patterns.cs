using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADHelper.Config {
    class Patterns {

        // prereq - all of these patterns are lowercased elsewhere prior to being called here

        private static readonly Dictionary<string, List<string>> patternDictionary = new Dictionary<string, List<string>> {
            { "ImportID", new List<string> { "id", "import", "importid", "import id", "external id", "externalid", "userid", "user id", "unique id", "uniqueid", "system id", "systemid" } },
            { "FirstName", new List<string> { "first", "firstname", "first name", "fname", "givenname", "given name", "preferred name", "preferred" } },
            { "LastName", new List<string> { "last", "lastname", "last name", "lname", "surname", "familyname", "family name" } },
            { "Email", new List<string> { "email", "email address", "emailaddress", "e-mail" } },
            { "SamAccountName", new List<string> { "active directory", "directory name", "ad login", "activedirectory", "sam", "samaccount", "sam account", "samaccountname", "sam account name", "samaccount name", "sam pre-2000", "pre-2000", "pre2000", "sam pre-windows 2000", "windows pre2000", "windows pre-2000", "windows pre 2000" } },
            { "Password", new List<string> { "pass", "password", "pword", "secret" } },
            { "DisplayName", new List<string> { "display name", "displayname", "display" } },
            { "Description", new List<string> { "description", "desc" } },
            { "Office", new List<string> { "office", "office location", "office name", "office location name" } },
            { "TelephoneNumber", new List<string> { "telephone", "telephone number", "telephonenumber", "phone", "phone number", "phonenumber" } },
            { "Street", new List<string> { "street", "street address", "streetaddress", "address" } },
            { "City", new List<string> { "city", "town" } },
            { "State", new List<string> { "state", "province", "state/province", "stateprovince" } },
            { "PostalCode", new List<string> { "postal code", "postalcode", "zip", "zip code", "zipcode" } },
            { "Country", new List<string> { "country", "nation", "country name", "countryname" } },
            { "Mobile", new List<string> { "mobile", "mobile phone", "mobilephone", "cell", "cell phone", "cellphone" } },
            { "JobTitle", new List<string> { "job title", "jobtitle", "title", "position" } },
            { "Department", new List<string> { "department", "dept" } },
            { "Company", new List<string> { "company", "organization", "org" } },
            { "ManagerName", new List<string> { "manager", "manager name", "managername", "supervisor", "supervisor name", "supervisorname" } },
            { "DistinguishedName", new List<string> { "distinguished name", "distinguishedname", "dn" } },
            { "Domain", new List<string> { "domain", "domain name", "domainname" } },
            { "HomeDirectory", new List<string> { "home directory", "homedirectory", "home dir", "homedir", "home", "folder", "home folder", "homefolder", "netshare", "network share", "networkshare" } },
            { "HomeDrive", new List<string> { "home drive", "homedrive", "drive letter", "drive", "letter" } },
            { "Script", new List<string> { "script", "powershell" } }
        };

        public static string GetPattern(string key) {
            if (patternDictionary.ContainsKey(key)) {
                return "(" + String.Join("|", patternDictionary[key].ToArray()) + ")";
            }
            throw new ArgumentException($"Pattern for key '{key}' not found.");
        }

        public static IEnumerable<string> GetKeys() {
            return patternDictionary.Keys;
        }
    }
}