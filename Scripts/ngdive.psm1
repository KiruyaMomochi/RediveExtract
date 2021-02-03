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
  Unity3D
  Usm
  Acb
  Subtitle
  Skip
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
    [AssetTypes]$Type
  )

  begin {
    $null = New-Item -ItemType Directory -Force $OutputDirectory
  }
    
  process {
    $OutputDirectoryFull = (Get-Item $OutputDirectory).FullName
    $Extension = [System.IO.Path]::GetExtension($Path)
    $BaseName = [System.IO.Path]::GetFileNameWithoutExtension($Path)

    if ($null -ne $Type) {
      # Break
    }
    elseif ($Extension -eq '.usm') {
      $Type = [AssetTypes]::Usm
    }
    elseif ($Extension -eq '.acb') {
      $Type = [AssetTypes]::Acb
    }
    elseif ($BaseName -like 'storydata_movie_[0-9]*' -or $BaseName -like 'storydata_tw_movie_[0-9]*') {
      $Type = [AssetTypes]::Subtitle
    }
    elseif ($BaseName -like 'storydata_[0-9]*') {
      $Type = [AssetTypes]::StoryData
    }
    elseif ($BaseName -like 'consttext_*') {
      $Type = [AssetTypes]::ConstText
    }
    elseif ($BaseName -like 'masterdata_*') {
      $Type = [AssetTypes]::Database
    }
    elseif ($Extension -eq 'awb') {
      $Type = [AssetTypes]::Skip
    }
    else {
      $Type = [AssetTypes]::Unity3D
    }

    switch ($Type) {
      ([AssetTypes]::Usm) {
        & $Program extract usm --source $Path --dest $OutputDirectoryFull
        Get-ChildItem "$OutputDirectoryFull/*.wav" | ConvertAudio
        $exportFiles = $OutputDirectory
      }
      ([AssetTypes]::Acb) {
        & $Program extract acb --source $Path --dest $OutputDirectoryFull
        Get-ChildItem "$OutputDirectoryFull/*.wav" | ConvertAudio
        $exportFiles = $OutputDirectory
      }
      ([AssetTypes]::Unity3D) {
        $exportFiles = & $Program extract unity3d --source $Path --dest $OutputDirectoryFull
      }
      ([AssetTypes]::Subtitle) {
        $BaseName = $BaseName.Replace('storydata_', '')
        $dest = Join-Path $OutputDirectoryFull "$BaseName.vtt"
        & $Program extract vtt --source $Path --dest $dest
        $exportFiles = $dest
      }
      ([AssetTypes]::StoryData) {
        $BaseName = $BaseName.Replace('storydata_', '')
        $json = Join-Path $OutputDirectoryFull "json/$BaseName.json"
        $yaml = Join-Path $OutputDirectoryFull "yaml/$BaseName.yaml"
        & $Program extract storydata --source $Path --json $json --yaml $yaml
      }
      ([AssetTypes]::ConstText) {
        $BaseName = $BaseName.Replace('consttext_', '')
        $json = Join-Path $OutputDirectoryFull "$BaseName.json"
        $yaml = Join-Path $OutputDirectoryFull "$BaseName.yaml"
        & $Program extract consttext --source $Path --json $json --yaml $yaml
      }
      ([AssetTypes]::Database) {
        $db = Join-Path $OutputDirectoryFull "$BaseName.sqlite"
      
        & $Program extract database --source $Path --dest $db
        $tables = sqlite3 $db "SELECT tbl_name FROM sqlite_master WHERE type='table' and tbl_name not like 'sqlite_%'"

        $OutputDirectoryFull = $OutputDirectoryFull.Replace("\", "/")
        $csv = "$OutputDirectoryFull/csv"
        $json = "$OutputDirectoryFull/json"

        $null = New-Item -ItemType Directory -Force $csv, $json
        $tables | ForEach-Object {
          Write-Output $_
          sqlite3 $db '.header on' '.mode csv' ".output $csv/$_.csv" "select * from $_;" '.mode json' ".output $json/$_.json" "select * from $_;"
        }
        $exportFiles = $tables
      }
      Default {}
    }

    return $exportFiles
  }
}

function Import-Manifest {
  [OutputType([AssetItem])]
  param (
    [Parameter(Mandatory, ValueFromPipeline)]
    $manifest
  )
  process {
    Import-Csv $manifest -Header Path, MD5, Category, Length
  }
}

