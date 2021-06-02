using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Text.RegularExpressions;

namespace ADHelper.Tasks {
	class Task_BatchSetPasswords {

		bool hasHeaders = true;
		string pathToUserCsv;
		private List<string> badSamAccountNames = new List<string>();
		private Config.Options opts;

		public Task_BatchSetPasswords(Config.Options options) {
			this.pathToUserCsv = options.CsvPath;
			opts = options;
		}

		public void Run() {
			try {
				//open a users csv and start reading it
				StreamReader reader = new StreamReader(pathToUserCsv, true);

				//if headers, burn the first line
				if (hasHeaders) {
					reader.ReadLine();
				}
				//read the rest of the file
				int count = 0;
				string line;
				while ((line = reader.ReadLine()) != null) {
					count++;
					//foreach user in file
					string[] columns = line.Split(',');

					//try without removing punctuation
					string fname = columns[1];
					string lname = columns[3];
					string samAccountName = columns[6];
					string email = columns[5];
					string password = columns[8];

					// save the domain
					//string domain = email.Split('@')[1];

					// cleaning with regex
					string domain = email.Split('@')[1];
					string pattern = "[^-ñA-Za-z0-9]";
					samAccountName = Regex.Replace(samAccountName, pattern, "");
					fname = Regex.Replace(fname, pattern, "");
					lname = Regex.Replace(lname, pattern, "");

					try {
						using (var context = new PrincipalContext(ContextType.Domain, opts.Domain, opts.DistinguishedName)) {
                            // only if setting passwords but not creating accounts
                            using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)) {
								user.SetPassword(password);
							}
						}
						Console.WriteLine("ok: " + email);
					} catch (Exception ex) {
						Console.WriteLine("ex when email = " + email);
						Console.WriteLine(ex.Message);

						badSamAccountNames.Add(samAccountName);
					}
				}
			} catch (IOException e) {
				Console.WriteLine(e.Message);
			}
			Console.WriteLine("\n\nfails:");
			foreach (string s in badSamAccountNames) {
				Console.WriteLine(s);
			}
		}
	}
}
