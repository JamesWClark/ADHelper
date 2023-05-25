# ADHelper

This utility was written in C# for the purpose of creating lists of active directory users and setting their passwords.

It runs from command line only. Download a copy, then use it as follows.

This is not a signed executable and your security software will warn you about using it. 

Download: https://github.com/JamesWClark/ADHelper/blob/main/Release/ADHelper.zip?raw=true

Unzip and run with the following tasks: `creat_users` or `set_passwords`

`./ADHelper.exe -csv users.csv -xml config.xml -task create_users`  
`./ADHelper.exe -csv users.csv -xml config.xml -task set_passwords`  

Our primary work flow is to...

1) run the `create_users` to populate active directory
2) manually run google apps directory sync
3) verify google apps password sync is running correctly
4) and finally, run the `set_password` task to sync local network and google cloud login passwords.

To use, we first need a table of data for users. This can be created in Excel or Google Sheets but must be saved to CSV format for this application. This app will attempt to detect headings but to be safe use the following in any order... 

Import ID | FirstName | LastName | SamAccount | Email | Password
--- | --- | --- | --- | --- | ---
0001 | Crash | Test | CrashTest25 | CrashTest25@amdg.rockhursths.edu | Abcd1234
0002 | Henry | Dummy | HenryDummy25 | HenryDummy25@amdg.rockhursths.edu | Defg4567

Secondly, we need to establish our configuration. This is a complete config.xml example. Copy, paste, and edit this in xml format.

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
	  <domain>student.rockhurst.int</domain>
	  <distinguishedName>OU=2026,OU=Highly Managed,OU=Users,OU=Student.Greenlease,DC=student,DC=rockhurst,DC=int</distinguishedName>
	  <csv>
		<headers>true</headers>
	  </csv>
	  <password>
		<generator>true</generator>
	  </password>
	</configuration>

Some info about each field follows...
	
 * `domain` - the active directory domain we are connecting to
 * `distinguishedName` - the name of the active directory org unit where users are to be created
 * `csv/headers` - true if the csv file has headers, false otherwise
 * `password/generator - will create new passwords if set to `true`, set to `false` if you want to use passwords from the csv file

