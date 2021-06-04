# ADHelper

This utility was written in C# for the purpose of creating lists of active directory users and setting their passwords.

It runs from command line only. Download a copy, then use it as follows.

This is not a signed executable and your security software will warn you about using it. 

Download: https://github.com/JamesWClark/ADHelper/blob/main/Release/ADHelper.zip?raw=true

Unzip and run with the following tasks: `creat_users` or `set_passwords`

`./ADHelper.exe -csv users.csv -xml config.xml -task create_users`  
`./ADHelper.exe -csv users.csv -xml config.xml -task set_passwords`  

Our primary work flow is to create the users in active directory, run google apps directory sync, verify google apps password sync is running correctly, and finally run the set password task to sync local network and google cloud login passwords.

To use, we first need a table of data for users with headers fixed in this order. This can be created in Excel or Google Sheets but must be saved to CSV format for this application.

FirstName 0 | NickName1 | MidName 2 | LastName 3 | FirstLast 4 | Email 5 | Sam 6 | 7? | Password 8
--- | --- | --- | --- | --- | --- | --- | --- | ---
Crash | Crash | | Test | CrashTest | CrashTest25@amdg.rockhursths.edu | CrashTest | | ABC123
Test | Henry | | Dummy | HenryDummy | HenryDummy25@amdg.rockhursths.edu | HenryDummy | | DEF456

Secondly, we need to establish our configuration. This is a complete config.xml example. Copy, paste, and edit this in xml format.

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
	  <domain>student.rockhurst.int</domain>
	  <distinguishedName>OU=2025,OU=Highly Managed,OU=Users,OU=Student.Greenlease,DC=student,DC=rockhurst,DC=int</distinguishedName>
	  <usernames>
		<regexFilter>[^A-Za-z0-9-]</regexFilter>
		<suffix>25</suffix>
	  </usernames>
	  <csv>
		<headers>true</headers>
	  </csv>
	</configuration>

Some info about each field follows...
	
 * `domain` - the active directory domain we are connecting to
 * `distinguishedName` - the name of the active directory org unit where users are to be created
 * `usernames/regexFilter` - email addresses and usernames will be purged according to the regex filter. leave blank if not desired.
 * `usernames/suffix` - a value ended to the end of every username, eg MarkBayhylle becomes MarkBayhylle25
 * `csv/headers` - true if the csv file has headers, false otherwise
	
For more info about regex, paste the above example or test others at regex101.com