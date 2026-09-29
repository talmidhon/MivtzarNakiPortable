param([Parameter(Mandatory=$true)][string]$Folder)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath($Folder)
$app=Join-Path $root 'App'
$manifest=Get-Content -LiteralPath (Join-Path $app 'portable.manifest.json') -Encoding UTF8 -Raw | ConvertFrom-Json
$disk=@{}
foreach($file in Get-ChildItem -LiteralPath $app -Recurse -File) { $disk['App/'+$file.FullName.Substring($app.Length+1).Replace('\','/')]=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
foreach($p in $manifest.Files.PSObject.Properties) { if($disk['App/'+$p.Name] -ne $p.Value) { throw "Manifest mismatch $($p.Name)" } }
if($disk.Count -ne @($manifest.Files.PSObject.Properties).Count+1) { throw 'Unexpected App files' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipPath=$root+'-win-x64.zip'
$zip=[IO.Compression.ZipFile]::OpenRead($zipPath)
try {
 $seen=@{}
 foreach($entry in $zip.Entries) {
  if($seen.ContainsKey($entry.FullName) -or -not $disk.ContainsKey($entry.FullName)) { throw 'Unexpected ZIP entry' }
  $stream=$entry.Open(); $sha=[Security.Cryptography.SHA256]::Create()
  try { $hash=[BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') } finally { $stream.Dispose(); $sha.Dispose() }
  if($hash -ne $disk[$entry.FullName]) { throw "ZIP mismatch $($entry.FullName)" }
  $seen[$entry.FullName]=$true
 }
 if($seen.Count -ne $disk.Count) { throw 'Incomplete ZIP' }
} finally { $zip.Dispose() }
[pscustomobject]@{Version=$manifest.Version;AppBytes=(Get-ChildItem -LiteralPath $app -Recurse -File | Measure-Object Length -Sum).Sum;ZipBytes=(Get-Item -LiteralPath $zipPath).Length;ZipSha256=(Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash;Files=$disk.Count;AllHashesMatch=$true} | ConvertTo-Json