function ConvertFrom-Manifest {
  [OutputType([AssetItem])]
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
    
    git status -s
    git config user.name 'KiruyaMomochi'
    git config user.email 'KiruyaMomochi@users.noreply.github.com'

    switch ($Type) {
      ([AssetTypes]::Manifest) { 
        git add manifest/
        git add config.json
      }
      ([AssetTypes]::Database) {  
        git add db/json
        git add db/csv
      }
      ([AssetTypes]::StoryData) {  
        git add storytext/
        git add storydata/
      }
      ([AssetTypes]::ConstText) {  
        git add consttext/
      }
      ([AssetTypes]::Subtitle) {  
        git add subtitle/
      }
    }

    $message = "${Type}: $($json.Version -join '.')($($json.TruthVersion)) at $time"
      
    git commit -m $message
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

function Save-AllAssets {
  param (
    [string]$ManifestDirectory = './manifest/',
    [string]$OutputDirectory = './RawData/'
  )
  Get-ChildItem $ManifestDirectory -Exclude '*moviemanifest', 'sound*manifest', 'manifest_assetmanifest' | Import-Manifest | Save-AssetItem -OutputDirectory $OutputDirectory
  Get-ChildItem $ManifestDirectory 'moviemanifest' | Import-Manifest | Save-AssetItem -OutputDirectory $OutputDirectory -Type Movie
  Get-ChildItem $ManifestDirectory 'sound2manifest' | Import-Manifest | Save-AssetItem -OutputDirectory $OutputDirectory -Type Sound
}

function ConvertAudio {
  param (
    [Parameter(Mandatory, ValueFromPipeline)][System.IO.FileInfo]$Path
  )
  process {
    if ($Path.Exists) {
      $m4aPath = [System.IO.Path]::ChangeExtension($Path.FullName, '.m4a')
      ffmpeg -hide_banner -loglevel warning -y -i $Path -c:a aac -b:a 192k -movflags faststart $m4aPath
      Remove-Item $Path
    }
    else {
      Write-Error "Path $Path not exist"
    }
  }
}

function Expand-AllAssets {
  [CmdletBinding()]
  param (
    [Parameter(Mandatory)][string]$Program,
    [string]$AssetPath = './RawData/',
    [string]$OutputDirectory = './ExtractedData/'
  )
  
  $AssetDirectory = Join-Path $OutputDirectory "Assets"
  $BgmDirectory = Join-Path $OutputDirectory "Bgm"
  $MovieDirectory = Join-Path $OutputDirectory "Movies"
  $SoundDirectory = Join-Path $OutputDirectory "Sounds"
  $VoiceDirectory = Join-Path $OutputDirectory "Voices"

  $null = New-Item -ItemType Directory $AssetDirectory, $BgmDirectory, $MovieDirectory, $SoundDirectory,
  $VoiceDirectory, "$MovieDirectory/t", "$VoiceDirectory/t" -Force
  
  # Expand unity3d files
  Get-ChildItem (Join-Path $AssetPath 'a') | ForEach-Object {
    Write-Output $_.Name
    Expand-AssetItem -Program $Program -OutputDirectory $AssetDirectory -Type Unity3D -Path $_
  }

  # Expand bgm files
  Get-ChildItem (Join-Path $AssetPath 'b') | ForEach-Object {
    Write-Output $_.Name
    $OutPath = Join-Path $BgmDirectory $_.BaseName
    Expand-AssetItem -Program $Program -OutputDirectory $OutPath -Path $_
  }

  # Expand movies
  Get-ChildItem (Join-Path $AssetPath 'm') -Exclude 't' | ForEach-Object {
    Write-Output $_.Name
    $OutPath = Join-Path $MovieDirectory $_.BaseName
    Expand-AssetItem -Program $Program -OutputDirectory $OutPath -Path $_
  }
  Get-ChildItem (Join-Path $AssetPath 'm' 't') | ForEach-Object {
    Write-Output $_.Name
    $OutPath = Join-Path $MovieDirectory 't' $_.BaseName
    Expand-AssetItem -Program $Program -OutputDirectory $OutPath -Path $_
  }

  # Expand sounds
  Get-ChildItem (Join-Path $AssetPath 's') | ForEach-Object {
    Write-Output $_.Name
    $OutPath = Join-Path $SoundDirectory $_.BaseName
    Expand-AssetItem -Program $Program -OutputDirectory $OutPath -Path $_
  }

  # Expand voices
  Get-ChildItem (Join-Path $AssetPath 'v') -Exclude 't' | ForEach-Object {
    Write-Output $_.Name
    $OutPath = Join-Path $VoiceDirectory $_.BaseName
    Expand-AssetItem -Program $Program -OutputDirectory $OutPath -Path $_
  }
  Get-ChildItem (Join-Path $AssetPath 'v' 't') | ForEach-Object {
    Write-Output $_.Name
    $OutPath = Join-Path $VoiceDirectory 't' $_.BaseName
    Expand-AssetItem -Program $Program -OutputDirectory $OutPath -Path $_
  }
}

# Get-GitAddLines -Path .\manifest\storydata_assetmanifest -Last 1 | ConvertTo-AssetItem | Where-Object Path -Like '*storydata_[0-9]*' | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '233')
# Get-GitAddLines -Path .\manifest\storydata_assetmanifest -Last 1 | ConvertTo-AssetItem | Where-Object Path -Like '*storydata_movie_[0-9]*' | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '233')
# Get-GitAddLines -Path .\manifest\storydata_assetmanifest -Last 1 | ConvertTo-AssetItem | Where-Object Path -Like '*storydata_tw_movie_[0-9]*' | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '233')
# Get-GitAddLines -Path .\manifest\masterdata_assetmanifest -Last 1 | ConvertTo-AssetItem | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '666')
# Import-Manifest .\manifest\consttext_assetmanifest | Save-AssetItem -OutputDirectory (Join-Path $env:TEMP 'test') | Expand-AssetItem -Program C:\Users\xtyzw\Projects\RediveExtract\RediveExtract\bin\Release\net5.0\RediveExtract.exe -OutputDirectory (Join-Path $env:TEMP '666')