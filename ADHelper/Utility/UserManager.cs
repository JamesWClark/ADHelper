using System;
using System.DirectoryServices.AccountManagement;

namespace ADHelper.Utility {
    class UserManager {
        private PrincipalContext _context;

        public UserManager(string domain, string distinguishedName) {
            _context = new PrincipalContext(ContextType.Domain, domain, distinguishedName);
        }

        public void CreateUser(string firstName, string lastName, string samAccountName, string email, string password) {
            using (var user = new UserPrincipal(_context)) {
                user.Name = $"{firstName} {lastName}";
                user.SamAccountName = samAccountName;
                user.UserPrincipalName = $"{samAccountName}@{_context.ConnectedServer}";
                user.GivenName = firstName;
                user.DisplayName = $"{firstName} {lastName}";
                user.Surname = lastName;
                user.EmailAddress = email;
                user.SetPassword(password);
                user.Enabled = true;
                user.Save();
            }
        }

        public void SetPassword(string samAccountName, string password) {
            using (var user = UserPrincipal.FindByIdentity(_context, IdentityType.SamAccountName, samAccountName)) {
                user.SetPassword(password);
            }
        }
    }
}