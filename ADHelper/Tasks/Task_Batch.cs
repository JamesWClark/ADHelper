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

		public string alphatize(string dirty) {
			/*
			string mapDirty = "ŠŽšžŸÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖÙÚÛÜÝàáâãäåçèéêëìíîïðñòóôõöùúûüýÿ";
			string mapClean = "SZszYAAAAAACEEEEIIIIDNOOOOOUUUUYaaaaaaceeeeiiiidnooooouuuuyy";
			string clean = "";
			bool isCleanCharacter = true;
			for(int i = 0; i < dirty.Length; i++) {
				for(int k = 0; k < mapDirty.Length; k++) {
					if (dirty[i] == mapDirty[k]) {
						clean += mapClean[k];
						isCleanCharacter = false;
						break;
                    }
                }
				if(!isCleanCharacter) {
					isCleanCharacter = true;
				} else {
					clean += dirty[i]; 
                }
            }
			return clean;
			*/
			return dirty;
        }

		public static string WordListPassword(int n) {
			string pw = "";
			Random rand = new Random();
			for (int i = 0; i < n; i++) {
				pw += Config.WordList.Words[rand.Next(0, Config.WordList.Words.Length)];
			}
			String end = "69"; // because high school students
			while (end == "69") {
				end = "" + rand.Next(0, 10) + rand.Next(0, 10);
			}
			return pw + end;
		}

		public void Run() {
			try {
				//open a users csv and start reading it
				StreamReader reader = new StreamReader(opts.CsvPath, true);

				string success_file_path = $"succeeded.{DateTime.Now.ToFileTime()}.csv";
				string fail_file_path = $"failed.{DateTime.Now.ToFileTime()}.csv";

				int indexImportID = 0,
					indexFirstName = 1,
					indexLastName = 2,
					indexSamAccountName = 3,
					indexEmail = 4,
					indexPassword = 5;

				string regexImportID	= Config.Patterns.ImportID;
				string regexFirstName	= Config.Patterns.FirstName;
				string regexLastName	= Config.Patterns.LastName;
				string regexSamAccount	= Config.Patterns.SamAccount;
				string regexEmail		= Config.Patterns.Email;
				string regexPassword	= Config.Patterns.Password;

				string line; // will store each line from the file

				//if headers, burn the first line
				if (opts.InDataHeaders) {
					line = reader.ReadLine();
					string[] columns = line.Split(',');
					
					for (int i = 0; i < columns.Length; i++) {
						string header = columns[i].Trim().ToLower();

						if (Regex.IsMatch(header, regexImportID)) {
							indexImportID = i;
						}
						if (Regex.IsMatch(header, regexFirstName)) {
							indexFirstName = i;
						}
						if (Regex.IsMatch(header, regexLastName)) {
							indexLastName = i;
						}
						if (Regex.IsMatch(header, regexSamAccount)) {
							indexSamAccountName = i;
						}
						if (Regex.IsMatch(header, regexEmail)) {
							indexEmail = i;
						}
						if (Regex.IsMatch(header, regexPassword)) {
							indexPassword = i;
						}
					}
				}
				//read the rest of the file
				int count = 0;
				while ((line = reader.ReadLine()) != null) {

					string[] columns = line.Split(',');

					//try without removing punctuation
					string importID			= alphatize(columns[indexImportID].Trim());
					string fname			= alphatize(columns[indexFirstName].Trim());
					string lname			= alphatize(columns[indexLastName].Trim());
					string samAccountName	= alphatize(columns[indexSamAccountName].Trim());
					string email			= alphatize(columns[indexEmail].Trim());

					string domain			= email.Split('@')[1];
					string password = "";
					if(opts.GeneratePasswords) {
						password = WordListPassword(2);
                    } else {
						password = columns[indexPassword].Trim();
					}

					/* let's not do this w/ regex... 
					// cleaning with regex
					if (opts.UsernameRegex.Length > 0) {
						samAccountName = Regex.Replace(samAccountName, opts.UsernameRegex, "");
						fname = Regex.Replace(fname, opts.UsernameRegex, "");
						lname = Regex.Replace(lname, opts.UsernameRegex, "");
					}*/

					// bad decision? ignores email field from input file
					// email = samAccountName + opts.Suffix + "@" + domain;

					try {
						using (var context = new PrincipalContext(ContextType.Domain, opts.Domain, opts.DistinguishedName)) {
							// only if setting passwords but not creating accounts

							switch(opts.Task) {
								case "create_users":
									using (var user = new UserPrincipal(context)) {
										// new accounts only
										user.Name = fname + " " + lname;
										user.SamAccountName = samAccountName;
										user.UserPrincipalName = $"{samAccountName}@{opts.Domain}";
										user.GivenName = fname;
										user.DisplayName = fname + " " + lname;
										user.Surname = lname;
										user.EmailAddress = email;
										user.SetPassword(password);
										user.Enabled = true;
										user.Save();
										using (var tw = new StreamWriter(success_file_path, true)) {
											if(count == 0) {
												tw.WriteLine("Import ID,First Name,Last Name,Email,AD Login,Password");
                                            }
											tw.WriteLine($"{importID},{fname},{lname},{email},{samAccountName},{password}");
											Console.WriteLine("created: " + email);
										}
									}
									break;
								case "set_passwords":
									using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)) {
										user.SetPassword(password);
										using (var tw = new StreamWriter(success_file_path, true)) {
											tw.WriteLine($"{fname},{lname},{email},{samAccountName},{password}");
											Console.WriteLine($"Setting password for record: {fname},{lname},{email},{samAccountName},{password}");
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
					count++;
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
