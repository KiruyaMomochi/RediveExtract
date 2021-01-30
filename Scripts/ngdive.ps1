enum ManifestItemTypes {
  AssetBundles
  Movie
  Sound
}

enum AssetTypes {
  Manifest
  Database
  StoryData
  ConstText
}

class AssetItem {
  [string] $Md5
  [string] $Path
  [string] $Category
  [int] $Length
}

function Save-AssetItem {
  [CmdletBinding()]
  [OutputType([System.IO.FileInfo])]
  param (
    [Parameter(Mandatory, ValueFromPipelineByPropertyName, ParameterSetName = "each")][string] $Md5,
    [Parameter(ValueFromPipelineByPropertyName, ParameterSetName = "each")][string] $Path = $Md5,
    [Parameter(ValueFromPipelineByPropertyName, ParameterSetName = "each")][string] $Category,
    [Parameter(Mandatory, ValueFromPipeline, ParameterSetName = "item")][AssetItem]$Item,
    [Parameter()][string]$OutputDirectory,
    [Parameter()][ManifestItemTypes]$Type = [ManifestItemTypes]::AssetBundles
  )
  
  begin {
    $wc = New-Object System.Net.WebClient
  }

  process {
    # Check the existence of item
    if ($Item) {
      $Md5 = $Item.Md5
      $Path = $Item.Path
      $Category = $Item.Category
    }

    # Append output directory
    if ($OutputDirectory) {
      $Path = Join-Path $OutputDirectory $Path
    }
        
    # If MD5 is the same, skip the file
    if ((Test-Path $Path) -and (Get-FileHash $Path -Algorithm MD5).Hash -like $MD5) {
      return $Path
    }
        
    # Create directory
    EnsureDirectory -Directory (Split-Path $Path)

    # Calculate hash prefix
    $HashPre = $MD5.Substring(0, 2)

    try {
      $Private:ProgressPreference = "SilentlyContinue"
      $wc.DownloadFile("https://img-pc.so-net.tw/dl/pool/$Type/$HashPre/$MD5", $Path)
    }
    catch {
      Write-Host "Error occured when saving ${Path}:" -ForegroundColor Red
      Write-Host $_
      return;
    }

    return $Path
  }
}

