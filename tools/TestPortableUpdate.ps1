param([string]$Folder = 'artifacts/MivtzarNaki-delivery')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$delivery = [IO.Path]::GetFullPath((Join-Path $repo $Folder))
if (@(Get-Process MivtzarNaki -ErrorAction SilentlyContinue).Count -ne 0) { throw 'An app/helper is already running. Do not overlap tests or interrupt a user instance.' }
$failureBuild = Join-Path $repo 'artifacts/launch-failure-fixture'
& dotnet build (Join-Path $repo 'tools/UpdateLaunchFixture/UpdateLaunchFixture.csproj') -c Release -o $failureBuild --nologo
if ($LASTEXITCODE -ne 0) { throw 'Launch-failure fixture build failed.' }
$archive = $delivery + '-win-x64.zip'
$testRoot = Join-Path $repo ('artifacts/updater-fixture-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot | Out-Null
$helper = Join-Path $testRoot 'helper'
Copy-Item -LiteralPath (Join-Path $delivery 'App') -Destination $helper -Recurse
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$results = @()
foreach ($scenario in @('success','rollback','session')) {
    $root = Join-Path $testRoot $scenario
    New-Item -ItemType Directory -Path $root | Out-Null
    Copy-Item -LiteralPath (Join-Path $delivery 'App') -Destination (Join-Path $root 'App') -Recurse
    $target = Join-Path $root 'App/MivtzarNaki.exe'
    $manifestPath = Join-Path $root 'App/portable.manifest.json'
    $manifest = Get-Content -Encoding UTF8 -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $manifest.Version = '0.0.9'
    if ($scenario -eq 'session') {
        Copy-Item -LiteralPath (Join-Path $failureBuild 'MivtzarNaki.dll') -Destination (Join-Path $root 'App/MivtzarNaki.dll') -Force
        $manifest.Files.'MivtzarNaki.dll' = (Get-FileHash -LiteralPath (Join-Path $root 'App/MivtzarNaki.dll')).Hash
    }
    $manifest | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 -LiteralPath $manifestPath
    foreach ($name in @('Data','OfflinePayloads','.updates')) { New-Item -ItemType Directory -Path (Join-Path $root $name) | Out-Null }
    Set-Content -LiteralPath (Join-Path $root 'Data/preserve.txt') -Value 'user-data'
    Set-Content -LiteralPath (Join-Path $root 'Data/settings.json') -Value '{"Theme":"Dark"}'
    Set-Content -LiteralPath (Join-Path $root 'Data/diagnostic.log') -Value 'log-fixture'
    Set-Content -LiteralPath (Join-Path $root 'OfflinePayloads/preserve.txt') -Value 'payload-fixture'
    Set-Content -LiteralPath (Join-Path $root '.updates/update-error.log') -Value 'old failure'
    $candidate = Join-Path $root '.updates/MivtzarNaki.next.zip'
    if ($scenario -eq 'success') { Copy-Item -LiteralPath $archive -Destination $candidate }
    else {
        $bad = Join-Path $root 'bad-package'
        New-Item -ItemType Directory -Path $bad | Out-Null
        Copy-Item -LiteralPath (Join-Path $delivery 'App') -Destination (Join-Path $bad 'App') -Recurse
        $badAssembly = Join-Path $bad 'App/MivtzarNaki.dll'
        if ($scenario -eq 'rollback') { Copy-Item -LiteralPath (Join-Path $failureBuild 'MivtzarNaki.dll') -Destination $badAssembly -Force }
        $badManifestPath = Join-Path $bad 'App/portable.manifest.json'
        $badManifest = Get-Content -Encoding UTF8 -LiteralPath $badManifestPath -Raw | ConvertFrom-Json
        $badManifest.Files.'MivtzarNaki.dll' = (Get-FileHash -LiteralPath $badAssembly).Hash
        if ($scenario -eq 'session') { $badManifest.Version = '0.2.0' }
        $badManifest | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 -LiteralPath $badManifestPath
        $badZip = [IO.Compression.ZipFile]::Open($candidate, [IO.Compression.ZipArchiveMode]::Create)
        try {
            Get-ChildItem -LiteralPath $bad -Recurse -File | ForEach-Object {
                $name = $_.FullName.Substring($bad.Length + 1).Replace('\','/')
                [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($badZip, $_.FullName, $name) | Out-Null
            }
        } finally { $badZip.Dispose() }
    }
    $hash = (Get-FileHash -LiteralPath $candidate).Hash
    # Real helper process; fixtures only. No Defender install/repair switches are used.
    $arguments = @('--apply-update', [int]::MaxValue, ('"' + $target + '"'), ('"' + $candidate + '"'), $hash)
    if ($scenario -eq 'session') {
        $sourceZip = Join-Path $root 'source.zip'
        Copy-Item -LiteralPath $candidate -Destination $sourceZip
        $arguments = @('--exercise-update', ('"' + $sourceZip + '"'), $hash)
        $process = Start-Process -FilePath $target -ArgumentList $arguments -WorkingDirectory $env:WINDIR -WindowStyle Hidden -PassThru
    } else {
        $process = Start-Process -FilePath (Join-Path $helper 'MivtzarNaki.exe') -ArgumentList $arguments -WorkingDirectory $env:WINDIR -WindowStyle Hidden -PassThru
    }
    if (-not $process.WaitForExit(60000)) { throw "Helper timed out: PID $($process.Id)" }
    $process.Refresh()
    $expectedCode = if ($scenario -eq 'rollback') { 1 } else { 0 }
    if ($process.ExitCode -ne $expectedCode) { throw "Unexpected helper exit: $($process.ExitCode)" }
    if ($scenario -eq 'session') {
        $helperResult = Join-Path $root '.updates/helper-result.txt'
        for ($attempt=0; $attempt -lt 200 -and -not (Test-Path -LiteralPath $helperResult); $attempt++) { Start-Sleep -Milliseconds 200 }
        if (-not (Test-Path -LiteralPath $helperResult) -or (Get-Content -LiteralPath $helperResult -Raw).Trim() -ne '0') { throw 'Session-started helper did not succeed.' }
        if (@(Get-ChildItem -LiteralPath (Join-Path $root '.updates') -Directory -Filter 'verified-*').Count -ne 0) { throw 'Verified staging directory was not cleaned.' }
    }
    $launched = @(Get-Process MivtzarNaki -ErrorAction SilentlyContinue | Where-Object Path -EQ $target)
    if ($launched.Count -ne 1) { throw "Expected one launched/restored app, got $($launched.Count)" }
    foreach ($app in $launched) {
        # Give the restored application time to create its window; closing does not operate its UI controls.
        for ($attempt=0; $attempt -lt 30 -and $app.MainWindowHandle -eq 0; $attempt++) { Start-Sleep -Milliseconds 200; $app.Refresh() }
        if (-not $app.HasExited) {
            if ($app.MainWindowHandle -ne 0) {
                if (-not $app.CloseMainWindow() -or -not $app.WaitForExit(15000)) { throw "Fixture app did not close: PID $($app.Id)" }
            } else {
                # Start-Process -WindowStyle Hidden can propagate to the test child.
                # This is our isolated fixture process, with no installation in progress.
                $app.Kill()
                $app.WaitForExit()
            }
        }
    }
    $installed = Get-Content -Encoding UTF8 -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $expectedVersion = if ($scenario -eq 'session') { '0.2.0' } elseif ($scenario -eq 'success') { (Get-Content -Encoding UTF8 -LiteralPath (Join-Path $delivery 'App/portable.manifest.json') -Raw | ConvertFrom-Json).Version } else { '0.0.9' }
    if ($installed.Version -ne $expectedVersion) { throw 'Wrong installed manifest version.' }
    foreach ($entry in $installed.Files.PSObject.Properties) {
        if ((Get-FileHash -LiteralPath (Join-Path (Join-Path $root 'App') $entry.Name)).Hash -ne $entry.Value) { throw "Installed file mismatch: $($entry.Name)" }
    }
    if ((Get-Content -LiteralPath (Join-Path $root 'Data/preserve.txt') -Raw).Trim() -ne 'user-data') { throw 'Data changed.' }
    if ((Get-Content -LiteralPath (Join-Path $root 'OfflinePayloads/preserve.txt') -Raw).Trim() -ne 'payload-fixture') { throw 'Payload changed.' }
    if ((Get-Content -LiteralPath (Join-Path $root 'Data/settings.json') -Raw).Trim() -ne '{"Theme":"Dark"}') { throw 'Settings changed.' }
    if (-not (Get-Content -LiteralPath (Join-Path $root 'Data/diagnostic.log') -Raw).StartsWith('log-fixture')) { throw 'Old log lost.' }
    if ($scenario -ne 'rollback' -and ((Test-Path -LiteralPath $candidate) -or (Test-Path -LiteralPath (Join-Path $root '.updates/update-error.log')))) { throw 'Successful update left stale candidate/error.' }
    $results += [pscustomobject]@{ Scenario=$scenario; ExitCode=$process.ExitCode; Version=$installed.Version; UserDataPreserved=$true; FilesVerified=@($installed.Files.PSObject.Properties).Count; Root=$root }
    Write-Output "PASS updater $scenario"
}
$results | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $testRoot 'results.json')
Write-Output "Evidence: $testRoot/results.json"
