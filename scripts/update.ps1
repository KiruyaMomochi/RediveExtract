#!/usr/bin/env pwsh
[CmdletBinding()]
param (
    [Parameter(Mandatory)][string]$Program
)

Import-Module (Join-Path $PSScriptRoot 'ngdive.psm1')

<#
.SYNOPSIS
Use git to check if a file is updated or created.
#>
function FileUpdated {
    git status $args[0] -s
}

& $Program fetch --step
while (FileUpdated './manifest') {
    if (FileUpdated './manifest/masterdata2_assetmanifest') {
        Import-Manifest ./manifest/masterdata2_assetmanifest 
        | Save-AssetItem -OutputDirectory . 
        | Expand-AssetItem -Program $Program -OutputDirectory ./db
    }

    if (FileUpdated './manifest/storydata2_assetmanifest') {
        Get-GitAddLines -Path ./manifest/storydata2_assetmanifest -Last 1
        | ConvertFrom-Manifest
        | Where-Object Path -Like '*storydata_[0-9]*'
        | Save-AssetItem -OutputDirectory .
        | Expand-AssetItem -Program $Program -OutputDirectory ./storydata


        Get-GitAddLines -Path ./manifest/storydata2_assetmanifest -Last 1
        | ConvertFrom-Manifest
        | Where-Object Path -Like '*storydata_*movie_[0-9]*'
        | Save-AssetItem -OutputDirectory .
        | Expand-AssetItem -Program $Program -OutputDirectory ./subtitle
    }

    if (FileUpdated './manifest/consttext2_assetmanifest') {
        Import-Manifest ./manifest/consttext2_assetmanifest
        | Save-AssetItem -OutputDirectory '.'
        | Expand-AssetItem -Program $Program -OutputDirectory ./consttext
    }

    Invoke-RediveCommit -Type Manifest
    Invoke-RediveCommit -Type Database
    Invoke-RediveCommit -Type StoryData
    Invoke-RediveCommit -Type Subtitle
    Invoke-RediveCommit -Type ConstText

    & $Program fetch --step
}
