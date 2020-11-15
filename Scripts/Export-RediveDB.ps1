Import-Module $PSScriptRoot/Save-RedivePool.psm1

$csv = Import-Csv .\manifest\masterdata_assetmanifest -Header Path, MD5, Category, Length
$null = New-Item -ItemType Directory "db", "db/csv", "db/lines", "db/json" -Force

foreach ($item in $csv) {
    $db = $item.MD5 + ".sqlite"
    
    $null = Save-RedivePool -MD5 ($item.MD5)
    python $PSScriptRoot/export_db.py $item.MD5 $db
    $tables = sqlite3 $db "SELECT tbl_name FROM sqlite_master WHERE type='table' and tbl_name not like 'sqlite_%'"
    $tables | ForEach-Object {
        Write-Host $_
        sqlite3 $db '.header on' '.mode csv' ".output db/csv/$_.csv" "select * from $_;"
        sqlite3 $db '.header on' '.mode json' ".output db/json/$_.json" "select * from $_;"
    }
}
