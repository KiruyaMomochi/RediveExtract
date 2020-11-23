[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    [ValidateSet("Manifest", "Database", "Storytext", "ConstText")]
    $Type
)

if (git status -s)
{
    $json = Get-Content "./config.json" | ConvertFrom-Json
    $time = [System.TimeZoneInfo]::ConvertTimeBySystemTimeZoneId((Get-Date), 'Taipei Standard Time').ToString("yyyy-MM-dd HH:mm:ss")
    $message = "${Type}: $($json.Version -join '.')($($json.TruthVersion)) at $time"

    git status -s
    git config user.name 'KiruyaMomochi'
    git config user.email 'KiruyaMomochi@users.noreply.github.com'
    
    if ($Type -eq "Manifest") {
        git add .
    }
    if ($Type -eq "Database") {
        git add db/
    }
    if ($Type -eq "Storytext") {
        git add storytext/
        git add storydata/
    }
    if ($Type -eq "ConstText") {
        git add consttext/
    }
    
    git commit -m $message
    git push
}
