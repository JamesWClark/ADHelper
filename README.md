# ADHelper

## Overview

ADHelper is a tool designed to help automate the creation of Active Directory users from a CSV file. While the original application was written in C#, we recommend using the provided PowerShell script for its simplicity and reliability.

## How to Use the PowerShell Script

1. **Prepare the CSV File**: Ensure your `users.csv` file is formatted correctly. You can download a sample [users.csv](ADHelper/TestData/users.csv) file to get started.

2. **Download the PowerShell Script**: Download the [Create-ADUsers.ps1](PowerShell/Create-ADUsers.ps1) script.

3. **Place Files in the Same Directory**: Ensure both the `users.csv` file and the `Create-ADUsers.ps1` script are in the same directory.

4. **Run the PowerShell Script**: Open PowerShell and navigate to the directory containing the files. Run the script using the following command:

    ```powershell
    .\Create-ADUsers.ps1 -CsvPath "users.csv"   
    ```

    This will read the `users.csv` file and create the users in Active Directory.

## Sample CSV File

Here is a sample `users.csv` file format:

```csv
ImportID,FirstName,LastName,Email,SamAccountName,Password,Description,Office,DistinguishedName,TelephoneNumber,MobileNumber,Street,City,State,PostalCode,JobTitle,Department,Company,HomeDrive,HomeDirectory,ManagerName,Script,PwdResetRequired
1,John,Doe,johndoe@example.com,johndoe,Password123,Trainer,Main Office,"OU=101,OU=Standard Users,OU=Managed Users,DC=domain,DC=local",456-777-5555,123-456-7890,,,,,Trainer,Dark Arts,Acme Corp,H:,\\DC1\Test\%username%,jwclark,Get-ADUser -Identity jwclark -Properties memberof | Select-Object -ExpandProperty memberof | Add-ADGroupMember -Members johndoe,TRUE
```

## Script Details

The `Create-ADUsers.ps1` script reads the `users.csv` file and creates users in Active Directory based on the information provided. It supports setting various user attributes, creating home directories, and running custom scripts for each user.

For more details, you can view the script source code [here](PowerShell/Create-ADUsers.ps1).
