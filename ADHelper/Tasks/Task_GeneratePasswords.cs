using System;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Text;
using ADHelper.ADClasses;

namespace ADHelper.Tasks {
	class Task_GeneratePasswords {

		private int length;
		private string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345678901234567890123456789";
		private AD_UsersCollection users;
		private StreamWriter writer;
		private string uniformPassword = String.Empty;

		public Task_GeneratePasswords(ADClasses.AD_UsersCollection users) {
			this.users = users;
		}

		public Task_GeneratePasswords(ADClasses.AD_UsersCollection users, int length) {
			this.length = length;
			this.users = users;
		}
		/// <summary>
		/// Sets a uniform password on all AD distinguishedName properties passed in users collection.
		/// </summary>
		/// <example>
		/// //the 8 is erroneous. sets all passwords to @school1
		/// Tasks.Task_GeneratePasswords task = new Tasks.Task_GeneratePasswords(users, 8, "@school1");</example>
		/// <param name="users"></param>
		/// <param name="length"></param>
		/// <param name="password"></param>
		public Task_GeneratePasswords(ADClasses.AD_UsersCollection users, int length, string password) {
			this.length = length;
			this.users = users;
			this.uniformPassword = password;
		}
		/// <summary>
		/// invokes a SetPassword for each user in ad_userscollection object
		/// output: user_passwords.csv in bin folder
		/// </summary>
		public void Run() {

			//will throw null if called after the first constructor

			writer = new StreamWriter("user_passwords.csv", true);
			foreach (DirectoryEntry user in users.Users) {
				Console.WriteLine(user.Path);
				string newPassword;
				if(uniformPassword == String.Empty) { // a uniform password was not provided
					newPassword = GenerateSinglePassword(length);
				} else {
					newPassword = uniformPassword;
				}
				user.Invoke("SetPassword", new object[] { newPassword });
				writer.WriteLine(user.Path + "," + newPassword);
			}
			foreach (string name in users.BadSamAccountNames) {
				Console.WriteLine(name);
			}
			writer.Close();
		}

		/// <summary>
		/// generates random character sequence where index 1 is upper, 2 is lower, 3 is numeric
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		private string GenerateSinglePassword(int length) {
			StringBuilder builder = new StringBuilder();
			Random random = new Random();
			//rule: there must be one lower, one upper, and one number
			//upper: always capitalize first character = between index of A and Z
			builder.Append(chars[random.Next(chars.IndexOf('A'), chars.IndexOf('Z'))]);
			//lower: always lowercase second character = between index of a and z
			builder.Append(chars[random.Next(chars.IndexOf('a'), chars.IndexOf('z'))]);
			//numeric: always use number third character = between index of Z+1 and chars.length
			builder.Append(chars[random.Next(chars.IndexOf('Z') + 1, chars.Length - 1)]);
			//the rest can be any random
			for (int i = 3; i < length; i++) {
				builder.Append(chars[random.Next(0, chars.Length)]);
			}
			return builder.ToString();
		}
		//hard coded way to call a password change based on user provided password.
		//hard-coded to set as "old" and "new"
		private static void ChangePW(DirectoryEntry entry) {
			try {
				// Change the password.
				entry.Invoke("ChangePassword", new object[] { "old", "new" });
			} catch (Exception excep) {
				Console.WriteLine("Error changing password. Reason: " + excep.Message + "\n" + excep.InnerException);
			}
		}
	}
}
