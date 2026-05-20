<#
.SYNOPSIS
Generates passwords for users in a CSV or Excel file.

.EXAMPLE
.\Generate-Passwords.ps1 -FromFile users.csv -WordList wordlist.txt
#>

param (
    [Parameter(HelpMessage = "Enter the name or path of a CSV or Excel file (e.g., users.csv or users.xlsx)")]
    [Alias("Path")]
    [string]$FromFile,

    [Parameter(HelpMessage = "Enter the path to the word list text file")]
    [string]$WordList
)

# ---------------------------------------------------------------------------
# Configuration — adjust these to change the password format
# ---------------------------------------------------------------------------
$NumWords  = 2   # number of words to concatenate
$NumDigits = 2   # number of trailing digits
# ---------------------------------------------------------------------------

if (-not $FromFile -or -not $WordList) {
    Write-Host ""
    Write-Host "USAGE: .\Generate-Passwords.ps1 -FromFile <users.csv|users.xlsx> -WordList <wordlist.txt>" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Example: .\Generate-Passwords.ps1 -FromFile users.xlsx -WordList wordlist.txt"
    Write-Host ""
    exit 1
}

# Load word list
$words = Get-Content $WordList | Where-Object { $_.Trim() -ne "" }

# Import CSV or XLSX
$worksheetName = $null
if ($FromFile -match '\.xlsx$') {
    if (-not (Get-Module -ListAvailable -Name ImportExcel)) {
        Write-Host ""
        Write-Host "ERROR: The ImportExcel module is required to process Excel files." -ForegroundColor Red
        Write-Host "Please run (as administrator): Install-Module ImportExcel" -ForegroundColor Yellow
        Write-Host ""
        exit 1
    }
    Import-Module ImportExcel -ErrorAction Stop

    # Get the first worksheet name
    $excelInfo = Get-ExcelSheetInfo -Path $FromFile
    $worksheetName = $excelInfo[0].Name
    $users = Import-Excel -Path $FromFile -WorksheetName $worksheetName
} else {
    $users = Import-Csv $FromFile -ErrorAction Stop
}

# 
$users = $users | Where-Object { $_.SamAccountName -or $_.Email -or $_.Name }

foreach ($user in $users) {
    # Skip blank rows
    if (-not ($user.SamAccountName -or $user.Email -or $user.Name)) { continue }

    # Generate words and digits
    $wordPart   = -join (1..$NumWords  | ForEach-Object { $words | Get-Random })
    do {
        $digitPart = -join ((0..9) | Get-Random -Count $NumDigits)
    } while ($digitPart -eq "67" -or $digitPart -eq "69")

    # Combine for password
    $password = "$wordPart$digitPart"

    # Update password property
    if ($user.PSObject.Properties.Match('Password')) {
        $user.Password = $password
    } else {
        $user | Add-Member -NotePropertyName 'Password' -NotePropertyValue $password -Force
    }

    # After first user, try to export and catch errors
    if ($i -eq 0) {
        try {
            if ($FromFile -match '\.xlsx$') {
                $users | Export-Excel -Path $FromFile -WorksheetName $worksheetName -NoNumberConversion * -ClearSheet
            } else {
                $users | Export-Csv $FromFile -NoTypeInformation
            }
        } catch {
            Write-Host ""
            Write-Host "ERROR: Unable to write to file '$FromFile'. It may be open in another program or locked." -ForegroundColor Red
            Write-Host "Please close the file and try again." -ForegroundColor Yellow
            Write-Host ""
            exit 1
        }
    }

    # Log to console (adjust identifier as needed)
    $id = $user.SamAccountName
    if (-not $id) { $id = $user.Email }
    if (-not $id) { $id = $user.Name }
    Write-Host "Generated password for $($id): $($password)" -ForegroundColor Cyan
}

# Export updated file (overwrite original)
if ($FromFile -match '\.xlsx$') {
    $users | Export-Excel -Path $FromFile -WorksheetName $worksheetName -NoNumberConversion * -ClearSheet
} else {
    $users | Export-Csv $FromFile -NoTypeInformation
}