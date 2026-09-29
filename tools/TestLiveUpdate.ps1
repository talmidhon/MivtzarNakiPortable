param([string]$Baseline='artifacts/MivtzarNaki-delivery',[string]$Delivery='artifacts/MivtzarNaki-0.1.1')
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
if(@(Get-Process MivtzarNaki -ErrorAction SilentlyContinue).Count) { throw 'Another app/helper is running' }
$baselineRoot=[IO.Path]::GetFullPath((Join-Path $repo $Baseline))
$deliveryRoot=[IO.Path]::GetFullPath((Join-Path $repo $Delivery))
$old=Get-Content -LiteralPath (Join-Path $baselineRoot 'App/portable.manifest.json') -Encoding UTF8 -Raw | ConvertFrom-Json
$new=Get-Content -LiteralPath (Join-Path $deliveryRoot 'App/portable.manifest.json') -Encoding UTF8 -Raw | ConvertFrom-Json
if($old.Version -ne '0.1.0' -or [version]$new.Version -le [version]$old.Version) { throw 'Two genuine versions required' }
$harness=Join-Path $repo 'artifacts/live-update-driver'
dotnet build (Join-Path $repo 'tools/LiveUpdateFixture/LiveUpdateFixture.csproj') -c Release -o $harness --nologo
if($LASTEXITCODE -ne 0) { throw 'Live driver build failed' }
$root=Join-Path $repo ('artifacts/live-update-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
Copy-Item -LiteralPath (Join-Path $baselineRoot 'App') -Destination (Join-Path $root 'App') -Recurse
# Only this isolated copy gets a driver assembly. Baseline Core/Windows/runtime are unchanged.
Copy-Item -LiteralPath (Join-Path $harness 'MivtzarNaki.dll') -Destination (Join-Path $root 'App/MivtzarNaki.dll') -Force
$old.Files.'MivtzarNaki.dll'=(Get-FileHash -LiteralPath (Join-Path $root 'App/MivtzarNaki.dll')).Hash
$old | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $root 'App/portable.manifest.json') -Encoding UTF8
New-Item -ItemType Directory -Path (Join-Path $root 'Data'),(Join-Path $root 'OfflinePayloads') | Out-Null
$feed='https://raw.githubusercontent.com/talmidhon/MivtzarNakiPortable/main/version.json'
@{Theme='Dark';UpdateFeed=$feed} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $root 'Data/settings.json') -Encoding UTF8
'preserved-live-log' | Set-Content -LiteralPath (Join-Path $root 'Data/diagnostic.log')
'preserved-payload-fixture' | Set-Content -LiteralPath (Join-Path $root 'OfflinePayloads/preserve.txt')
$target=Join-Path $root 'App/MivtzarNaki.exe'
$parent=Start-Process -FilePath $target -ArgumentList '--live-update', $feed -WorkingDirectory $env:WINDIR -WindowStyle Hidden -PassThru
$done=Join-Path $root '.updates/helper-result.txt'
for($n=0;$n -lt 240 -and -not (Test-Path -LiteralPath $done);$n++) {
 $parent.Refresh()
 if($parent.HasExited -and $parent.ExitCode -ne 0) { throw "Live driver failed with exit $($parent.ExitCode); evidence $root" }
 Start-Sleep -Milliseconds 500
}
if(-not (Test-Path -LiteralPath $done) -or (Get-Content -LiteralPath $done -Raw).Trim() -ne '0') { throw "Live update failed; evidence $root" }
if(-not $parent.WaitForExit(15000)) { throw 'Parent did not exit' }
$parent.Refresh()
if($parent.ExitCode -ne 0) { throw 'Parent failed' }
$apps=@(Get-Process MivtzarNaki -ErrorAction SilentlyContinue | Where-Object Path -EQ $target)
if($apps.Count -ne 1) { throw 'Expected one new real application' }
foreach($app in $apps) {
 for($n=0;$n -lt 30 -and $app.MainWindowHandle -eq 0;$n++) { Start-Sleep -Milliseconds 200; $app.Refresh() }
 if($app.MainWindowHandle -ne 0) { if(-not $app.CloseMainWindow() -or -not $app.WaitForExit(15000)) { throw 'Test app did not close' } }
 else { $app.Kill(); $app.WaitForExit() } # Only our known test process, no Defender action.
}
$installed=Get-Content -LiteralPath (Join-Path $root 'App/portable.manifest.json') -Encoding UTF8 -Raw | ConvertFrom-Json
if($installed.Version -ne $new.Version) { throw 'Wrong installed version' }
foreach($p in $new.Files.PSObject.Properties) { if((Get-FileHash -LiteralPath (Join-Path (Join-Path $root 'App') $p.Name)).Hash -ne $p.Value) { throw "Installed mismatch: $($p.Name)" } }
if((Get-FileHash -LiteralPath (Join-Path $root 'App/portable.manifest.json')).Hash -ne (Get-FileHash -LiteralPath (Join-Path $deliveryRoot 'App/portable.manifest.json')).Hash) { throw 'Manifest differs from published delivery' }
$settings=Get-Content -LiteralPath (Join-Path $root 'Data/settings.json') -Encoding UTF8 -Raw | ConvertFrom-Json
if($settings.Theme -ne 'Dark' -or $settings.UpdateFeed -ne $feed) { throw 'Settings lost' }
if(-not (Get-Content -LiteralPath (Join-Path $root 'Data/diagnostic.log') -Raw).StartsWith('preserved-live-log')) { throw 'Old logs lost' }
if((Get-Content -LiteralPath (Join-Path $root 'OfflinePayloads/preserve.txt') -Raw).Trim() -ne 'preserved-payload-fixture') { throw 'Payload lost' }
$backups=@(Get-ChildItem -LiteralPath (Join-Path $root '.updates') -Directory -Filter 'previous-*')
if($backups.Count -ne 1) { throw 'Missing previous-version backup' }
foreach($p in $old.Files.PSObject.Properties) { if((Get-FileHash -LiteralPath (Join-Path $backups[0].FullName $p.Name)).Hash -ne $p.Value) { throw 'Old app backup damaged' } }
if(Test-Path -LiteralPath (Join-Path $root '.updates/MivtzarNaki.next.zip')) { throw 'Candidate not cleaned' }
$result=[ordered]@{BaselineVersion='0.1.0';PublishedVersion=$installed.Version;LiveFeed=$feed;RealGithubDownload=$true;RealBaselineUpdateEngine=$true;RealHelperAndXamlAcknowledgement=$true;AppFilesVerified=@($new.Files.PSObject.Properties).Count;PreviousAppBackupVerified=$true;PayloadSettingsLogsPreserved=$true;Root=$root}
$result | ConvertTo-Json | Tee-Object -FilePath (Join-Path $root 'results.json')
