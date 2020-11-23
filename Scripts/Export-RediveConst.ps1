param (
    [Parameter(Mandatory)]
    [string]
    $Program
)

Import-Module $PSScriptRoot/Save-RedivePool.psm1

$csv = Import-Csv .\manifest\consttext_assetmanifest -Header Path, MD5, Category, Length
$null = New-Item -ItemType Directory "consttext" -Force

foreach ($item in $csv) {
    $null = Save-RedivePool -MD5 ($item.MD5)
    & $Program extract consttext --source $item.MD5 --json ./consttext/consttext.json --yaml ./consttext/consttext.yaml
}
