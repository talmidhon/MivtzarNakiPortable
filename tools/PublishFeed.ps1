param([Parameter(Mandatory=$true)][string]$Tag,[string]$Repository='talmidhon/MivtzarNakiPortable')
$ErrorActionPreference='Stop'
if($Repository -ne 'talmidhon/MivtzarNakiPortable' -or $Tag -notmatch '^v\d+\.\d+\.\d+$') { throw 'Unexpected repository/tag' }
$release=gh api "repos/$Repository/releases/tags/$Tag" | ConvertFrom-Json
if($LASTEXITCODE -ne 0 -or $release.draft -or $release.prerelease) { throw 'Only published stable releases may update the feed' }
$latest=gh api "repos/$Repository/releases/latest" | ConvertFrom-Json
if($LASTEXITCODE -ne 0 -or $latest.tag_name -ne $Tag) { throw 'Feed must point to latest stable release' }
$asset=@($release.assets | Where-Object name -EQ 'MivtzarNaki-win-x64.zip')
$checksum=@($release.assets | Where-Object name -EQ 'SHA256SUMS.txt')
if($asset.Count -ne 1 -or $checksum.Count -ne 1) { throw 'Missing release ZIP/checksum' }
$temp=Join-Path ([IO.Path]::GetTempPath()) ('mivtzar-feed-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
 gh release download $Tag --repo $Repository --dir $temp --pattern 'MivtzarNaki-win-x64.zip' --pattern 'SHA256SUMS.txt'
 if($LASTEXITCODE -ne 0) { throw 'Release asset download failed' }
 $zip=Join-Path $temp 'MivtzarNaki-win-x64.zip'
 $hash=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
 $line=(Get-Content -LiteralPath (Join-Path $temp 'SHA256SUMS.txt') -Raw).Trim()
 if($line -notmatch '^([A-Fa-f0-9]{64})  MivtzarNaki-win-x64\.zip$' -or $Matches[1] -ne $hash) { throw 'Checksum mismatch' }
 Add-Type -AssemblyName System.IO.Compression.FileSystem
 $archive=[IO.Compression.ZipFile]::OpenRead($zip)
 try {
  $entry=$archive.GetEntry('App/portable.manifest.json')
  if(-not $entry) { throw 'Missing folder manifest' }
  $reader=New-Object IO.StreamReader($entry.Open())
  try { $manifest=$reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
  if($manifest.Version -ne $Tag.Substring(1)) { throw 'Release/manifest version mismatch' }
 } finally { $archive.Dispose() }
 $oldPath=Join-Path (Split-Path $PSScriptRoot -Parent) 'version.json'
 if(Test-Path -LiteralPath $oldPath) {
  $old=Get-Content -LiteralPath $oldPath -Raw -Encoding UTF8 | ConvertFrom-Json
  if([version]$old.latest_version -gt [version]$manifest.Version) { throw 'Metadata downgrade rejected' }
 }
 $feed=[ordered]@{latest_version=$manifest.Version;download_url=$asset[0].browser_download_url;sha256=$hash;message='עדכון מבצר נקי — תיקייה ניידת מלאה'}
 $feed | ConvertTo-Json | Set-Content -LiteralPath $oldPath -Encoding UTF8
 Write-Output "Verified published release $Tag, SHA256=$hash"
} finally {
 # Known, freshly created temporary directory; no repository or user data is removed.
 if([IO.Path]::GetFullPath($temp).StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()),[StringComparison]::OrdinalIgnoreCase)) { Remove-Item -LiteralPath $temp -Recurse -Force }
}
