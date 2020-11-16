param (
    $ManifestPath = './manifest/'
)
Import-Module $PSScriptRoot/Save-RedivePool.psm1
Get-ChildItem $ManifestPath -Exclude '*moviemanifest', 'sound*manifest' | Get-ManifestItem | ForEach-Object { $_ } | ForEach-Object -Parallel {Import-Module $using:PSScriptRoot/Save-RedivePool.psm1; Save-ManifestItem $_}
Get-ChildItem $ManifestPath 'moviemanifest' | Get-ManifestItem | ForEach-Object { $_ } | ForEach-Object -Parallel {Import-Module $using:PSScriptRoot/Save-RedivePool.psm1; Save-ManifestItem $_ -Type Movie}
Get-ChildItem $ManifestPath 'sound2manifest' | Get-ManifestItem | ForEach-Object { $_ } | ForEach-Object -Parallel {Import-Module $using:PSScriptRoot/Save-RedivePool.psm1; Save-ManifestItem $_ -Type Sound}
