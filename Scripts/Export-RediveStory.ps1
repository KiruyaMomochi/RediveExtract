Import-Module $PSScriptRoot/Get-RedivePool.psm1

$csv = Import-Csv .\manifest\storydata_assetmanifest -Header Path, md5, Category, Length
$null = New-Item -ItemType Directory "storydata", "storydata/yaml", "storydata/json" -Force
$storydata = $csv | Where-Object Path -Match 'storydata_\d+.unity3d'

foreach ($item in $storydata) {
    $id = [regex]::Match($item.Path, 'storydata_(\d+).unity3d').Groups[1].Value
    
    $null = Get-RedivePool -Hash ($item.md5)
    python $PSScriptRoot/export_storytext.py $item.md5 $id
    dotnet run --project "$PSScriptRoot/../RediveExtract" --configuration Release -- deserialize --input $id --json "storydata/json/$id.json" --yaml "storydata/yaml/$id.yaml"
}
