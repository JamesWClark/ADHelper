using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.IO;
using System.Security.AccessControl;
using System.Diagnostics;

namespace ADHelper.Utility {
    class UserManager {
        private PrincipalContext _context;

        public UserManager(string domain, string distinguishedName) {
            _context = new PrincipalContext(ContextType.Domain, domain, distinguishedName);
        }

         public void CreateUser(Dictionary<string, string> userFields) {
            Console.WriteLine($"CreateUser called with: {string.Join(", ", userFields.Select(kv => kv.Key + "=" + kv.Value))}");
            using (var entry = new DirectoryEntry($"LDAP://{_context.ConnectedServer}/{_context.Container}")) {
                using (var newUser = entry.Children.Add($"CN={userFields["FirstName"]} {userFields["LastName"]}", "user")) {
                    Console.WriteLine("Creating DirectoryEntry object");
        
                    // Mandatory fields
                    newUser.Properties["samAccountName"].Value = userFields["SamAccountName"].Length > 20 ? userFields["SamAccountName"].Substring(0, 20) : userFields["SamAccountName"];
                    newUser.Properties["userPrincipalName"].Value = $"{userFields["SamAccountName"]}@{_context.ConnectedServer}";
                    newUser.Properties["givenName"].Value = userFields["FirstName"];
                    newUser.Properties["sn"].Value = userFields["LastName"];
                    newUser.Properties["displayName"].Value = userFields.ContainsKey("DisplayName") ? userFields["DisplayName"] : $"{userFields["FirstName"]} {userFields["LastName"]}";
                    newUser.Properties["mail"].Value = userFields["Email"];
        
                    // Optional fields
                    SetProperty(newUser, "Description", userFields);
                    SetProperty(newUser, "Office", userFields);
                    SetProperty(newUser, "TelephoneNumber", userFields);
                    SetProperty(newUser, "Street", userFields);
                    SetProperty(newUser, "City", userFields);
                    SetProperty(newUser, "State", userFields);
                    SetProperty(newUser, "PostalCode", userFields);
                    SetProperty(newUser, "Mobile", userFields);
                    SetProperty(newUser, "JobTitle", userFields);
                    SetProperty(newUser, "Department", userFields);
                    SetProperty(newUser, "Company", userFields);
                    SetProperty(newUser, "ManagerName", userFields);
        
                    // Set the manager
                    if (userFields.ContainsKey("ManagerName") && !string.IsNullOrEmpty(userFields["ManagerName"])) {
                        string managerDn = GetManagerDn(userFields["ManagerName"]);
                        newUser.Properties["manager"].Value = managerDn;
                    }
        
                    // Set the home directory and home drive
                    if (userFields.ContainsKey("HomeDrive") && !userFields.ContainsKey("HomeDirectory")) {
                        throw new Exception("HomeDrive is specified without HomeDirectory, which is invalid.");
                    }
        
                    if (userFields.ContainsKey("HomeDirectory") && !string.IsNullOrEmpty(userFields["HomeDirectory"])) {
                        string homeDirectory = userFields["HomeDirectory"].Replace("%username%", userFields["SamAccountName"]);
                        newUser.Properties["homeDirectory"].Value = homeDirectory;
        
                        if (userFields.ContainsKey("HomeDrive") && !string.IsNullOrEmpty(userFields["HomeDrive"])) {
                            newUser.Properties["homeDrive"].Value = userFields["HomeDrive"];
                        }
        
                        // Create the network folder on the server
                        CreateNetworkFolder(homeDirectory, userFields["SamAccountName"]);
                    }
        
                    newUser.CommitChanges();
        
                    // Set the password
                    newUser.Invoke("SetPassword", new object[] { userFields["Password"] });
        
                    // Enable the account
                    int val = (int)newUser.Properties["userAccountControl"].Value;
                    newUser.Properties["userAccountControl"].Value = val & ~0x2; // Enable account
        
                    // Require password change at next login
                    if (userFields.ContainsKey("PwdResetRequired") && bool.TryParse(userFields["PwdResetRequired"], out bool pwdResetRequired) && pwdResetRequired) {
                        newUser.Properties["pwdLastSet"].Value = 0;
                    }
        
                    newUser.CommitChanges();
        
                    Console.WriteLine($"User {userFields["SamAccountName"]} created successfully.");
        
                    // Run the script
                    if (userFields.ContainsKey("Script") && !string.IsNullOrEmpty(userFields["Script"])) {
                        Console.WriteLine("Attempting Script: " + userFields["Script"]);
                        RunPowerShell(userFields["Script"]);
                    }
                }
            }
        }

        private void CreateNetworkFolder(string path, string username) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);

                // Set permissions
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                DirectorySecurity security = directoryInfo.GetAccessControl();
                security.AddAccessRule(new FileSystemAccessRule(username, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                directoryInfo.SetAccessControl(security);

                Console.WriteLine($"Network folder created at {path} for user {username}");
            } else {
                Console.WriteLine($"Network folder already exists at {path}");
            }
        }

        private string GetManagerDn(string managerSamAccountName) {
            using (var searcher = new DirectorySearcher(new DirectoryEntry($"LDAP://{_context.Name}"))) {
                searcher.Filter = $"(&(objectClass=user)(samAccountName={managerSamAccountName}))";
                searcher.PropertiesToLoad.Add("distinguishedName");
        
                var result = searcher.FindOne();
                if (result != null) {
                    return result.Properties["distinguishedName"][0].ToString();
                } else {
                    throw new Exception($"Manager with SamAccountName '{managerSamAccountName}' not found.");
                }
            }
        }

        private static readonly Dictionary<string, string> ADPropertyMap = new Dictionary<string, string> {
            { "Description", "description" },
            { "Office", "physicalDeliveryOfficeName" },
            { "TelephoneNumber", "telephoneNumber" },
            { "Street", "streetAddress" },
            { "City", "l" },
            { "State", "st" },
            { "PostalCode", "postalCode" },
            { "Mobile", "mobile" },
            { "JobTitle", "title" },
            { "Department", "department" },
            { "Company", "company" },
            { "ManagerName", "manager" },
            { "HomeDirectory", "homeDirectory" },
            { "HomeDrive", "homeDrive" },
            { "PwdResetRequired", "pwdLastReset" }
        };

        private void SetProperty(DirectoryEntry entry, string patternKey, Dictionary<string, string> userFields) {
            string adPropertyName = ADPropertyMap[patternKey];
            if (userFields.ContainsKey(patternKey) && !string.IsNullOrEmpty(userFields[patternKey])) {
                entry.Properties[adPropertyName].Value = userFields[patternKey];
            }
        }

        private void RunPowerShell(string script) {
            using (Process process = new Process()) {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output)) {
                    Console.WriteLine($"Output: {output}");
                }

                if (!string.IsNullOrEmpty(error)) {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }


        public void SetPassword(string samAccountName, string password, bool pwdResetRequired = false) {
            Console.WriteLine($"SetPassword called with: {samAccountName}");
            try {
                using (var user = UserPrincipal.FindByIdentity(_context, samAccountName)) {
                    if (user != null) {
                        user.SetPassword(password);
                        if (pwdResetRequired) {
                            user.ExpirePasswordNow();
                        }
                        user.Save();
                        Console.WriteLine($"Password for {samAccountName} set successfully.");
                    } else {
                        Console.WriteLine($"User {samAccountName} not found.");
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error setting password for {samAccountName}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}