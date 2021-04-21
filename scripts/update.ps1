#!/usr/bin/env pwsh

$Binary = /home/kyaru/KiruyaMomochi/RediveExtract/RediveExtract/bin/Release/net5.0/RediveExtract



pushd /home/kyaru/KiruyaMomochi/RediveExtract/ && git pull
Import-Module /home/kyaru/KiruyaMomochi/RediveExtract/Scripts/ngdive.psm1
Expand-NewAssets -Program $Binary -Commit (Get-Content /home/kyaru/KiruyaMomochi/.commit) -ManifestDirectory /home/kyaru/KiruyaMomochi/RediveData/manifest/ -AssetPath /home/kyaru/KiruyaMomochi/RawData/ -OutputDirectory /home/kyaru/KiruyaMomochi/ExtractedData/ > /home/kyaru/KiruyaMomochi/ExtractedData/updatelog; git rev-parse --verify HEAD > /home/kyaru/KiruyaMomochi/.commit
popd

