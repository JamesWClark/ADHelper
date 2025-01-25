using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

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
                    // SetProperty(newUser, "description", userFields);
                    // SetProperty(newUser, "physicalDeliveryOfficeName", userFields);
                    // SetProperty(newUser, "telephoneNumber", userFields);
                    // SetProperty(newUser, "streetAddress", userFields);
                    // SetProperty(newUser, "l", userFields);
                    // SetProperty(newUser, "st", userFields);
                    // SetProperty(newUser, "postalCode", userFields);
                    // SetProperty(newUser, "mobile", userFields);
                    // SetProperty(newUser, "title", userFields);
                    // SetProperty(newUser, "department", userFields);
                    // SetProperty(newUser, "company", userFields);
                    // SetProperty(newUser, "manager", userFields);

                    newUser.CommitChanges();

                    // Set the password
                    newUser.Invoke("SetPassword", new object[] { userFields["Password"] });

                    // Enable the account
                    int val = (int)newUser.Properties["userAccountControl"].Value;
                    newUser.Properties["userAccountControl"].Value = val & ~0x2; // Enable account

                    newUser.CommitChanges();

                    Console.WriteLine($"User {userFields["SamAccountName"]} created successfully.");
                }
            }
        }

        // // Access the DirectoryEntry to set additional attributes
        // DirectoryEntry directoryEntry = (DirectoryEntry)user.GetUnderlyingObject();

        // // Set additional fields if they exist
        // 
        // 
        // if (userFields.ContainsKey("TelephoneNumber")) directoryEntry.Properties["telephoneNumber"].Value = userFields["TelephoneNumber"];
        // if (userFields.ContainsKey("Street")) directoryEntry.Properties["streetAddress"].Value = userFields["Street"];
        // if (userFields.ContainsKey("City")) directoryEntry.Properties["l"].Value = userFields["City"];
        // if (userFields.ContainsKey("State")) directoryEntry.Properties["st"].Value = userFields["State"];
        // if (userFields.ContainsKey("PostalCode")) directoryEntry.Properties["postalCode"].Value = userFields["PostalCode"];
        // if (userFields.ContainsKey("Mobile")) directoryEntry.Properties["mobile"].Value = userFields["Mobile"];
        // if (userFields.ContainsKey("JobTitle")) directoryEntry.Properties["title"].Value = userFields["JobTitle"];
        // if (userFields.ContainsKey("Department")) directoryEntry.Properties["department"].Value = userFields["Department"];
        // if (userFields.ContainsKey("Company")) directoryEntry.Properties["company"].Value = userFields["Company"];
        // if (userFields.ContainsKey("ManagerName")) directoryEntry.Properties["manager"].Value = userFields["ManagerName"];

        // // Commit the changes to the directory
        // directoryEntry.CommitChanges();

        private void SetProperty(DirectoryEntry entry, string propertyName, Dictionary<string, string> userFields) {
            entry.Properties[propertyName].Value = userFields.ContainsKey(propertyName) && !string.IsNullOrEmpty(userFields[propertyName]) ? userFields[propertyName] : null;
        }

        public void SetPassword(string samAccountName, string password) {
            Console.WriteLine($"SetPassword called with: {samAccountName}");
            try {
                using (var user = UserPrincipal.FindByIdentity(_context, samAccountName)) {
                    if (user != null) {
                        user.SetPassword(password);
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