Import-Module $PSScriptRoot/Save-RedivePool.psm1
Get-ChildItem ./manifest/masterdata_assetmanifest | Get-ManifestItem | ForEach-Object { $_ } | ForEach-Object -Parallel {Import-Module $using:PSScriptRoot/Save-RedivePool.psm1; Save-ManifestItem $_}
