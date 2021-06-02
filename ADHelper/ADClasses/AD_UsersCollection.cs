using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Text.RegularExpressions;

namespace ADHelper.ADClasses {
	class AD_UsersCollection {

		private List<DirectoryEntry> users = new List<DirectoryEntry>();
		private List<string> badSamAccountNames = new List<string>();
		private Config.Options opts;
		private bool hasHeaders = true;

		/// <summary>
		/// map users in AD to those listed in a CSV file
		/// </summary>
		/// <param name="csv"></param>
		public AD_UsersCollection(Config.Options options) {
			opts = options;

			try {
				//open a users csv and start reading it
				StreamReader reader = new StreamReader(options.CsvPath, true);

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
					string email = columns[5];
					string samAccountName = columns[6];
					string password = columns[8];

					DirectoryEntry user = GetUser(samAccountName);
					if (user != null) {
						users.Add(user);
					} else {
						badSamAccountNames.Add(count + ": " + samAccountName);
					}
				}
			} catch (IOException e) {
				Console.WriteLine(e.Message);
			}
		}

		private DirectoryEntry GetUser(string UserName) {
			DirectoryEntry de = ADClasses.AD_DirectoryEntry.GetDirectoryObject(opts);
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot = de;

			deSearch.Filter = "(&(objectClass=user)(SAMAccountName=" + UserName + "))";
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResult results = deSearch.FindOne();

			if (!(results == null)) {
				de = new DirectoryEntry(
					results.Path, 
					opts.Username,
					opts.Password, 
					AuthenticationTypes.Secure
				);
				return de;
			} else {
				return null;
			}
		}

		/*
		 * there will inevitably be a case where mapping from one system into another does not occur perfectly
		 * such that some users will have punctuation and others will not, so be careful setting a rule in the application that 
		 * requires all or nothing, thus creating a dead lock scenario
		 */

		/// <summary>
		/// removes punctuation from names
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private string RemovePunctuation(string s) {
			return Regex.Replace(s, "[^A-Za-z0-9]", "");
		}

		public List<DirectoryEntry> Users {
			get { return users; }
		}
		public List<string> BadSamAccountNames {
			get { return badSamAccountNames; }
		}
	}
}
