using System;
using System.Configuration;
using System.DirectoryServices;

namespace ADHelper.ADClasses {
    class AD_DirectoryEntry {
        public DirectoryEntry dirEntry;

        AD_DirectoryEntry(Config.Options options) {
            dirEntry = GetDirectoryObject(options);
        }

        public static DirectoryEntry GetDirectoryObject(Config.Options options) {
            DirectoryEntry oDE;
            oDE = new DirectoryEntry(
                options.Domain,
                options.Username,
                options.Password,
                AuthenticationTypes.Secure
            );
            return oDE;
        }

        [Obsolete("GetDirectoryObject() is deprecated. Use GetDirectoryObject(Config.Options) instead.")]
        public static DirectoryEntry GetDirectoryObject() {
            DirectoryEntry oDE;
            oDE = new DirectoryEntry(
                ConfigurationManager.AppSettings["ldap_root"],
                ConfigurationManager.AppSettings["ldap_admin_username"],
                ConfigurationManager.AppSettings["ldap_admin_password"],
                AuthenticationTypes.Secure
            );
            return oDE;
        }
    }

}
