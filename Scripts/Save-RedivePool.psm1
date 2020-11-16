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
        return $Path;
    }

    $dir = Split-Path $Path
    if ($dir) {
        if ((Test-Path $dir) -and -not (Test-Directory $dir)) {
            throw [System.ArgumentException]("$dir exists but it's not a directory.");
        }
        $null = New-Item -Path $dir -ItemType Directory -Force
    }    

    $Private:ProgressPreference = "SilentlyContinue"
    $null = Invoke-WebRequest -Uri "https://img-pc.so-net.tw/dl/pool/AssetBundles/$HashPre/$MD5" -OutFile $Path
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
        [Parameter(Mandatory ,ValueFromPipeline)]
        $Item,
        [switch]
        $Asynchronous = $false
    )
    process {
        Save-RedivePool -MD5 $Item.MD5 -Path $Item.Path
    }
}

function Test-Directory {
    param (
        $Path
    )
    
    return (Test-Path $Path) -and ((Get-Item $Path) -is [System.IO.DirectoryInfo])
}
