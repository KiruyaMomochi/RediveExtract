Import-Module $PSScriptRoot/Save-RedivePool.psm1

$null = New-Item -ItemType Directory "storydata", "storydata/yaml", "storydata/json" -Force

$updated = git log -p -1 .\manifest\storydata_assetmanifest | Select-String '^\+(.*)' | % {$_.Matches.Groups[1].Value} | ConvertFrom-Csv -Header Path, MD5, Category, Length
$storydata = $updated | Where-Object Path -Match 'storydata_\d+.unity3d'
foreach ($item in $storydata) {
    $id = [regex]::Match($item.Path, 'storydata_(\d+).unity3d').Groups[1].Value

    Write-Host $id
    
    $null = Save-RedivePool -MD5 ($item.MD5)
    python $PSScriptRoot/export_storytext.py $item.MD5 "$id.bytes"
    dotnet run --project "$PSScriptRoot/../RediveExtract" --configuration Release -- deserialize --input "$id.bytes" --json "storydata/json/$id.json" --yaml "storydata/yaml/$id.yaml"
}

$all = Import-Csv .\manifest\storydata_assetmanifest -Header Path, MD5, Category, Length
$storydata = $all | Where-Object Path -Match 'storydata_\d+.unity3d'
foreach ($item in $storydata) {
    $id = [regex]::Match($item.Path, 'storydata_(\d+).unity3d').Groups[1].Value
    if (Test-Path "storydata/json/${id}.json") {
        continue
    }

    Write-Host $id
    
    $null = Save-RedivePool -MD5 ($item.MD5)
    python $PSScriptRoot/export_storytext.py $item.MD5 "$id.bytes"
    dotnet run --project "$PSScriptRoot/../RediveExtract" --configuration Release -- deserialize --input "$id.bytes" --json "storydata/json/$id.json" --yaml "storydata/yaml/$id.yaml"
}
