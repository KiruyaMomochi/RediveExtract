function Save-RedivePool {
    param (
        [Parameter(Mandatory, ValueFromPipelineByPropertyName)]
        [string]
        $MD5,
        [Parameter(ValueFromPipelineByPropertyName)]
        [string]
        $Path = $MD5,
        [Parameter()]
        [ValidateSet("AssetBundles", "Movie", "Sound")]
        [string]
        $Type = "AssetBundles"
    )

    $HashPre = $MD5.Substring(0, 2)

    if ((Test-Path $Path) -and (Get-FileHash $Path -Algorithm MD5).Hash -like $MD5) {
        return;
    }

    $dir = Split-Path $Path
    if ($dir) {
        if ((Test-Path $dir) -and -not (Test-Directory $dir)) {
            throw [System.ArgumentException]("$dir exists but it's not a directory.");
        }
        $null = New-Item -Path $dir -ItemType Directory -Force
    }    

    try {
        $Private:ProgressPreference = "SilentlyContinue"
        $null = Invoke-WebRequest -Uri "https://img-pc.so-net.tw/dl/pool/$Type/$HashPre/$MD5" -OutFile $Path
    }
    catch {
        Write-Host "Error occured when saving ${Path}:"
        Write-Host $_ -ForegroundColor Red
        return;
    }

    return $Path
}

function Get-ManifestItem {
    param (
        [Parameter(Mandatory, ValueFromPipeline)]
        $manifest
    )
    process {
        Import-Csv $manifest -Header Path, MD5, Category, Length
    }
}

function Save-ManifestItem {
    param (
        [Parameter(Mandatory, ValueFromPipeline)]
        $Item,
        [switch]
        $Asynchronous = $false,
        [Parameter()]
        [ValidateSet("AssetBundles", "Movie", "Sound")]
        [string]
        $Type = "AssetBundles"
    )
    process {
        Save-RedivePool -MD5 $Item.MD5 -Path $Item.Path -Type $Type
    }
}

function Test-Directory {
    param (
        $Path
    )
    
    return (Test-Path $Path) -and ((Get-Item $Path) -is [System.IO.DirectoryInfo])
}


function Export-RediveStory {
    param (
        [Parameter(Mandatory)]
        [string]
        $Program,
        [switch]
        $FetchAll = $false
    )
    $null = New-Item -ItemType Directory "storydata", "storydata/yaml", "storydata/json" -Force

    if ($FetchAll) {
        $all = Import-Csv .\manifest\storydata_assetmanifest -Header Path, MD5, Category, Length
        $storydata = $all | Where-Object Path -Match 'storydata_\d+.unity3d'
    }
    else {
        $updated = git log -p -1 .\manifest\storydata_assetmanifest | Select-String '^\+(.*)' | ForEach-Object { $_.Matches.Groups[1].Value } | ConvertFrom-Csv -Header Path, MD5, Category, Length
        Write-Host $updated
        $storydata = $updated | Where-Object Path -Match 'storydata_\d+.unity3d'
    }

    foreach ($item in $storydata) {
        $id = [regex]::Match($item.Path, 'storydata_(\d+).unity3d').Groups[1].Value

        Write-Host $id
        $null = Save-RedivePool -MD5 ($item.MD5)
        & $Program extract storydata --source $item.MD5 --json "storydata/json/$id.json" --yaml "storydata/yaml/$id.yaml"
    }
}

function Export-RediveConst {
    param (
        [Parameter(Mandatory)]
        [string]
        $Program
    )
    $csv = Import-Csv .\manifest\consttext_assetmanifest -Header Path, MD5, Category, Length
    $null = New-Item -ItemType Directory "consttext" -Force

    foreach ($item in $csv) {
        $null = Save-RedivePool -MD5 ($item.MD5)
        & $Program extract consttext --source $item.MD5 --json ./consttext/consttext.json --yaml ./consttext/consttext.yaml
    }
}

function Export-RediveDb {
    param (
        [Parameter(Mandatory)]
        [string]
        $Program
    )
    $csv = Import-Csv .\manifest\masterdata_assetmanifest -Header Path, MD5, Category, Length
    $null = New-Item -ItemType Directory "db", "db/csv", "db/json" -Force

    foreach ($item in $csv) {
        $db = $item.MD5 + ".sqlite"
    
        $null = Save-RedivePool -MD5 ($item.MD5)
        & $Program extract database --source $item.MD5 --dest $db
        $tables = sqlite3 $db "SELECT tbl_name FROM sqlite_master WHERE type='table' and tbl_name not like 'sqlite_%'"
        $tables | ForEach-Object {
            Write-Host $_
            sqlite3 $db '.header on' '.mode csv' ".output db/csv/$_.csv" "select * from $_;" '.mode json' ".output db/json/$_.json" "select * from $_;"
        }
    }

}

function Invoke-RediveCommit {
    param (
        [Parameter(Mandatory)]
        [string]
        [ValidateSet("Manifest", "Database", "Storytext", "ConstText")]
        $Type,
        [switch]
        $NoPull = $false
    )

    if (git status -s) {
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
    
        if (-not $NoPull) {
            git pull            
        }
        
        git commit -m $message
        git push
    }
}
