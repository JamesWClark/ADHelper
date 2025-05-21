<#
.SYNOPSIS
Sets passwords for Active Directory users from a CSV or Excel file.

.EXAMPLE
.\Set-ADUserPasswords.ps1 -FromFile users.csv
.\Set-ADUserPasswords.ps1 -FromFile users.xlsx
#>

[CmdletBinding()]
param (
    [Parameter(HelpMessage="Enter the path to a CSV or Excel file (e.g., users.csv or users.xlsx)")]
    [Alias("Path")]
    [string]$FromFile
)

if (-not $FromFile) {
    Write-Host ""
    Write-Host "USAGE: .\Set-ADUserPasswords.ps1 -FromFile <users.csv|users.xlsx>" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Example: .\Set-ADUserPasswords.ps1 -FromFile users.xlsx"
    Write-Host ""
    exit 1
}

# Verify AD module is available
if (-not (Get-Module -ListAvailable -Name ActiveDirectory)) {
    throw "Active Directory module is not installed. Please install RSAT tools."
}
Import-Module ActiveDirectory -ErrorAction Stop

# Import CSV or XLSX
if ($FromFile -match '\.xlsx$') {
    if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
        throw "ImportExcel module is not installed. Run 'Install-Module ImportExcel' first."
    }
    Import-Module ImportExcel -ErrorAction Stop
    $data = Import-Excel $FromFile
    $fileType = "Excel"
} else {
    $data = Import-Csv $FromFile -ErrorAction Stop
    $fileType = "CSV"
}

if ($data.Count -eq 0) {
    throw "$fileType file is empty"
}

# Validate required columns exist
$requiredColumns = @('SamAccountName', 'Password')
$missingColumns = $requiredColumns | Where-Object { $_ -notin $data[0].PSObject.Properties.Name }
if ($missingColumns) {
    throw "Missing required columns in ${fileType}: $($missingColumns -join ', ')"
}

# Function to set AD user password
function Set-ADUserPasswordFromCsv {
    param (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$UserFields
    )

    try {
        if (-not $UserFields.SamAccountName) {
            throw "SamAccountName is missing"
        }
        if (-not $UserFields.Password) {
            throw "Password is missing"
        }

        Set-ADAccountPassword -Identity $UserFields.SamAccountName -Reset -NewPassword (ConvertTo-SecureString $UserFields.Password -AsPlainText -Force)

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

foreach ($user in $data) {
    try {
        Set-ADUserPasswordFromCsv -UserFields $user
    }
    catch {
        Write-Error "Error processing user $($user.SamAccountName): $_"
    }
}