function Expand-AssetItem {
  [CmdletBinding()]
  param (
    [Parameter(Mandatory)][string]$Program,
    [Parameter(Mandatory, ValueFromPipeline, ValueFromPipelineByPropertyName)][string]$Path,
    [string]$OutputDirectory = ".",
    [ManifestItemTypes]$Type
  )
    
  begin {
    $null = New-Item -ItemType Directory -Force $OutputDirectory
  }
    
  process {
    $OutputDirectoryFull = (Get-Item $OutputDirectory).FullName
    $Extension = [System.IO.Path]::GetExtension($Path)
    $BaseName = [System.IO.Path]::GetFileNameWithoutExtension($Path)

    if ($BaseName -like 'storydata_movie_[0-9]*') {
      $dest = Join-Path $OutputDirectoryFull "$BaseName.vtt"
      & $Program extract vtt --source $Path --dest $dest
      $exportFiles = $dest
    }
    elseif ($Extension -eq 'usm') {
      & $Program extract usm --source $Path --dest $OutputDirectoryFull
      $exportFiles = $OutputDirectory
    }
    elseif ($Extension -eq 'acb') {
      & $Program extract acb --source $Path --dest $OutputDirectoryFull
      $exportFiles = $OutputDirectory
    }
    elseif ($BaseName -match 'storydata_(\d+)') {
      $id = $Matches[1]
      $json = Join-Path $OutputDirectoryFull "json/$id.json"
      $yaml = Join-Path $OutputDirectoryFull "yaml/$id.yaml"
      & $Program extract storydata --source $Path --json $json --yaml $yaml
    }
    elseif ($BaseName -like 'consttext_*') {
      $Suffix = $BaseName.Replace('consttext_', '')
      $json = Join-Path $OutputDirectoryFull "$Suffix.json"
      $yaml = Join-Path $OutputDirectoryFull "$Suffix.yaml"
      & $Program extract consttext --source $Path --json $json --yaml $yaml
    }
    elseif ($BaseName -like 'masterdata_*') {
      $db = Join-Path $OutputDirectoryFull "$BaseName.sqlite"
      
      & $Program extract database --source $Path --dest $db
      $tables = sqlite3 $db "SELECT tbl_name FROM sqlite_master WHERE type='table' and tbl_name not like 'sqlite_%'"

      $OutputDirectoryFull = $OutputDirectoryFull.Replace("\", "/")
      $csv = "$OutputDirectoryFull/csv"
      $json = "$OutputDirectoryFull/json"

      $null = New-Item -ItemType Directory -Force $csv, $json
      $tables | ForEach-Object {
        sqlite3 $db '.header on' '.mode csv' ".output $csv/$_.csv" "select * from $_;" '.mode json' ".output $json/$_.json" "select * from $_;"
      }
      $exportFiles = $tables
    }
    elseif ($Type -eq [ManifestItemTypes]::AssetBundles) {
      $exportFiles = & $Program extract unity3d --source $Path --dest $OutputDirectoryFull
    }

    Join-Path $OutputDirectoryFull '*.wav' -Resolve | ForEach-Object {
      $wav = $_.FullName
      $flac = [System.IO.Path]::ChangeExtension($wav, "flac")
      flac -f $wav -o $flac
      kid3-cli -c "select $wav" -c copy -c "select $flac" -c paste
      Remove-Item $wav
    }

    return $exportFiles
  }
}

function Import-Manifest {
  param (
    [Parameter(Mandatory, ValueFromPipeline)]
    $manifest
  )
  process {
    Import-Csv $manifest -Header Path, MD5, Category, Length
  }
}

function ConvertFrom-Manifest {
  param (
    [Parameter(Mandatory, ValueFromPipeline)]
    [string]
    $InputObject
  )
  process {
    return ConvertFrom-Csv -InputObject $InputObject -Header Path, MD5, Category, Length
  }
}

function EnsureDirectory {
  param (
    [string]
    $Directory
  )
    
  if ($Directory) {
    $null = New-Item -Path $Directory -ItemType Directory -Force
    if (-not (Test-Directory $Directory)) {
      throw [System.ArgumentException]("$Directory exists but is not a directory.");
    }
  }
}

function Test-Directory {
  param (
    [string]
    $Path
  )
  
  return (Get-Item $Path) -is [System.IO.DirectoryInfo]
}

function Invoke-RediveCommit {
  param (
    [Parameter(Mandatory)][AssetTypes]$Type,
    [switch]$NoPull = $false
  )

  if (git status -s) {
    $json = Get-Content "./config.json" | ConvertFrom-Json

    if ($IsLinux) {
      $time = [System.TimeZoneInfo]::ConvertTimeBySystemTimeZoneId((Get-Date), 'Asia/Taipei').ToString("yyyy-MM-dd HH:mm:ss")
    }
    else {
      $time = [System.TimeZoneInfo]::ConvertTimeBySystemTimeZoneId((Get-Date), 'Taipei Standard Time').ToString("yyyy-MM-dd HH:mm:ss")
    }

    $message = "${Type}: $($json.Version -join '.')($($json.TruthVersion)) at $time"

    git status -s
    git config user.name 'KiruyaMomochi'
    git config user.email 'KiruyaMomochi@users.noreply.github.com'
  
    switch ($Type) {
      ([AssetTypes]::Manifest) { 
        git add .
      }
      ([AssetTypes]::Database) {  
        git add db/json
        git add db/yaml
      }
      ([AssetTypes]::StoryData) {  
        git add storytext/
        git add storydata/
      }
      ([AssetTypes]::ConstText) {  
        git add consttext/
      }
    }

    if (-not $NoPull) {
      git pull            
    }
      
    git commit -m $message
    git push
  }
}

function Get-GitAddLines {
  [CmdletBinding()]
  param (
    [Parameter(Mandatory, ValueFromPipeline)]
    [string]
    $Path,
    [Parameter(ParameterSetName = "Diff")]
    [string]
    $Commit,
    [Parameter(ParameterSetName = "Log")]
    [int]
    $Last = 1
  )
  process {
    if ($Commit) {
      $diff = git diff --unified=0 $Commit -- $Path
    }
    else {
      $diff = git log -p "-$Last" $Path
    }
    return ($diff | Select-String -Raw '^\+[^+]' | ForEach-Object { $_.Substring(1) })
  }
}

function Save-AllManifests {
  param (
      $ManifestPath = './manifest/',
      $OutputDirectory
  )
  Get-ChildItem $ManifestPath -Exclude '*moviemanifest', 'sound*manifest', 'manifest_assetmanifest' | Import-Manifest | Save-AssetItem -OutputDirectory $OutputDirectory
  Get-ChildItem $ManifestPath 'moviemanifest' | Import-Manifest | Save-AssetItem -OutputDirectory $OutputDirectory -Type Movie
  Get-ChildItem $ManifestPath 'sound2manifest' | Import-Manifest | Save-AssetItem -OutputDirectory $OutputDirectory -Type Sound
}

# Get-GitAddLines -Path .\manifest\storydata_assetmanifest -Last 1 | ConvertTo-AssetItem | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '233')
# Get-GitAddLines -Path .\manifest\masterdata_assetmanifest -Last 1 | ConvertTo-AssetItem | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '666')
# Import-AssetItem .\manifest\consttext_assetmanifest  | ConvertTo-AssetItem | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '666')