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

# Function to create AD user
function New-ADUserFromCsv {
    param (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$UserFields
    )

    try {
        # Basic user parameters
        $userParams = @{
            SamAccountName = $UserFields.SamAccountName
            GivenName = $UserFields.FirstName 
            Surname = $UserFields.LastName
            Name = "$($UserFields.FirstName) $($UserFields.LastName)"
            DisplayName = "$($UserFields.FirstName) $($UserFields.LastName)"
            EmailAddress = $UserFields.Email
            AccountPassword = (ConvertTo-SecureString $UserFields.Password -AsPlainText -Force)
            Enabled = $true
        }

        # UserPrincipalName logic
        $userParams.UserPrincipalName = if ($UserFields.UserPrincipalName) { 
            "$($UserFields.UserPrincipalName)@$($UserFields.UPNSuffix)" 
        } elseif ($UserFields.UPNSuffix) { 
            "$($UserFields.SamAccountName)@$($UserFields.UPNSuffix)" 
        } else { 
            "$($UserFields.SamAccountName)@$env:USERDNSDOMAIN" 
        }

        # Optional parameters
        if ($UserFields.Description) { $userParams.Description = $UserFields.Description }
        if ($UserFields.Office) { $userParams.Office = $UserFields.Office }
        if ($UserFields.TelephoneNumber) { $userParams.OfficePhone = $UserFields.TelephoneNumber }
        if ($UserFields.MobileNumber) { $userParams.MobilePhone = $UserFields.MobileNumber }
        if ($UserFields.Street) { $userParams.StreetAddress = $UserFields.Street }
        if ($UserFields.City) { $userParams.City = $UserFields.City }
        if ($UserFields.State) { $userParams.State = $UserFields.State }
        if ($UserFields.PostalCode) { $userParams.PostalCode = $UserFields.PostalCode }
        if ($UserFields.JobTitle) { $userParams.Title = $UserFields.JobTitle }
        if ($UserFields.Department) { $userParams.Department = $UserFields.Department }
        if ($UserFields.Company) { $userParams.Company = $UserFields.Company }
        if ($UserFields.DistinguishedName) { $userParams.Path = $UserFields.DistinguishedName }

        # Create the user
        New-ADUser @userParams -PassThru

        # Set home directory if specified
        if ($UserFields.HomeDirectory) {
            $homePath = $UserFields.HomeDirectory.Replace("%username%", $UserFields.SamAccountName)
            Set-ADUser -Identity $UserFields.SamAccountName -HomeDirectory $homePath -HomeDrive $UserFields.HomeDrive
            
            # Create home directory if it doesn't exist
            if (!(Test-Path $homePath)) {
                New-Item -Path $homePath -ItemType Directory
                $acl = Get-Acl $homePath
                $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($UserFields.SamAccountName, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
                $acl.AddAccessRule($accessRule)
                Set-Acl -Path $homePath -AclObject $acl
            }
        }

        # Set manager if specified
        if ($UserFields.ManagerName) {
            $manager = Get-ADUser -Identity $UserFields.ManagerName
            Set-ADUser -Identity $UserFields.SamAccountName -Manager $manager
        }

        # Set password change required if specified
        if ($UserFields.PwdResetRequired -eq "TRUE") {
            Set-ADUser -Identity $UserFields.SamAccountName -ChangePasswordAtLogon $true
        }

        # Execute script if specified
        if ($UserFields.Script) {
            $script = $UserFields.Script.Replace("%username%", $UserFields.SamAccountName)
            try {
                Invoke-Expression $script
                Write-Host "Successfully executed script for user: $($UserFields.SamAccountName)"
            }
            catch {
                Write-Error "Failed to execute script for user $($UserFields.SamAccountName): $_"
            }
        }

        Write-Host "Successfully created user: $($UserFields.Email)"
    }
    catch {
        Write-Error "Failed to create user $($UserFields.Email): $_"
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
    $requiredColumns = @('SamAccountName', 'FirstName', 'LastName', 'Email', 'Password')
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
        New-ADUserFromCsv -UserFields $user
    }
    catch {
        Write-Error "Error processing user $($user.Email): $_"
    }
}