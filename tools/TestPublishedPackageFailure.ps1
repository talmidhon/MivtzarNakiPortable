param([string]$Delivery='artifacts/MivtzarNaki-0.1.1')
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
if(@(Get-Process MivtzarNaki -ErrorAction SilentlyContinue).Count) { throw 'Another app/helper is running' }
$deliveryRoot=[IO.Path]::GetFullPath((Join-Path $repo $Delivery))
$root=Join-Path $repo ('artifacts/published-failure-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
Copy-Item -LiteralPath (Join-Path $deliveryRoot 'App') -Destination (Join-Path $root 'App') -Recurse
New-Item -ItemType Directory -Path (Join-Path $root '.updates'),(Join-Path $root 'Data'),(Join-Path $root 'OfflinePayloads') | Out-Null
'{"Theme":"Dark"}' | Set-Content -LiteralPath (Join-Path $root 'Data/settings.json')
'preserved-log' | Set-Content -LiteralPath (Join-Path $root 'Data/diagnostic.log')
'preserved-payload' | Set-Content -LiteralPath (Join-Path $root 'OfflinePayloads/preserve.txt')
gh release download v0.1.1 --repo talmidhon/MivtzarNakiPortable --dir (Join-Path $root '.updates') --pattern MivtzarNaki-win-x64.zip
if($LASTEXITCODE -ne 0) { throw 'Real published asset download failed' }
$source=Join-Path $root '.updates/MivtzarNaki-win-x64.zip'
$feed=Get-Content -LiteralPath (Join-Path $repo 'version.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if((Get-FileHash -LiteralPath $source).Hash -ne $feed.sha256) { throw 'Downloaded published asset mismatch' }
$candidate=Join-Path $root '.updates/MivtzarNaki.next.zip'
Copy-Item -LiteralPath $source -Destination $candidate
$stream=[IO.File]::Open($candidate,[IO.FileMode]::Open,[IO.FileAccess]::Write,[IO.FileShare]::None)
try { $stream.SetLength(1024) } finally { $stream.Dispose() }
$target=Join-Path $root 'App/MivtzarNaki.exe'
$args=@('--apply-update',[int]::MaxValue,('"'+$target+'"'),('"'+$candidate+'"'),$feed.sha256)
$helper=Start-Process -FilePath (Join-Path $deliveryRoot 'App/MivtzarNaki.exe') -ArgumentList $args -WorkingDirectory $env:WINDIR -WindowStyle Hidden -PassThru
if(-not $helper.WaitForExit(30000)) { throw 'Helper timed out' }
$helper.Refresh()
if($helper.ExitCode -ne 1) { throw 'Expected checksum rejection' }
$apps=@(Get-Process MivtzarNaki -ErrorAction SilentlyContinue | Where-Object Path -EQ $target)
if($apps.Count -ne 1) { throw 'Previous good app was not reopened' }
foreach($app in $apps) {
 for($n=0;$n -lt 30 -and $app.MainWindowHandle -eq 0;$n++) { Start-Sleep -Milliseconds 200; $app.Refresh() }
 if($app.MainWindowHandle -ne 0) { if(-not $app.CloseMainWindow() -or -not $app.WaitForExit(15000)) { throw 'Test app did not close' } }
 else { $app.Kill(); $app.WaitForExit() }
}
$manifest=Get-Content -LiteralPath (Join-Path $root 'App/portable.manifest.json') -Encoding UTF8 -Raw | ConvertFrom-Json
foreach($p in $manifest.Files.PSObject.Properties) { if((Get-FileHash -LiteralPath (Join-Path (Join-Path $root 'App') $p.Name)).Hash -ne $p.Value) { throw 'Current good app changed' } }
if((Get-Content -LiteralPath (Join-Path $root 'Data/settings.json') -Raw).Trim() -ne '{"Theme":"Dark"}') { throw 'Settings changed' }
if(-not (Get-Content -LiteralPath (Join-Path $root 'Data/diagnostic.log') -Raw).StartsWith('preserved-log')) { throw 'Log lost' }
if((Get-Content -LiteralPath (Join-Path $root 'OfflinePayloads/preserve.txt') -Raw).Trim() -ne 'preserved-payload') { throw 'Payload lost' }
[pscustomobject]@{RealPublishedAssetDownloaded=$true;OriginalPublishedSha256=$feed.sha256;TruncatedCopyRejected=$true;ExpectedHelperExitCode=$helper.ExitCode;GoodAppFilesPreserved=@($manifest.Files.PSObject.Properties).Count;SettingsLogsPayloadPreserved=$true;Root=$root} | ConvertTo-Json | Tee-Object -FilePath (Join-Path $root 'results.json')
