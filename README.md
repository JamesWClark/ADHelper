# ADHelper

## Overview

ADHelper is a tool designed to help automate the creation of Active Directory users from a CSV file. While the original application was written in C#, we recommend using the provided PowerShell script for its simplicity and reliability.

## Quick Start

1. **Download Required Files**
   - [Create-ADUsers.ps1](PowerShell/Create-ADUsers.ps1)
   - [Sample users.csv](PowerShell/users.csv)

   Note: First time running PowerShell scripts? See [Script Security](#script-security-and-windows-protection) section.

2. **Prepare Your CSV File**
   Use the sample CSV as a template and modify for your needs.

3. **Run the Script**
   ```powershell
   .\Create-ADUsers.ps1 -CsvPath "users.csv"
   ```

## Sample CSV Format

```csv
ImportID,FirstName,LastName,Email,SamAccountName,Password,Description,Office,DistinguishedName,TelephoneNumber,MobileNumber,Street,City,State,PostalCode,JobTitle,Department,Company,HomeDrive,HomeDirectory,ManagerName,Script,PwdResetRequired
1,John,Doe,johndoe@example.com,johndoe,Password123,Trainer,Main Office,"OU=101,OU=Standard Users,OU=Managed Users,DC=domain,DC=local",456-777-5555,123-456-7890,,,,,Trainer,Dark Arts,Acme Corp,H:,\\DC1\Test\%username%,jwclark,Get-ADUser -Identity jwclark -Properties memberof | Select-Object -ExpandProperty memberof | Add-ADGroupMember -Members johndoe,TRUE
```

## Advanced Workflows

### Google Workspace Integration

For organizations using Google Workspace (formerly G Suite), here's a recommended workflow:

1. Create AD users using `Create-ADUsers.ps1`
2. Run Google Cloud Directory Sync (GCDS) to create corresponding Google accounts
3. Verify Google Password Sync for Active Directory is installed and running on your domain controller
4. Use `Set-ADUserPasswords.ps1` to update passwords if needed:
    ```powershell
    .\Set-ADUserPasswords.ps1 -CsvPath "passwords.csv"
    ```

## Appendix: Technical Details

### Script Security and Windows Protection

When downloading PowerShell scripts from the internet, Windows marks them as potentially unsafe. Here's what you need to know:

1. **Check if scripts are blocked**:
    ```powershell
    Get-Item .\Create-ADUsers.ps1 -Stream Zone.Identifier -ErrorAction SilentlyContinue
    ```

2. **Unblocking options**:
    - Using PowerShell (Recommended):
      ```powershell
      Unblock-File -Path .\Create-ADUsers.ps1
      ```
    - Using Windows Explorer: Right-click → Properties → Check "Unblock"
    - Using PowerShell policy (Not recommended):
      ```powershell
      Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
      ```

### PowerShell Execution Policy

Before running scripts for the first time, you'll need to set the execution policy. Here are the recommended approaches:

1. **For current session only** (safest):
    ```powershell
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
    ```

2. **For current user**:
    ```powershell
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
    ```

3. **For entire system** (requires admin):
    ```powershell
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
    ```

To check your current policy:
```powershell
Get-ExecutionPolicy