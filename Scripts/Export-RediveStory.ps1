param (
    [Parameter(Mandatory)]
    [string]
    $Program,
    [switch]
    $FetchAll = $false
)

Import-Module $PSScriptRoot/Save-RedivePool.psm1

$null = New-Item -ItemType Directory "storydata", "storydata/yaml", "storydata/json" -Force

if ($FetchAll) {
    $all = Import-Csv .\manifest\storydata_assetmanifest -Header Path, MD5, Category, Length
    $storydata = $all | Where-Object Path -Match 'storydata_\d+.unity3d'
} else {
    $updated = git log -p -1 .\manifest\storydata_assetmanifest | Select-String '^\+(.*)' | ForEach-Object {$_.Matches.Groups[1].Value} | ConvertFrom-Csv -Header Path, MD5, Category, Length
    $storydata = $updated | Where-Object Path -Match 'storydata_\d+.unity3d'
}

foreach ($item in $storydata) {
    $id = [regex]::Match($item.Path, 'storydata_(\d+).unity3d').Groups[1].Value

    Write-Host $id
    $null = Save-RedivePool -MD5 ($item.MD5)
    & $Program extract storydata --source $item.MD5 --json "storydata/json/$id.json" --yaml "storydata/yaml/$id.yaml"
}
