# ADHelper

## Step 1: Prepare the Data
- It is likely helpful to have an import ID (student ID, employee ID, etc) to match with other data later.
- Create a column for SAMAccount (concatenate existing columns).
- Create a column for email address (concatenate SAMAccount with desired email suffix).
- Remove unecessary columns.

## Step 2: Sanitize User Input
Consider some examples of inputs that would be invalid or potentially undesirable for an email address:

Nick Name | Last Name | E-Mail | Reason
--- | --- | --- | ---
Connor | O'Brien | ConnorO'Brien28\@amdg.rockhursths.edu | Apostrophe
Axel |Garcia-Soto | AxelGarcia-Soto28\@amdg.rockhursths.edu | Hyphen
Aaron |De La Cruz Benavides | AaronDe La Cruz Benavides28\@amdg.rockhursths.edu | Spaces
SAMSON |BRADDOCK | SAMSONBRADDOCK28\@amdg.rockhursths.edu | Capitalization
T.O. |Redmond | T.O.Redmond28\@amdg.rockhursths.edu | Periods

Here are some potential fixes:
- Use find and replace to search for and replace hyphens, apostrophes, and spaces with nothing.
- Manually fix capitalization.

Nick Name | Last Name | E-Mail | Fix
--- | --- | --- | ---
Connor | OBrien | ConnorOBrien28\@amdg.rockhursths.edu | Delete apostrophe
Axel |GarciaSoto | AxelGarciaSoto28\@amdg.rockhursths.edu | Delete hyphen
Aaron |DeLaCruzBenavides | AaronDeLaCruzBenavides28\@amdg.rockhursths.edu | Delete spaces
Samson |Braddock | SamsonBraddock28\@amdg.rockhursths.edu | Fix capitalization
TO |Redmond | TORedmond28\@amdg.rockhursths.edu | Delete periods

## Step 3: Download the ADHelper Utility and Config File
Download and extract the zip file:  
https://github.com/JamesWClark/ADHelper/blob/main/Release/ADHelper.zip?raw=true


## Step 3: Export the Data
Export your data to CSV format, placing it in the same folder as the above zip.

## Step 4: Run a Task
Run with the following tasks: `creat_users` or `set_passwords`

`./ADHelper.exe -csv users.csv -xml config.xml -task create_users`  
`./ADHelper.exe -csv users.csv -xml config.xml -task set_passwords`  

## Appendix

The XML config looks like this:

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

