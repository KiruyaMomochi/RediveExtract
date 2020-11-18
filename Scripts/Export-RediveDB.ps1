Import-Module $PSScriptRoot/Save-RedivePool.psm1

$csv = Import-Csv .\manifest\masterdata_assetmanifest -Header Path, MD5, Category, Length
$null = New-Item -ItemType Directory "db", "db/csv", "db/json" -Force

foreach ($item in $csv) {
    $db = $item.MD5 + ".sqlite"
    
    $null = Save-RedivePool -MD5 ($item.MD5)
    dotnet run --project "$PSScriptRoot/../RediveExtract" --configuration Release -- extract database --source $item.MD5 --dest $db
    $tables = sqlite3 $db "SELECT tbl_name FROM sqlite_master WHERE type='table' and tbl_name not like 'sqlite_%'"
    $tables | ForEach-Object {
        Write-Host $_
        sqlite3 $db '.header on' '.mode csv' ".output db/csv/$_.csv" "select * from $_;" '.mode json' ".output db/json/$_.json" "select * from $_;"
    }
}
