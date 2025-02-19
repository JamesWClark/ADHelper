# Import required modules
Import-Module ActiveDirectory

# Function to set AD user password
function Set-ADUserPasswordFromCsv {
    param (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$UserFields
    )

    try {
        # Check for required fields
        if (-not $UserFields.SamAccountName) {
            throw "SamAccountName is missing"
        }
        if (-not $UserFields.Password) {
            throw "Password is missing"
        }

        # Set the password
        Set-ADAccountPassword -Identity $UserFields.SamAccountName -Reset -NewPassword (ConvertTo-SecureString $UserFields.Password -AsPlainText -Force)

        # Require password change at next login if specified
        if ($UserFields.PwdResetRequired -eq "TRUE") {
            Set-ADUser -Identity $UserFields.SamAccountName -ChangePasswordAtLogon $true
        }

        Write-Host "Successfully set password for user: $($UserFields.SamAccountName)"
    }
    catch {
        Write-Error "Failed to set password for user $($UserFields.SamAccountName): $_"
        throw
    }
}

# Main script
param (
    [string]$CsvPath = "users.csv"
)

$csv = Import-Csv $CsvPath
foreach ($user in $csv) {
    try {
        Set-ADUserPasswordFromCsv -UserFields $user
    }
    catch {
        Write-Error "Error processing user $($user.SamAccountName): $_"
    }
}