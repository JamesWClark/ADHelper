using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Text.RegularExpressions;

namespace ADHelper.Tasks {
	class Task_Batch {

		private List<string> badSamAccountNames = new List<string>();
		private Config.Options opts;

		public Task_Batch(Config.Options options) {
			opts = options;
		}

		public void Run() {
			try {
				//open a users csv and start reading it
				StreamReader reader = new StreamReader(opts.CsvPath, true);

				string success_file_path = $"succeeded.{DateTime.Now.ToFileTime()}.csv";
				string fail_file_path = $"failed.{DateTime.Now.ToFileTime()}.csv";


				//if headers, burn the first line
				if (opts.InDataHeaders) {
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
					string domain = email.Split('@')[1];
					string password = columns[8];

					// cleaning with regex
					if (opts.UsernameRegex.Length > 0) {
						samAccountName = Regex.Replace(samAccountName, opts.UsernameRegex, "");
						fname = Regex.Replace(fname, opts.UsernameRegex, "");
						lname = Regex.Replace(lname, opts.UsernameRegex, "");
					}

					// bad decision? ignores email field from input file
					email = samAccountName + opts.Suffix + "@" + domain;

					try {
						using (var context = new PrincipalContext(ContextType.Domain, opts.Domain, opts.DistinguishedName)) {
							// only if setting passwords but not creating accounts

							switch(opts.Task) {
								case "create_users":
									using (var user = new UserPrincipal(context)) {
										// new accounts only
										user.SamAccountName = samAccountName;
										user.GivenName = fname;
										user.Surname = lname;
										user.EmailAddress = email;
										user.SetPassword(password);
										user.Enabled = true;
										user.Save();
										using (var tw = new StreamWriter(success_file_path, true)) {
											tw.WriteLine($"{fname},{lname},{email},{samAccountName},{password}");
											Console.WriteLine("created: " + email);
										}
									}
									break;
								case "set_passwords":
									using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)) {
										user.SetPassword(password);
										using (var tw = new StreamWriter(success_file_path, true)) {
											tw.WriteLine($"{fname},{lname},{email},{samAccountName},{password}");
											Console.WriteLine("password set: " + email);
										}
									}
									break;
								default:
									// i think this will never throw
									throw new ArgumentException($"Unsupported Task: {opts.Task}");
                            }
						}
					} catch (Exception ex) {
						Console.WriteLine("failed: " + email);
						Console.WriteLine(ex.Message);
						using (var tw = new StreamWriter(fail_file_path, true)) {
							tw.WriteLine($"{samAccountName},{email},{ex.Message}");
						}
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
