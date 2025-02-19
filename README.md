# ADHelper

## Overview

ADHelper is a tool designed to help automate the creation of Active Directory users from a CSV file. While the original application was written in C#, we recommend using the provided PowerShell script for its simplicity and reliability.

## How to Use the PowerShell Script

1. **Prepare the CSV File**: Ensure your `users.csv` file is formatted correctly. You can download a sample [users.csv](ADHelper/TestData/users.csv) file to get started.

2. **Download the PowerShell Script**: Download the [Create-ADUsers.ps1](ADHelper/TestData/Create-ADUsers.ps1) script.

3. **Place Files in the Same Directory**: Ensure both the `users.csv` file and the `Create-ADUsers.ps1` script are in the same directory.

4. **Run the PowerShell Script**: Open PowerShell and navigate to the directory containing the files. Run the script using the following command:

    ```powershell
    .\Create-ADUsers.ps1 -CsvPath "users.csv"   
    ```

    This will read the `users.csv` file and create the users in Active Directory.

## Sample CSV File

Here is a sample `users.csv` file format:

## Script Details

The `Create-ADUsers.ps1` script reads the `users.csv` file and creates users in Active Directory based on the information provided. It supports setting various user attributes, creating home directories, and running custom scripts for each user.

For more details, you can view the script source code [here](ADHelper/TestData/Create-ADUsers.ps1).

## Conclusion

Using the PowerShell script is a reliable and efficient way to manage Active Directory user creation. It simplifies the process and reduces the potential for errors. We recommend using this approach for your AD user management needs.