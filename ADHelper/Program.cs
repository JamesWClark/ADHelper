using System;
using System.IO;
using System.Configuration;
using System.DirectoryServices;
using System.Text.RegularExpressions;

namespace ADHelper {
	/// <summary>
	/// 
	/// </summary>
	class Program {

		/**
		 * @TODO read more, learn to use uname/pword auth
		 * https://docs.microsoft.com/en-us/dotnet/api/system.directoryservices.accountmanagement?view=dotnet-plat-ext-5.0
		 */

		// ADHelper.exe -csv test.csv -config config.xml -task create_users
		// ADHelper.exe -csv test.csv -config config.xml -task set_passwords


		// test data https://docs.google.com/spreadsheets/d/1WjtfjSwoOvFsAnMVRwAyEEczGB7-vM4-ZYbqup4Nv2I/edit#gid=0

		static void Main(string[] args) {


			Config.Options opts = new Config.Options(args);

			switch(opts.Task) {
				case "create_users":
					//batch create new users from csv, set email property, set password, enable, join hard-coded ou
					Tasks.Task_BatchCreateUsers create_users = new Tasks.Task_BatchCreateUsers(opts);
					create_users.Run();
					break;
				case "set_passwords":
					Tasks.Task_BatchSetPasswords set_passwords = new Tasks.Task_BatchSetPasswords(opts);
					set_passwords.Run();
					break;
				default:
					break;
            }


			//string dn = "OU=2018,OU=Lightly Managed,OU=Users,OU=Student.Greenlease,DC=student,DC=rockhurst,DC=int";

			/*
			var username = ConfigurationManager.AppSettings["ldap_admin_username"];
			var password = ConfigurationManager.AppSettings["ldap_admin_password"];
			var file = ConfigurationManager.AppSettings["ldap_user_file"]; //this file burns the header
			*/



			// these don't work? batch password set is done from create users script
			/*
			//ADClasses.AD_UsersCollection users = new ADClasses.AD_UsersCollection(file, true);
			//Tasks.Task_GeneratePasswords task = new Tasks.Task_GeneratePasswords(users);
            */

			/* how to write to file...
			string path = @"test.txt";
			using (var tw = new StreamWriter(path, true)) {
				tw.WriteLine("Got it");
            }
			*/
		}
	}
}
