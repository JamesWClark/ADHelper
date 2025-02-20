[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [ValidateScript({
        if(-not (Test-Path $_)) {
            throw "CSV file not found: $_"
        }
        if(-not ($_ -match "\.csv$")) {
            throw "File must be a CSV file"
        }
        $true
    })]
    [string]$CsvPath
)

# Verify AD module is available
if (-not (Get-Module -ListAvailable -Name ActiveDirectory)) {
    throw "Active Directory module is not installed. Please install RSAT tools."
}

# Import required modules
Import-Module ActiveDirectory -ErrorAction Stop

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
try {
    $csv = Import-Csv $CsvPath -ErrorAction Stop
    if ($csv.Count -eq 0) {
        throw "CSV file is empty"
    }
    
    # Validate required columns exist
    $requiredColumns = @('SamAccountName', 'Password')
    $missingColumns = $requiredColumns | Where-Object { $_ -notin $csv[0].PSObject.Properties.Name }
    if ($missingColumns) {
        throw "Missing required columns in CSV: $($missingColumns -join ', ')"
    }
} catch {
    Write-Error "Failed to process CSV file: $_"
    exit 1
}

foreach ($user in $csv) {
    try {
        Set-ADUserPasswordFromCsv -UserFields $user
    }
    catch {
        Write-Error "Error processing user $($user.SamAccountName): $_"
    }
}