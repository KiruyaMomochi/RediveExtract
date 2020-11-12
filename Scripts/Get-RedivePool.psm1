function Get-RedivePool {
    param (
        [string]
        $Hash,
        [string]
        $OutFile = $Hash
    )

    $HashPre = $Hash.Substring(0, 2)
    Invoke-WebRequest -Uri "https://img-pc.so-net.tw/dl/pool/AssetBundles/$HashPre/$Hash" -OutFile $OutFile
    return $OutFile
}
