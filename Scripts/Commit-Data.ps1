[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    [ValidateSet("Manifest", "Database", "Storytext")]
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
        git add storytext/ storydata/
    }
    
    git commit -m $message
    git push
}
