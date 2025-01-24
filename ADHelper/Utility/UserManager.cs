using System;
using System.DirectoryServices.AccountManagement;

namespace ADHelper.Utility {
    class UserManager {
        private PrincipalContext _context;

        public UserManager(string domain, string distinguishedName) {
            _context = new PrincipalContext(ContextType.Domain, domain, distinguishedName);
        }

        public void CreateUser(string firstName, string lastName, string samAccountName, string email, string password) {
            Console.WriteLine($"CreateUser called with: {firstName}, {lastName}, {samAccountName}, {email}");
            try {
                using (var user = new UserPrincipal(_context)) {
                    Console.WriteLine("Creating UserPrincipal object");
                    user.Name = $"{firstName} {lastName}";
                    user.SamAccountName = samAccountName.Length > 20 ? samAccountName.Substring(0, 20) : samAccountName; // Ensure sAMAccountName is within 20 characters
                    user.UserPrincipalName = $"{samAccountName}@{_context.ConnectedServer}";
                    user.GivenName = firstName;
                    user.DisplayName = $"{firstName} {lastName}";
                    user.Surname = lastName;
                    user.EmailAddress = email;
                    user.SetPassword(password);
                    user.Enabled = true;
                    user.Save();
                    Console.WriteLine($"User {samAccountName} created successfully.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error creating user {samAccountName}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
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